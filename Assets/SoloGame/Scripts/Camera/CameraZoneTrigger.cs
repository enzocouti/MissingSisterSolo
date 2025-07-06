using UnityEngine;


//Trigger placed in level to lock camera and begin a wave

public class CameraZoneTrigger : MonoBehaviour
{
    public float lockx; //Where to lock camera
    public int waveIndex;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            SideCameraFollow cam = Camera.main.GetComponent<SideCameraFollow>();
            if (cam != null)
                cam.LockToX(lockx);

            CombatZoneManager.Instance?.TriggerWave(waveIndex);
            Destroy(gameObject); //Remove trigger
        }
    }
}
