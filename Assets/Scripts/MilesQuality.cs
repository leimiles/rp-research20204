using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MilesQuality : MonoBehaviour
{
    GUIStyle qualityInfoStyle;
    string qualityInfo;
    void Start()
    {
        qualityInfoStyle = new GUIStyle();
        qualityInfoStyle.fontSize = 45;
        qualityInfoStyle.normal.textColor = Color.magenta;
        qualityInfo = GetQualityInfo();
    }

    void OnGUI()
    {
        GUILayout.Label(qualityInfo, qualityInfoStyle);
    }

    static string GetQualityInfo()
    {
        int level = QualitySettings.GetQualityLevel();
        switch (level)
        {
            case 0:
                return "mobile low";
            case 1:
                return "mobile high";
            case 2:
                return "miles mode";
            default:
                return "unknown";

        }
    }
}
