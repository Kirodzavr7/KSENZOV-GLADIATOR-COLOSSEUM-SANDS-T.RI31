using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAxeAnimator : MonoBehaviour
{
    [SerializeField] private SimpleFighter fighter;
    [SerializeField] private string attackTrigger = "attack";

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (fighter == null)
            fighter = GetComponentInParent<SimpleFighter>();

        if (fighter == null)
            Debug.LogWarning("EnemyAxeAnimator: назначь SimpleFighter владельца в поле Fighter.", this);
        if (_animator.runtimeAnimatorController == null)
            Debug.LogWarning("EnemyAxeAnimator: назначь Animator Controller на Animator этого оружия.", this);
        else if (!HasParameter(attackTrigger, AnimatorControllerParameterType.Trigger))
            Debug.LogWarning($"EnemyAxeAnimator: в Controller нет Trigger '{attackTrigger}'. Проверь регистр имени.", this);
    }

    private void OnEnable()
    {
        CoreCombatEvents.AttackStarted += HandleAttackStarted;
    }

    private void OnDisable()
    {
        CoreCombatEvents.AttackStarted -= HandleAttackStarted;
    }

    private void HandleAttackStarted(SimpleFighter attacker)
    {
        if (fighter == null || attacker != fighter) return;
        if (_animator == null || _animator.runtimeAnimatorController == null) return;
        if (HasParameter(attackTrigger, AnimatorControllerParameterType.Trigger))
            _animator.SetTrigger(attackTrigger);
    }

    private bool HasParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (string.IsNullOrEmpty(parameterName) || _animator == null ||
            _animator.runtimeAnimatorController == null)
            return false;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }
}