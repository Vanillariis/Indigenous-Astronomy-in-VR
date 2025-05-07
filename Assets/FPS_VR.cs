using UnityEngine;

public class FPS_VR : MonoBehaviour
{
    [Tooltip("How often to update the displayed FPS (in seconds).")]
    public float updateInterval = 0.5f;

    private float accum = 0f;  // FPS accumulated over the interval
    private int frames = 0;   // Frames drawn over the interval
    private float timeleft;      // Left time for current interval
    private float fps;           // Current FPS

    void Start()
    {
        if (updateInterval <= 0f)
            updateInterval = 0.5f;
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.unscaledDeltaTime;
        accum += Time.unscaledDeltaTime > 0 ? (1.0f / Time.unscaledDeltaTime) : 0;
        frames++;

        // Interval ended — update GUI text and start new interval
        if (timeleft <= 0.0f)
        {
            fps = accum / frames;
            timeleft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }

    void OnGUI()
    {
        // Reserve a small style
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 80;
        style.normal.textColor = Color.white;

        // Prepare the text
        string text = fps.ToString("F1") + " FPS";

        // Measure text size
        Vector2 size = style.CalcSize(new GUIContent(text));

        // Draw in top right corner (10px from top/right)
        Rect rect = new Rect(
            Screen.width - size.x - 10,
            10,
            size.x,
            size.y
        );
        GUI.Label(rect, text, style);
    }
}
