using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Application : MonoBehaviour {

    public enum OperatingPlatforms
    {
        Windows,
        OSX,
        Other
    }

    /// <summary>
    /// Returns what platform the app is currently running on
    /// </summary>
    /// <returns></returns>
    public static OperatingPlatforms GetOperatingPlatform()
    {
        switch (Application.platform)
        {
            //Is Windows
            case RuntimePlatform.WindowsEditor:
                return OperatingPlatforms.Windows;
            case RuntimePlatform.WindowsPlayer:
                return OperatingPlatforms.Windows;
            //Is OSX
            case RuntimePlatform.OSXEditor:
                return OperatingPlatforms.OSX;
            case RuntimePlatform.OSXPlayer:
                return OperatingPlatforms.OSX;
            //Something Else
            default:
                return OperatingPlatforms.Other;
        }
    }
}
