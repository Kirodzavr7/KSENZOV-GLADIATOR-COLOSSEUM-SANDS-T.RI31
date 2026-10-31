using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions _playerInputActions;
    private bool _isBlocking;

    public event EventHandler OnLockAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnPlayerAttack;
    public event EventHandler OnBlockStarted;
    public event EventHandler OnBlockCanceled;

    private void Awake()
    {
        Instance = this;

        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();

        _playerInputActions.Player.Pause.performed += Pause_Performed;
        _playerInputActions.Player.LockOn.performed += Lock_Performed;
        _playerInputActions.Combat.Attack.started += PlayerAttack_started;
        _playerInputActions.Combat.Block.performed += Block_Performed;
    }

    private void Pause_Performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Lock_Performed(InputAction.CallbackContext context)
    {
        OnLockAction?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerAttack_started(InputAction.CallbackContext context)
    {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    private void Block_Performed(InputAction.CallbackContext context)
    {
        _isBlocking = !_isBlocking;

        if (_isBlocking)
            OnBlockStarted?.Invoke(this, EventArgs.Empty);
        else
            OnBlockCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        if (_playerInputActions == null)
            return;

        _playerInputActions.Player.LockOn.performed -= Lock_Performed;
        _playerInputActions.Player.Pause.performed -= Pause_Performed;
        _playerInputActions.Combat.Attack.started -= PlayerAttack_started;
        _playerInputActions.Combat.Block.performed -= Block_Performed;

        _playerInputActions.Disable();
    }

    public Vector2 GetMovementVector()
    {
        Vector3 inputVector =
            _playerInputActions.Player.Move.ReadValue<Vector3>();

        return new Vector2(inputVector.x, inputVector.y).normalized;
    }

    public Vector2 GetMouseLookVector()
    {
        return _playerInputActions.Player.Look.ReadValue<Vector2>();
    }
}
