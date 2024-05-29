using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SelectObjects : EditorWindow
{
    private float minSize = 0f;
    private float maxSize = 0f;

    List<Object> selectedObjects = new List<Object>();

    [MenuItem("ArtTools/物体分组")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<SelectObjects>("物体分组");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(" 按包围盒对角线长过滤");

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("包围盒最小最大值:");
        GUILayout.Space(45);
        minSize = EditorGUILayout.FloatField(minSize);
        maxSize = EditorGUILayout.FloatField(maxSize);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        EditorGUILayout.Space();

        if (GUILayout.Button("物体分组"))
        {
            selectedObjects.Clear();
            SelectObjectsWithTransform();
        }
    }

    // 选择场景物体
    void SelectObjectsWithTransform()
    {
        Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();

        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                float size = renderer.bounds.size.magnitude;

                if (!(minSize == 0 && maxSize == 0) && (size >= minSize && size <= maxSize))
                {
                    selectedObjects.Add(renderer.transform.gameObject);
                }
            }
        }

        Selection.objects = selectedObjects.ToArray();
        Debug.Log("Objects 选择完成，共选择：" + selectedObjects.Count + "个");
    }
}