using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // For scene change tracking

public class FPSLogger : MonoBehaviour
{
    // FPS Stuff
    
    public float logInterval = 1f; // Time interval between each FPS log (in seconds)
    private float timer = 0f; // Accumulates time for FPS calculation
    private int frameCount = 0; // Counts frames per log interval

    private List<string> logLines = new List<string>(); // Holds all the log lines for CSV export
    private string filePath; //Used to print the filePath once the file has been saved

    private static FPSLogger instance; // Singleton to ensure only one logger instance across scenes
    
    // Saving the file after 30 seconds
    
    public GameObject saveLogTriggerObject; //
    public float saveDelay = 30f;

    private bool saveCountdownStarted = false;
    private float saveTimer = 0f;
    void Awake()
    {
        // Ensure only one instance of the logger persists across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist this GameObject across scenes

        // Subscribe to scene change events
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void Start()
    {
        // Get shared "Logs" directory path from LoggerUtils
        string directoryPath = LoggerUtils.GetLogDirectory();

        // Create a unique log file with timestamped filename
        filePath = Path.Combine(directoryPath, $"FPS_Log_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");

        // Add CSV header for R compatibility
        logLines.Add("Time (s),FPS,Scene");

        // Log initial scene when the experience starts
        logLines.Add($"0.00,Start,{SceneManager.GetActiveScene().name}");
    }

    void Update()
    {
        frameCount++; // Increment frame counter each frame
        timer += Time.unscaledDeltaTime; // Add delta time (frame time) to the timer

        if (timer >= logInterval)
        {
            // Calculate FPS based on accumulated frames and time
            float fps = frameCount / timer;

            // Log time since level load, FPS, and current scene name
            float timeSinceStart = Time.timeSinceLevelLoad;
            logLines.Add($"{timeSinceStart:F2},{fps:F2},{SceneManager.GetActiveScene().name}");
            
            //Check if the script works in the console
            //Debug.Log($"[FPSLogger] Time: {timeSinceStart:F2}s | FPS: {fps:F2} | Scene: {SceneManager.GetActiveScene().name}");


            // Reset counters after logging
            timer = 0f;
            frameCount = 0;
        }
        
        // Start timer if the object becomes active and timer hasn't already started
        if (saveLogTriggerObject != null && saveLogTriggerObject.activeSelf && !saveCountdownStarted && SceneManager.GetActiveScene().name == "SunsetScene")
        {
            saveCountdownStarted = true;
            saveTimer = 0f;
        }

        // Count down and save when time is up
        if (saveCountdownStarted)
        {
            saveTimer += Time.unscaledDeltaTime;

            if (saveTimer >= saveDelay)
            {
                SaveLog();
                saveCountdownStarted = false; // prevent saving again
            }
        }
    }

    // Called when a scene changes, logs the scene name and time since level load
    void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        float timeSinceStart = Time.timeSinceLevelLoad;
        logLines.Add($"{timeSinceStart:F2},SceneChanged,{newScene.name}");
    }

    // Called when the application is quitting, saves the log to file
    void OnApplicationQuit()
    {
        SaveLog(); // Save the log when the application quits
    }

    // Saves the log to the specified file path
    public void SaveLog()
    {
        // Write all collected log lines to the CSV file
        File.WriteAllLines(filePath, logLines);
        Debug.Log("FPS log saved to: " + filePath);
    }

    void OnDestroy()
    {
        // Unsubscribe from scene change events to avoid memory leaks
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}
