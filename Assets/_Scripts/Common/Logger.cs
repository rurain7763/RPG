using UnityEngine;

public static class Logger
{
    [System.Diagnostics.Conditional("DEV_BUILD")]
    public static void Info(string message)
    {
        Debug.LogFormat("{0} : {1}", System.DateTime.Now.ToString("HH:mm:ss.fff"), message);
    }

    [System.Diagnostics.Conditional("DEV_BUILD")]
    public static void Warn(string message)
    {
        Debug.LogWarningFormat("{0} : {1}", System.DateTime.Now.ToString("HH:mm:ss.fff"), message);
    }

    public static void Error(string message)
    {
        Debug.LogErrorFormat("{0} : {1}", System.DateTime.Now.ToString("HH:mm:ss.fff"), message);
    }
}
