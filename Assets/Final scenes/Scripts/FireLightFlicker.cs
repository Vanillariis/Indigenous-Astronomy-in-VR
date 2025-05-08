using UnityEngine;

public class FireLightFlicker : MonoBehaviour
{
    public Light fireLight;
    public float minIntensity = 2.5f;
    public float maxIntensity = 4.5f;
    public float flickerSpeed = 0.1f;

    private float timer;

    void Start()
    {
        if (fireLight == null)
            fireLight = GetComponent<Light>();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            fireLight.intensity = Random.Range(minIntensity, maxIntensity);
            timer = flickerSpeed * Random.Range(0.5f, 1.5f);
        }
    }
}

