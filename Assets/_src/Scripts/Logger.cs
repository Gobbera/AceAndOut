using UnityEngine;

public static class Logger
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        switch (level)
        {
            case LogLevel.Info:
                Debug.Log($"[INFO] {message}");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"[WARNING] {message}");
                break;
            case LogLevel.Error:
                Debug.LogError($"[ERROR] {message}");
                break;
        }
    }
}
