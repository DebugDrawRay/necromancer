using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Vector3 anchorOffset;
    private Transform anchor;
    private Vector3 anchorPosition
    {
        get
        {
            return anchor.position + anchorOffset;
        }
    }
    [Range(0, 1)]
    public float cameraTracking;

    void Start()
    {
        anchor = Necromancer.instance.transform;
    }

    void FixedUpdate()
    {
        if (anchor != null)
        {
            transform.position = Vector3.Lerp(transform.position, anchorPosition, cameraTracking);
        }
    }
}
