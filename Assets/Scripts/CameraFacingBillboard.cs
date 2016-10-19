using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
    void Update()
    {
        Quaternion cam = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        transform.LookAt(transform.position + cam * Vector3.forward,
            cam * Vector3.up);
    }
}