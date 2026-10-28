using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private TargetLock _targetLock;
    [SerializeField] private float _mouseSensivity = 0.2f;
    [SerializeField] private float _distance = 5f;
    [SerializeField] private Vector2 _pichLimits = new Vector2(-20f, 60f);
    [SerializeField] private float _rotationSpeed = 10f;
    
    private float _yaw = 0f;
    private float _pitch = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (_targetLock == null && _target != null)
        {
            _targetLock = _target.GetComponent<TargetLock>();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
      if (_target == null) return;
      if (Time.timeScale == 0f) return;
      
      if (_targetLock != null && _targetLock.IsLocked)
      {
          Transform currentTarget = _targetLock.CurrentTarget;

          // РЕЖИМ ЛОКА: камера смотрит на врага
          Vector3 lookDir = (currentTarget.position - _target.position).normalized;
          Vector3 targetCameraPosition = _target.position - (lookDir * _distance) + Vector3.up * 2f;
            
          transform.position = Vector3.Lerp(transform.position, targetCameraPosition, _rotationSpeed * Time.deltaTime);

          Quaternion targetRotation = Quaternion.LookRotation(currentTarget.position - transform.position);
          transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
      }
      else
      {
          Vector3 mouseInput = GameInput.Instance.GetMouseLookVector();

          _yaw += mouseInput.x * _mouseSensivity;
          _pitch -= mouseInput.y * _mouseSensivity;
          _pitch = Mathf.Clamp(_pitch, _pichLimits.x, _pichLimits.y);

          Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
          Vector3 targetPosition = _target.position - (rotation * Vector3.forward * _distance);

          transform.rotation = rotation;
          transform.position = targetPosition;
      }
    }
}
