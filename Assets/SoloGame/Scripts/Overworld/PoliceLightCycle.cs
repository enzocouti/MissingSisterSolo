using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class PoliceLightCycle : MonoBehaviour
{
    [Header("Assign your Red and Blue 2D Lights")]
    public Light2D redLight;
    public Light2D blueLight;

    [Header("Settings")]
    public float cycleSpeed = 2f; 
    public float minIntensity = 0.2f;
    public float maxIntensity = 1f;

    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime * cycleSpeed;
        
        float t = Mathf.PingPong(timer, 1f);

        
        blueLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        redLight.intensity = Mathf.Lerp(maxIntensity, minIntensity, t);
    }
}
