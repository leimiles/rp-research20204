using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SelectObjects : EditorWindow
{
    private float m_MinSizeRe = 0f;
    private float m_MaxSizeRe = 0f;

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
        m_MinSizeRe = EditorGUILayout.FloatField(m_MinSizeRe);
        m_MaxSizeRe = EditorGUILayout.FloatField(m_MaxSizeRe);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        EditorGUILayout.Space();

        if (GUILayout.Button("物体分组"))
        {
            selectedObjects.Clear();
            SelectObjectsWithTransform();
        }
        if (GUILayout.Button("Mesh Filters"))
        {
            SelectMeshFilter();
        }
        if (GUILayout.Button("Mesh Filters By Mesh"))
        {
            if (targetMesh != null)
            {
                SelectMeshFilter(targetMesh);
            }
        }
        targetMesh = EditorGUILayout.ObjectField(targetMesh, typeof(Mesh), true) as Mesh;

    }

    Mesh targetMesh;

    void SelectMeshFilter()
    {

        MeshFilter[] meshFilters = GameObject.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            selectedObjects.Add(meshFilter.gameObject);
        }
        Selection.objects = selectedObjects.ToArray();

    }

    void SelectMeshFilter(Mesh mesh)
    {
        MeshFilter[] meshFilters = GameObject.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter != null && meshFilter.sharedMesh == targetMesh)
            {
                selectedObjects.Add(meshFilter.gameObject);
            }

        }
        Selection.objects = selectedObjects.ToArray();
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

                if (!(m_MinSizeRe == 0 && m_MaxSizeRe == 0) && (size >= m_MinSizeRe && size <= m_MaxSizeRe))
                {
                    selectedObjects.Add(renderer.transform.gameObject);
                }
            }
        }

        Selection.objects = selectedObjects.ToArray();
        Debug.Log("Objects 选择完成，共选择：" + selectedObjects.Count + "个");
    }
}