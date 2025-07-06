using UnityEngine;

// Makes the camera follow the player until reach a wave zone

public class SideCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 5f;

    private float lockX = float.NaN;
    private Vector3 offset;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        offset = transform.position - player.position;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 target = player.position + offset;
        target.y = transform.position.y;
        target.z = transform.position.z;

        if (!float.IsNaN(lockX))
        {
            target.x = lockX; // Camera is locked to specific x

        }

        transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
    }

    public void LockToX(float x)
    {
        lockX = x;
    }

    public void Unlock()
    {
        lockX = float.NaN;
    }
}
