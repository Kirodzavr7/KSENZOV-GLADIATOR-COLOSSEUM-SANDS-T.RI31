using UnityEngine;

public class TargetLock : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _lockCheckRadius = 10f;

    private Transform _currentTarget = null;

    public Transform CurrentTarget => _currentTarget;
    public bool IsLocked => _currentTarget != null;

    private void Start()
    {
        // Подписываемся на событие нажатия кнопки захвата из GameInput
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnLockAction += GameInput_OnLockAction;
        }
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся при уничтожении объекта
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnLockAction -= GameInput_OnLockAction;
        }
    }

    private void Update()
    {
        // Если цель ушла слишком далеко — сбрасываем захват
        if (_currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
            if (distanceToTarget > _lockCheckRadius * 1.5f)
            {
                UnlockTarget();
            }
        }
    }

    private void GameInput_OnLockAction(object sender, System.EventArgs e)
    {
        if (_currentTarget == null)
        {
            FindNearestTarget();
        }
        else
        {
            UnlockTarget();
        }
    }

    private void FindNearestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _lockCheckRadius, _targetLayer);

        Transform closestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = col.transform;
            }
        }

        if (closestTarget != null)
        {
            _currentTarget = closestTarget;
        }
    }

    public void UnlockTarget()
    {
        _currentTarget = null;
    }
}
