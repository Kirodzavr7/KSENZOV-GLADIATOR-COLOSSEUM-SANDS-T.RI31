using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
        GameInput.Instance.OnBlockStarted += GameInput_OnBlockStarted;
        GameInput.Instance.OnBlockCanceled += GameInput_OnBlockCanceled;
    }

    private bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }

        return false;
    }
    
    private void GameInput_OnBlockStarted(object sender, System.EventArgs e)
    {
        if (HasParameter("block", _animator))
        {
            _animator.SetBool("block", true);
        }
    }

    private void GameInput_OnBlockCanceled(object sender, System.EventArgs e)
    {
        if (HasParameter("block", _animator))
        {
            _animator.SetBool("block", false);
        }
    }

    private void GameInput_OnPlayerAttack(object sender, System.EventArgs e)
    {
        if (HasParameter("attack", _animator))
        {
            _animator.SetTrigger("attack");
        }
    }
    private void OnDestroy()
    {
        {
            GameInput.Instance.OnPlayerAttack -= GameInput_OnPlayerAttack;
            GameInput.Instance.OnBlockStarted -= GameInput_OnBlockStarted;
            GameInput.Instance.OnBlockCanceled -= GameInput_OnBlockCanceled;
        }
    }
}