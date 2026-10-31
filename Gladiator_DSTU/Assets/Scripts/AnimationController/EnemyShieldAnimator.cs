using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyShieldAnimator : MonoBehaviour
{
    [SerializeField] private SimpleFighter fighter;
    [SerializeField] private string blockingBool = "EnemyBlock";

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (fighter == null)
            fighter = GetComponentInParent<SimpleFighter>();
    }

    private void OnEnable()
    {
        CoreCombatEvents.BlockChanged += HandleBlockChanged;
    }

    private void OnDisable()
    {
        CoreCombatEvents.BlockChanged -= HandleBlockChanged;
    }

    private void HandleBlockChanged(SimpleFighter changedFighter)
    {
        if (fighter == null || changedFighter != fighter) return;
        if (_animator == null || _animator.runtimeAnimatorController == null) return;
        if (HasParameter(blockingBool, AnimatorControllerParameterType.Bool))
            _animator.SetBool(blockingBool, fighter.IsBlocking);
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