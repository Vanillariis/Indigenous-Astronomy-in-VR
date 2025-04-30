using System.IO;
using UnityEngine;

public static class LoggerUtils
{
    // Returns the shared log directory path (and ensures it's created)
    public static string GetLogDirectory()
    {
        string path = Path.Combine(Application.persistentDataPath, "Logs");
        Directory.CreateDirectory(path);
        return path;
    }
}