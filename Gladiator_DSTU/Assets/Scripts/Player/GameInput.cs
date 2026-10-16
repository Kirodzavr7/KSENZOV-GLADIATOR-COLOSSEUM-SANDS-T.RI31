using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
        
    private PlayerInputActions _playerInputActions;
    
    private void Awake()
    {
        Instance = this;
        
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();
    }
    
    public Vector3 GetMovementVector()
    {
        Vector3 inputVector = _playerInputActions.Player.Move.ReadValue<Vector3>();
        inputVector = inputVector.normalized;
        return inputVector;
    }
}
