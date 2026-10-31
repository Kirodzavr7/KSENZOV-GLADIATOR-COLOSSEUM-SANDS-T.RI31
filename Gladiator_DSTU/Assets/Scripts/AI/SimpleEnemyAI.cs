using UnityEngine;

[RequireComponent(typeof(SimpleFighter))]
public class SimpleEnemyAI : MonoBehaviour
{
    [SerializeField] private SimpleFighter target;
    [SerializeField, Min(0f)] private float aggroRange = 20f;
    [SerializeField, Min(0f)] private float stopDistance = 1.25f;
    [SerializeField, Min(0f)] private float blockRange = 3.5f;
    [SerializeField, Range(0f, 1f)] private float blockChance = 0.35f;
    [SerializeField, Min(0f)] private float blockCooldown = 2.5f;
    [SerializeField, Min(0f)] private float maxBlockDuration = 0.7f;
    [SerializeField, Min(0f)] private float turnSpeed = 6f;
    [SerializeField, Min(0.05f)] private float attackCheckInterval = 0.15f;

    private SimpleFighter _fighter;
    private float _nextCheck;
    private float _nextBlockAllowedTime;
    private float _blockEndTime;
    private bool _blockAttemptedForCurrentAttack;
    private bool _isAIBlocking;

    private void Awake()
    {
        _fighter = GetComponent<SimpleFighter>();
    }

    private void Start()
    {
        FindTargetIfNeeded();
    }

    private void FixedUpdate()
    {
        if (_fighter.IsDead) return;
        // Не меняем направление посреди атаки: это сохраняет сектор удара и не дёргает модель.
        if (_fighter.State == CoreFighterState.Attacking) return;

        FindTargetIfNeeded();
        if (target == null || target.IsDead)
        {
            StopAIBlocking();
            _blockAttemptedForCurrentAttack = false;
            _fighter.Move(Vector3.zero);
            return;
        }

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        if (distance > aggroRange)
        {
            StopAIBlocking();
            _blockAttemptedForCurrentAttack = false;
            _fighter.Move(Vector3.zero);
            return;
        }

        if (distance > 0.01f)
            _fighter.Face(toTarget, turnSpeed);

        bool targetAttacking = target.State == CoreFighterState.Attacking;
        if (!targetAttacking)
            _blockAttemptedForCurrentAttack = false;

        if (_isAIBlocking && (!targetAttacking || Time.time >= _blockEndTime ||
                              distance > blockRange || !_fighter.IsBlocking))
            StopAIBlocking();

        if (_isAIBlocking)
        {
            _fighter.Move(Vector3.zero, turnSpeed);
            return;
        }

        // За одну атаку игрока враг может сделать не более одной попытки блока.
        // Вероятность, cooldown и ограничение длительности не дают щиту спамить.
        if (targetAttacking && !_blockAttemptedForCurrentAttack && distance <= blockRange)
        {
            _blockAttemptedForCurrentAttack = true;
            bool canBlockNow = Time.time >= _nextBlockAllowedTime &&
                               Random.value <= blockChance && _fighter.CurrentStamina > 0f;
            if (canBlockNow)
            {
                _fighter.SetBlocking(true);
                if (_fighter.IsBlocking)
                {
                    _isAIBlocking = true;
                    _blockEndTime = Time.time + maxBlockDuration;
                    _nextBlockAllowedTime = Time.time + blockCooldown;
                    _fighter.Move(Vector3.zero, turnSpeed);
                    return;
                }
            }
        }

        float desiredStop = Mathf.Max(stopDistance, _fighter.AttackRange * 0.8f);
        bool targetCanBeHit = _fighter.CanHitTarget(target);
        if (!targetCanBeHit && distance > desiredStop)
        {
            _fighter.Move(toTarget.normalized, turnSpeed);
        }
        else
        {
            _fighter.Move(Vector3.zero, turnSpeed);
            if (targetCanBeHit && Time.time >= _nextCheck && _fighter.State == CoreFighterState.Free)
            {
                _nextCheck = Time.time + attackCheckInterval;
                _fighter.TryAttack();
            }
        }
    }

    private void StopAIBlocking()
    {
        if (!_isAIBlocking) return;
        _fighter.SetBlocking(false);
        _isAIBlocking = false;
    }

    private void FindTargetIfNeeded()
    {
        if (target != null) return;
        SimpleFighter[] fighters = FindObjectsByType<SimpleFighter>(FindObjectsSortMode.None);
        foreach (SimpleFighter candidate in fighters)
        {
            if (candidate != null && candidate != _fighter && candidate.Team != _fighter.Team)
            {
                target = candidate;
                break;
            }
        }
    }
}
