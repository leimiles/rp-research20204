using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MIDManager
{
    public static Dictionary<Mesh, Color> meshesSet;
    public static Dictionary<Mesh, int> meshesSetToTrisCount;
    public static Dictionary<Mesh, List<GameObject>> meshesSetToObjects;
    public static Dictionary<Material, Color> materialsSet;
    public static Dictionary<Material, List<GameObject>> materialsSetToObjects;
    public static Dictionary<Material, int> materialsSetToTrisCount;
    public static Dictionary<Shader, Color> shadersSet;
    public static Dictionary<Shader, List<GameObject>> shadersSetToObjects;
    public static Dictionary<Shader, int> shadersSetToTrisCount;
    public static Dictionary<string, Color> variantsSet;
    public static Dictionary<string, List<GameObject>> variantsSetToObjects;
    public static Dictionary<string, int> variantsSetToTrisCount;
    public static Color GetColor(Material material, GameObject gameObject)
    {
        RegisterMaterial(material, gameObject);
        return materialsSet[material];
    }
    public static Color GetColor(Shader shader, GameObject gameObject)
    {
        RegisterShader(shader, gameObject);
        return shadersSet[shader];
    }

    public static Color GetColor(string shaderVariantsName, GameObject gameObject)
    {
        RigisterShaderVariant(shaderVariantsName, gameObject);
        return variantsSet[shaderVariantsName];
    }

    public static Color GetColor(Mesh mesh, GameObject gameObject)
    {
        RigisterMesh(mesh, gameObject);
        return meshesSet[mesh];
    }

    public static string GetFullVariantName(Material material)
    {
        string shaderName = material.shader.name;
        List<LocalKeyword> localKeywords = new List<LocalKeyword>(material.enabledKeywords);
        localKeywords.Sort((n1, n2) => n1.name.CompareTo(n2.name));
        List<GlobalKeyword> globalKeywords = new List<GlobalKeyword>(Shader.enabledGlobalKeywords);
        globalKeywords.Sort((n1, n2) => n1.name.CompareTo(n2.name));
        string fullVariantsName = shaderName + "| ";
        foreach (GlobalKeyword globalKeyword in globalKeywords)
        {
            fullVariantsName += globalKeyword.name;
            fullVariantsName += "| ";
        }
        foreach (LocalKeyword localKeyword in localKeywords)
        {
            fullVariantsName += localKeyword.name;
            fullVariantsName += "| ";
        }
        return fullVariantsName;
    }

    public static void SwapColor(MIDFeature.MIDMode mode)
    {
        switch (mode)
        {
            case MIDFeature.MIDMode.ByShader:
                List<Shader> shaders = new List<Shader>();
                foreach (Shader shader in shadersSet.Keys)
                {
                    shaders.Add(shader);
                }
                foreach (Shader shader in shaders)
                {
                    shadersSet[shader] = GetNewColor();
                }
                break;
        }
    }
    static void RigisterShaderVariant(string shaderVariants, GameObject gameObject)
    {
        if (variantsSet == null)
        {
            variantsSet = new Dictionary<string, Color>();
            variantsSetToObjects = new Dictionary<string, List<GameObject>>();
            variantsSetToTrisCount = new Dictionary<string, int>();
        }
        if (!variantsSet.ContainsKey(shaderVariants))
        {
            Color newColor = GetNewColor();
            variantsSet.Add(shaderVariants, newColor);
            variantsSetToObjects[shaderVariants] = new List<GameObject>();
            variantsSetToObjects[shaderVariants].Add(gameObject);
            variantsSetToTrisCount[shaderVariants] = GetTrisCount(gameObject);
        }
        else
        {
            if (!variantsSetToObjects[shaderVariants].Contains(gameObject))
            {
                variantsSetToObjects[shaderVariants].Add(gameObject);
                variantsSetToTrisCount[shaderVariants] += GetTrisCount(gameObject);
            }
        }
    }

    static void RigisterMesh(Mesh mesh, GameObject gameObject)
    {
        if (meshesSet == null)
        {
            meshesSet = new Dictionary<Mesh, Color>();
            meshesSetToTrisCount = new Dictionary<Mesh, int>();
            meshesSetToObjects = new Dictionary<Mesh, List<GameObject>>();
        }
        if (!meshesSet.ContainsKey(mesh))
        {
            Color newColor = GetNewColor();
            meshesSet.Add(mesh, newColor);
            meshesSetToTrisCount.Add(mesh, mesh.triangles.Length / 3);
            meshesSetToObjects[mesh] = new List<GameObject>();
            meshesSetToObjects[mesh].Add(gameObject);
        }
        else
        {
            if (!meshesSetToObjects[mesh].Contains(gameObject))
            {
                meshesSetToObjects[mesh].Add(gameObject);
            }
        }

    }
    static void RegisterMaterial(Material material, GameObject gameObject)
    {
        if (materialsSet == null)
        {
            materialsSet = new Dictionary<Material, Color>();
            materialsSetToObjects = new Dictionary<Material, List<GameObject>>();
            materialsSetToTrisCount = new Dictionary<Material, int>();
        }
        if (!materialsSet.ContainsKey(material))
        {
            Color newColor = GetNewColor();
            materialsSet.Add(material, newColor);
            materialsSetToObjects[material] = new List<GameObject>();
            materialsSetToObjects[material].Add(gameObject);
            materialsSetToTrisCount[material] = GetTrisCount(gameObject);
        }
        else
        {
            if (!materialsSetToObjects[material].Contains(gameObject))
            {
                materialsSetToObjects[material].Add(gameObject);
                materialsSetToTrisCount[material] += GetTrisCount(gameObject);
            }
        }
    }
    static void RegisterShader(Shader shader, GameObject gameObject)
    {
        if (shadersSet == null)
        {
            shadersSet = new Dictionary<Shader, Color>();
            shadersSetToObjects = new Dictionary<Shader, List<GameObject>>();
            shadersSetToTrisCount = new Dictionary<Shader, int>();
        }
        if (!shadersSet.ContainsKey(shader))
        {
            Color newColor = GetNewColor();
            shadersSet.Add(shader, newColor);
            shadersSetToObjects[shader] = new List<GameObject>();
            shadersSetToObjects[shader].Add(gameObject);
            shadersSetToTrisCount[shader] = GetTrisCount(gameObject);
        }
        else
        {
            if (!shadersSetToObjects[shader].Contains(gameObject))
            {
                shadersSetToObjects[shader].Add(gameObject);
                shadersSetToTrisCount[shader] += GetTrisCount(gameObject);
            }
        }
    }

    static int GetTrisCount(GameObject gameObject)
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            return meshFilter.sharedMesh.triangles.Length / 3;
        }
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            return skinnedMeshRenderer.sharedMesh.triangles.Length / 3;
        }
        return 0;
    }
    public static void Clear()
    {
        if (materialsSet != null)
        {
            materialsSet.Clear();
            materialsSetToObjects.Clear();
            materialsSetToTrisCount.Clear();
            materialsSet = null;
        }
        if (shadersSet != null)
        {
            shadersSet.Clear();
            shadersSetToObjects.Clear();
            shadersSetToTrisCount.Clear();
        }
        if (variantsSet != null)
        {
            variantsSet.Clear();
            variantsSetToObjects.Clear();
            variantsSetToTrisCount.Clear();
        }
        if (meshesSet != null)
        {
            meshesSet.Clear();
            meshesSetToTrisCount.Clear();
            meshesSetToObjects.Clear();
        }
    }
    static Color GetNewColor()
    {
        return new Color(Random.Range(0.0f, 1.0f) * 0.5f + 0.5f, Random.Range(0.0f, 1.0f) * 0.5f + 0.5f, Random.Range(0.0f, 1.0f) * 0.5f + 0.5f);
    }

}
