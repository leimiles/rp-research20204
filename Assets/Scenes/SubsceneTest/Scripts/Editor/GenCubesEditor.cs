using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(GenCubes))]
public class GenCubesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Gen"))
        {

            GenCubes genCubes = target as GenCubes;
            if (genCubes == null)
            {
                return;
            }
            Delete(genCubes.gameObject);
            for (int i = 0; i < genCubes.count; i++)
            {
                Instantiate(genCubes.cube, Random.insideUnitSphere * genCubes.radius, Random.rotationUniform, genCubes.transform);
            }
        }
    }





    void Delete(GameObject parent)
    {
        Transform[] transforms = parent.GetComponentsInChildren<Transform>();
        if (transforms.Length > 1)
        {
            int count = transforms.Length;
            for (int i = 1; i < count; i++)
            {
                DestroyImmediate(transforms[i].gameObject);
            }
        }

    }
}
