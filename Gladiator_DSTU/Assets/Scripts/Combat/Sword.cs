using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer; // Укажем слой врага в инспекторе

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, входит ли объект в нужный слой через побитовое сравнение
        if ((enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Уничтожаем объект врага
            Destroy(other.gameObject);
        }
    }
}