using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool lockVerticalAxis = true;

    private void Awake()
    {
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 toCamera = targetCamera.transform.position - transform.position;
        if (lockVerticalAxis)
        {
            toCamera.y = 0f;
        }

        if (toCamera.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }

    public void SetCamera(Camera camera)
    {
        targetCamera = camera;
    }
}
