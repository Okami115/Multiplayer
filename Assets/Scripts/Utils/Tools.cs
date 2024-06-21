using OkamiNet.Utils;
using UnityEngine;

public class Tools : MonoBehaviour
{
    private void OnEnable()
    {
        UtilsTools.LOG += DebugLogs;
    }

    private void OnDestroy()
    {
        UtilsTools.LOG -= DebugLogs;
    }

    private void DebugLogs(string msg)
    {
        Debug.Log(msg);
    }
}
