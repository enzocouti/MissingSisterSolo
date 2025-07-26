using UnityEngine;
using System.Collections;


public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;

    private Vector3 defaultPos;
    private void Awake()
    {
        if (!Instance) Instance = this;
        defaultPos = transform.localPosition;
    }

    public static void Shake(float amount, float duration)
    {
        if (Instance)
            Instance.StartCoroutine(Instance.ShakeCoroutine(amount, duration));
    }

   private IEnumerator ShakeCoroutine(float amt, float dur)
{
    float timer = 0f;
    while (timer < dur)
    {
        Vector3 offset = (Vector3)Random.insideUnitCircle * amt;

        
        SideCameraFollow.Instance?.SetShakeOffset(offset);

        timer += Time.unscaledDeltaTime;
        yield return null;
    }

    SideCameraFollow.Instance?.SetShakeOffset(Vector3.zero);
}

}
