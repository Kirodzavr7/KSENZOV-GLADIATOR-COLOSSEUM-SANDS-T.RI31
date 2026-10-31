using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private LayerMask enemyLayer; // Слой врага

    private void OnTriggerStay(Collider other)
    {
        // Проверяем, что объект находится на слое врага
        if ((enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Проверяем, поднят ли блок в аниматоре
            bool isBlocking = playerAnimator.GetBool("block");

            if (isBlocking)
            {
                Debug.Log("Атака врага заблокирована щитом!");
                // Здесь можно заблокировать урон или оттолкнуть врага
            }
        }
    }
}
