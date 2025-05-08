using UnityEngine;

public class SmoothFireLightFlicker : MonoBehaviour
{
    public Light fireLight;
    public float baseIntensity = 3.5f;
    public float intensityVariation = 1.0f;
    public float flickerSpeed = 1.0f;

    private float noiseOffset;

    void Start()
    {
        if (fireLight == null)
            fireLight = GetComponent<Light>();

        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        fireLight.intensity = baseIntensity + (noise - 0.5f) * intensityVariation * 2f;
    }
}


