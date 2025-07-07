using UnityEngine;

public class SideCameraFollow : MonoBehaviour
{
    public static SideCameraFollow Instance { get; private set; }

    public Transform target;
    public float smoothSpeed = 5f;
    private bool isLocked = false;
    private Vector3 lockedPosition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void LateUpdate()
    {
        if (isLocked)
        {
            transform.position = Vector3.Lerp(transform.position, lockedPosition, smoothSpeed * Time.deltaTime);
        }
        else if (target != null)
        {
            Vector3 desiredPosition = new Vector3(target.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }

    public void LockCamera(Vector3 atPosition)
    {
        isLocked = true;
        lockedPosition = new Vector3(atPosition.x, transform.position.y, transform.position.z);
    }

    public void UnlockCamera()
    {
        isLocked = false;
    }
}
