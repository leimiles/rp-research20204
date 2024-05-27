using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShaderCollectionInfo), true)]
public class ShaderCollectionInfoEditor : Editor
{
    ShaderCollectionInfo shaderCollectionInfo;
    private void OnEnable()
    {
        if (shaderCollectionInfo == null)
        {
            shaderCollectionInfo = target as ShaderCollectionInfo;
        }
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSaveSection();

        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();

    }
    private void DrawSaveSection()
    {
        if (GUILayout.Button("Save"))
        {
            ShaderProcessor.SetShaderCollectionInfo();
        }
    }


}
