using UnityEngine;

public class SideCameraFollow : MonoBehaviour
{
    public static SideCameraFollow Instance;

    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 5f;

    private bool isLocked = false;
    private float lockedX;

    private void Awake() => Instance = this;

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 target;

        if (isLocked)
            target = new Vector3(lockedX, transform.position.y, transform.position.z);
        else
            target = new Vector3(player.position.x, transform.position.y, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
    }

    public void LockToX(float x)
    {
        isLocked = true;
        lockedX = x;
    }

    public void Unlock()
    {
        isLocked = false;
    }
}
