using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HandTrackingLogger : MonoBehaviour
{
    // Handtracking stuff
    
    private static HandTrackingLogger instance;
    
    public Transform leftHand;
    public Transform rightHand;

    public float logInterval = 1f; // Log every second
    private float timer = 0f;

    private List<string> logLines = new List<string>();
    private string filePath;
    
    // Saving the file after 30 seconds
    
    public GameObject saveLogTriggerObject; //
    public float saveDelay = 30f;

    private bool saveCountdownStarted = false;
    private float saveTimer = 0f;

    void Awake()
    {
        // If an instance already exists and it's not this one, destroy this duplicate
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set this as the singleton instance
        instance = this;

        // Prevent this GameObject from being destroyed between scene loads
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Get shared "Logs" directory path from LoggerUtils
        string directoryPath = LoggerUtils.GetLogDirectory();

        // Generate a unique filename using date and time
        filePath = Path.Combine(directoryPath, $"Hand_Log_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");

        // Write CSV header
        logLines.Add("Time (s),Scene,LeftPosX,LeftPosY,LeftPosZ,LeftRotX,LeftRotY,LeftRotZ,LeftRotW,RightPosX,RightPosY,RightPosZ,RightRotX,RightRotY,RightRotZ,RightRotW");
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= logInterval)
        {
            float time = Time.timeSinceLevelLoad;
            string scene = SceneManager.GetActiveScene().name;

            Vector3 lPos = leftHand != null ? leftHand.position : Vector3.zero;
            Quaternion lRot = leftHand != null ? leftHand.rotation : Quaternion.identity;

            Vector3 rPos = rightHand != null ? rightHand.position : Vector3.zero;
            Quaternion rRot = rightHand != null ? rightHand.rotation : Quaternion.identity;

            string line = string.Format("{0:F2},{1},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8:F4},{9:F4},{10:F4},{11:F4},{12:F4},{13:F4},{14:F4},{15:F4}",
                time, scene,
                lPos.x, lPos.y, lPos.z, lRot.x, lRot.y, lRot.z, lRot.w,
                rPos.x, rPos.y, rPos.z, rRot.x, rRot.y, rRot.z, rRot.w);

            logLines.Add(line);

            timer = 0f;
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

    void OnApplicationQuit()
    {
        SaveLog(); // Save when app quits
    }

    void OnDestroy()
    {
        // Only save if this instance is the active singleton (avoids double saving from destroyed duplicates)
        if (instance == this)
        {
            SaveLog();
        }
    }

    void SaveLog()
    {
        File.WriteAllLines(filePath, logLines);
        Debug.Log("Hand tracking log saved to: " + filePath);
    }
}
