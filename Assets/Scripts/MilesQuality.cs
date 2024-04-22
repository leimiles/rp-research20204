using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
// use new unity input system
using UnityEngine.InputSystem;

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

    void Update()
    {
        qualityInfo = GetQualityInfo();
    }

    void OnGUI()
    {
        //GUILayout.Label(qualityInfo, qualityInfoStyle);
    }

    public void SetLevel0(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            QualitySettings.SetQualityLevel(0);
        }
    }
    public void SetLevel1(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            QualitySettings.SetQualityLevel(1);
        }
    }
    public void SetLevel2(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            QualitySettings.SetQualityLevel(2);
        }
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
