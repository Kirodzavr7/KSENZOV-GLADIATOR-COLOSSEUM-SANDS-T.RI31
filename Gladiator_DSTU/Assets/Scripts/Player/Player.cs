using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    
    [SerializeField] private float _movingSpeed = 8f;
    [SerializeField] private float _rotationSpeed = 8f;
    
    private Rigidbody _rb;
    private PlayerAnimator _playerAnimator; // Ссылка на компонент анимаций

    private void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody>();
        _playerAnimator = GetComponent<PlayerAnimator>(); // Получаем аниматор
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVector();
        Vector3 inputDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (inputDir != Vector3.zero)
        {
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = cameraForward * inputVector.y + cameraRight * inputVector.x;

            // Плавный поворот
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            
            // Движение
            _rb.MovePosition(_rb.position + moveDirection * (_movingSpeed * Time.fixedDeltaTime));
        }
    }
}