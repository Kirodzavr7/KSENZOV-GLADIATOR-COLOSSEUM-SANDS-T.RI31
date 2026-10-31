using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFighter : MonoBehaviour
{
    [Header("Роль и характеристики")]
    [SerializeField] private FighterTeam team = FighterTeam.Enemy;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float defense = 0f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField, Range(0f, 180f)] private float blockArc = 120f;

    [Header("Выносливость")]
    [SerializeField, Min(0f)] private float maxStamina = 100f;
    [SerializeField, Min(0f)] private float attackStaminaCost = 20f;
    [SerializeField, Min(0f)] private float blockStaminaDrainPerSecond = 12f;
    [SerializeField, Min(0f)] private float staminaRegenPerSecond = 18f;
    [SerializeField, Min(0f)] private float staminaRegenDelay = 0.8f;

    [Header("Время атаки")]
    [SerializeField, Min(0f)] private float attackWindup = 0.2f;
    [SerializeField, Min(0f)] private float attackRecovery = 0.35f;
    [SerializeField] private LayerMask targetLayers = ~0;

    [Header("Animator Controller — имена параметров")]
    [SerializeField] private string runningBool = "IsRunning";
    [SerializeField] private string blockingBool = "IsBlocking";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string deathParameter = "IsDie";
    [SerializeField] private bool deathParameterIsBool = true;

    private Rigidbody _body;
    private Animator _animator;
    private Coroutine _attackCoroutine;
    private float _nextAttackTime;
    private float _lastStaminaUseTime;
    private bool _isBlocking;
    private bool _isRunning;

    public FighterTeam Team => team;
    public CoreFighterState State { get; private set; } = CoreFighterState.Free;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;
    public float MoveSpeed => moveSpeed;
    public float AttackRange => attackRange;
    public bool IsDead => State == CoreFighterState.Dead;
    public bool IsBlocking => State == CoreFighterState.Blocking;
    public Rigidbody Body => _body;

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _body.interpolation = RigidbodyInterpolation.Interpolate;
        CurrentHealth = Mathf.Max(1f, maxHealth);
        CurrentStamina = Mathf.Max(0f, maxStamina);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        attackDamage = Mathf.Max(0f, attackDamage);
        defense = Mathf.Max(0f, defense);
        attackRange = Mathf.Max(0.1f, attackRange);
        attackCooldown = Mathf.Max(0f, attackCooldown);
        maxStamina = Mathf.Max(0f, maxStamina);
        attackStaminaCost = Mathf.Max(0f, attackStaminaCost);
        blockStaminaDrainPerSecond = Mathf.Max(0f, blockStaminaDrainPerSecond);
        staminaRegenPerSecond = Mathf.Max(0f, staminaRegenPerSecond);
        staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);
    }

    public void Move(Vector3 direction, float turnSpeed = 10f)
    {
        if (IsDead || State == CoreFighterState.Attacking) return;
        direction.y = 0f;
        if (direction.sqrMagnitude > 1f) direction.Normalize();
        _isRunning = direction.sqrMagnitude > 0.001f;
        if (_isRunning) Face(direction, turnSpeed);

        Vector3 step = direction * (moveSpeed * Time.fixedDeltaTime);
        _body.MovePosition(_body.position + step);
    }

    public void Face(Vector3 direction, float turnSpeed = 10f)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f || IsDead) return;
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion smoothRotation = Quaternion.Slerp(
            _body.rotation,
            targetRotation,
            Mathf.Max(0f, turnSpeed) * Time.fixedDeltaTime);
        _body.MoveRotation(smoothRotation);
    }

    /// <summary>Проверяет перед атакой ту же дистанцию и передний сектор, что использует hit scan.</summary>
    public bool CanHitTarget(SimpleFighter target)
    {
        if (target == null || target == this || target.IsDead || target.Team == team) return false;

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f || Vector3.Angle(transform.forward, toTarget) > 65f)
            return false;

        Vector3 attackOrigin = transform.position + Vector3.up * 0.8f;
        Collider[] targetColliders = target.GetComponentsInChildren<Collider>();
        foreach (Collider targetCollider in targetColliders)
        {
            if (targetCollider == null || !targetCollider.enabled || targetCollider.isTrigger) continue;
            if ((targetLayers.value & (1 << targetCollider.gameObject.layer)) == 0) continue;
            Vector3 closestPoint = targetCollider.ClosestPoint(attackOrigin);
            if (Vector3.Distance(attackOrigin, closestPoint) <= attackRange)
                return true;
        }

        // Поддержка бойца без собственного Collider (например, Collider на отдельном hitbox-объекте).
        return targetColliders.Length == 0 && toTarget.magnitude <= attackRange;
    }

    public bool TryAttack()
    {
        if (State != CoreFighterState.Free || Time.time < _nextAttackTime) return false;
        if (!SpendStamina(attackStaminaCost))
        {
            CoreCombatEvents.RaiseStaminaFailed(this);
            return false;
        }
        _nextAttackTime = Time.time + attackCooldown;
        _attackCoroutine = StartCoroutine(AttackRoutine());
        return true;
    }

    private IEnumerator AttackRoutine()
    {
        State = CoreFighterState.Attacking;
        _isRunning = false;
        SetAnimatorBool(runningBool, false);
        SetAnimatorTrigger(attackTrigger);
        CoreCombatEvents.RaiseAttackStarted(this);

        if (attackWindup > 0f)
            yield return new WaitForSeconds(attackWindup);

        ApplyAttackHit();

        if (attackRecovery > 0f)
            yield return new WaitForSeconds(attackRecovery);

        if (!IsDead) State = CoreFighterState.Free;
        _attackCoroutine = null;
    }

    private void ApplyAttackHit()
    {
        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Collider[] hits = Physics.OverlapSphere(origin, attackRange, targetLayers, QueryTriggerInteraction.Ignore);
        SimpleFighter closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            SimpleFighter target = hit.GetComponentInParent<SimpleFighter>();
            if (target == null || target == this || target.Team == team || target.IsDead) continue;

            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) continue;
            if (Vector3.Angle(transform.forward, toTarget) > 65f) continue;

            float distance = toTarget.sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        if (closestTarget == null) return;

        Vector3 direction = closestTarget.transform.position - transform.position;
        direction.y = 0f;
        direction.Normalize();
        CoreHitInfo info = new CoreHitInfo
        {
            attacker = this,
            target = closestTarget,
            damage = attackDamage,
            direction = direction,
            point = closestTarget.transform.position + Vector3.up * 0.8f,
            result = CoreHitResult.Ignored
        };
        CoreCombatEvents.RaiseHit(closestTarget.ReceiveHit(info));
    }

    public CoreHitInfo ReceiveHit(CoreHitInfo hit)
    {
        hit.target = this;
        if (IsDead)
        {
            hit.result = CoreHitResult.Ignored;
            return hit;
        }

        Vector3 fromAttacker = hit.attacker != null
            ? hit.attacker.transform.position - transform.position
            : -hit.direction;
        fromAttacker.y = 0f;
        bool attackerInBlockArc = fromAttacker.sqrMagnitude > 0.0001f &&
            Vector3.Angle(transform.forward, fromAttacker) <= blockArc * 0.5f;

        if (IsBlocking && attackerInBlockArc)
        {
            hit.damage = 0f;
            hit.result = CoreHitResult.Blocked;
            return hit;
        }

        hit.damage = Mathf.Max(0f, hit.damage - defense);
        CurrentHealth = Mathf.Max(0f, CurrentHealth - hit.damage);
        hit.result = CurrentHealth <= 0f ? CoreHitResult.Killed : CoreHitResult.Damaged;

        if (CurrentHealth <= 0f) Die();
        return hit;
    }

    public void ToggleBlock()
    {
        if (IsDead || State == CoreFighterState.Attacking) return;
        SetBlocking(!_isBlocking);
    }

    public void SetBlocking(bool blocking)
    {
        if (IsDead || State == CoreFighterState.Attacking) return;
        if (_isBlocking == blocking) return;
        if (blocking && CurrentStamina <= 0f)
        {
            CoreCombatEvents.RaiseStaminaFailed(this);
            return;
        }
        _isBlocking = blocking;
        State = _isBlocking ? CoreFighterState.Blocking : CoreFighterState.Free;
        SetAnimatorBool(blockingBool, _isBlocking);
        CoreCombatEvents.RaiseBlockChanged(this);
    }

    private bool SpendStamina(float amount)
    {
        if (amount <= 0f) return true;
        if (CurrentStamina < amount) return false;
        CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
        _lastStaminaUseTime = Time.time;
        return true;
    }

    private void Die()
    {
        if (IsDead) return;
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = null;
        bool wasBlocking = _isBlocking;
        State = CoreFighterState.Dead;
        _isBlocking = false;
        _isRunning = false;
        if (wasBlocking) CoreCombatEvents.RaiseBlockChanged(this);
        SetAnimatorBool(blockingBool, false);
        SetAnimatorBool(runningBool, false);
        if (deathParameterIsBool)
            SetAnimatorBool(deathParameter, true);
        else
            SetAnimatorTrigger(deathParameter);
        _body.linearVelocity = Vector3.zero;
        _body.constraints = RigidbodyConstraints.FreezeAll;
        CoreCombatEvents.RaiseDied(this);
    }

    private void Update()
    {
        if (!IsDead)
        {
            if (IsBlocking && blockStaminaDrainPerSecond > 0f)
            {
                CurrentStamina = Mathf.Max(0f, CurrentStamina - blockStaminaDrainPerSecond * Time.deltaTime);
                _lastStaminaUseTime = Time.time;
                if (CurrentStamina <= 0f)
                {
                    SetBlocking(false);
                    CoreCombatEvents.RaiseStaminaFailed(this);
                }
            }
            else if (Time.time - _lastStaminaUseTime >= staminaRegenDelay && CurrentStamina < maxStamina)
            {
                CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + staminaRegenPerSecond * Time.deltaTime);
            }
        }
        SetAnimatorBool(runningBool, _isRunning && State == CoreFighterState.Free);
    }

    private void SetAnimatorBool(string parameter, bool value)
    {
        if (_animator != null && !string.IsNullOrEmpty(parameter) && HasParameter(parameter, AnimatorControllerParameterType.Bool))
            _animator.SetBool(parameter, value);
    }

    private void SetAnimatorTrigger(string parameter)
    {
        if (_animator != null && !string.IsNullOrEmpty(parameter) && HasParameter(parameter, AnimatorControllerParameterType.Trigger))
            _animator.SetTrigger(parameter);
    }

    private bool HasParameter(string parameter, AnimatorControllerParameterType type)
    {
        if (_animator == null || _animator.runtimeAnimatorController == null || string.IsNullOrEmpty(parameter))
            return false;

        foreach (AnimatorControllerParameter item in _animator.parameters)
            if (item.name == parameter && item.type == type) return true;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.8f, attackRange);
    }
}
