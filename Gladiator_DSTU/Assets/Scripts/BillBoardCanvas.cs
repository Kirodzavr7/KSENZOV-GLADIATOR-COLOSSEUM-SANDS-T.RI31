using UnityEngine;

public class BillBoardCanvas : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }
}
