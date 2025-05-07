using TMPro;
using UnityEngine;

public class FPS_VR : MonoBehaviour
{
    [Tooltip("Drag your TextMeshProUGUI here")]
    public TextMeshProUGUI fpsText;

    [Tooltip("Seconds between refreshes")]
    public float updateInterval = 0.5f;

    private float timeLeft;
    private float accum;
    private int frames;

    void Start()
    {
        if (updateInterval <= 0f)
            updateInterval = 0.5f;
        timeLeft = updateInterval;

        if (fpsText == null)
            Debug.LogError("VRFPSCounter: Please assign the fpsText in inspector.");
    }

    void Update()
    {
        timeLeft -= Time.unscaledDeltaTime;
        accum += (Time.unscaledDeltaTime > 0f) ? (1.0f / Time.unscaledDeltaTime) : 0f;
        frames++;

        if (timeLeft <= 0f)
        {
            float fps = accum / frames;
            fpsText.text = fps.ToString("F1") + " FPS";

            // reset for next interval
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
}
