using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float _movingSpeed = 8f;
    [SerializeField] private float _rotationSpeed = 8f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVector();
        Vector3 inputDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (inputDir != Vector3.zero)
        {
            // 2. Учитываем поворот камеры, чтобы "вперед" на клавиатуре было "вперед экрана"
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f; // Убираем наклон по вертикали, чтобы персонаж не взлетал
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Итоговое направление движения с учетом камеры
            Vector3 moveDirection = cameraForward * inputVector.y + cameraRight * inputVector.x;

            // 3. Плавный поворот персонажа в сторону движения
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            
            rb.MovePosition(rb.position + moveDirection * (_movingSpeed * Time.fixedDeltaTime));
        }
        
    }
}
