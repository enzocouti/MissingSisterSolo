using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    public int waveIndex = 0; // 0 = wave1, 1 = wave2, 2 = boss

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        triggered = true;

        SideCameraFollow.Instance?.LockToX(other.transform.position.x);
        CombatZoneManager.Instance?.TriggerWave(waveIndex);

        Destroy(gameObject); // Trigger only once
    }
}
