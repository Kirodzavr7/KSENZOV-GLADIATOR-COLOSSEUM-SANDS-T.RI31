using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
        
    private PlayerInputActions _playerInputActions;
    public event System.EventHandler OnLockAction;
    public event EventHandler OnPauseAction;
    private void Awake()
    {
        Instance = this;
        
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();

        _playerInputActions.Player.Pause.performed += Pause_Performed;
        _playerInputActions.Player.LockOn.performed += Lock_Performed;
    }
    private void Pause_Performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }
    private void Lock_Performed(InputAction.CallbackContext context)
    {
        OnLockAction?.Invoke(this, EventArgs.Empty);
    }
    private void OnDestroy()
    {
        // Отписываемся при уничтожении
        _playerInputActions.Player.LockOn.performed -= Lock_Performed;
    }
    
    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector3>();
        inputVector = inputVector.normalized; return inputVector;
    }

    public Vector2 GetMouseLookVector()
    {
        return _playerInputActions.Player.Look.ReadValue<Vector2>();
    }
    


}
