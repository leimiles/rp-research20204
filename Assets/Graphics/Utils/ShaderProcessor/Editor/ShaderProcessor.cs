using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderProcessor : IPreprocessShaders
{
    private static readonly string LOG_FILE_PATH = Application.dataPath + "/../ShaderLogs.txt";
    private static readonly string SHADER_INFO_PATH = "Assets/Graphics/Utils/ShaderProcessor/ShaderCollections/FunnyShaderCollections.asset";
    static ShaderCollectionInfo shaderCollectionInfo;
    public int callbackOrder
    {
        get
        {
            shaderCollectionInfo = AssetDatabase.LoadAssetAtPath<ShaderCollectionInfo>(SHADER_INFO_PATH);
            return 0;
        }
    }
    // shader run OnProcessShader once per type per pass, for example, shader "simplit" has a pass called "forwardlit", so this shader will run OnProcessShader at lease 2 times because forwardlit pass goes with vertex type and fragment type
    // attention : vertex-type always runs first, and if all vertex variants are stripped, the fragment-type won't run OnProcessShader
    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (shaderCollectionInfo == null)
        {
            SetShaderCollectionInfo();
        }
        if (shaderCollectionInfo.shaderInfos.Count > 0)
        {
            StripShadersKeywords(shader, snippet, data);
        }
    }

    public static void SetShaderCollectionInfo()
    {
        shaderCollectionInfo = AssetDatabase.LoadAssetAtPath<ShaderCollectionInfo>(SHADER_INFO_PATH);
        if (shaderCollectionInfo != null)
        {
            //UnityEngine.Debug.Log("funny shader collection found.");
        }
        else
        {
            UnityEngine.Debug.Log("funny shader collection won't work.");
        }
    }

    static void StripShadersKeywords(Shader shader, ShaderSnippetData shaderSnippetData, IList<ShaderCompilerData> shaderCompilerDatas)
    {
        foreach (ShaderCollectionInfo.ShaderInfo shaderInfo in shaderCollectionInfo.shaderInfos)
        {
            if (shaderInfo.shader != null && shaderInfo.shader.name == shader.name)
            {
                System.IO.File.AppendAllText(LOG_FILE_PATH, shader.name + "\tshader_type = " + shaderSnippetData.shaderType + "\tshader_pass = " + shaderSnippetData.passName + "\tcollection = " + shaderCompilerDatas.Count + "\ttime = " + System.DateTime.Now.ToString());

                for (int i = 0, index = 0; i < shaderCompilerDatas.Count; ++i, ++index)
                {
                    string shaderKeywordsStr = "";
                    string statusDesc = "keep\t";
                    ShaderKeyword[] shaderKeywordsArray = shaderCompilerDatas[i].shaderKeywordSet.GetShaderKeywords();

                    if (HasKeywordsToStrip(shaderInfo.keywordsToStrip, shaderCompilerDatas[i].shaderKeywordSet))
                    {
                        statusDesc = "strip\t";
                        shaderCompilerDatas.RemoveAt(i);
                        --i;
                    }
                    shaderKeywordsStr += statusDesc;
                    shaderKeywordsStr += string.Join('\t', shaderKeywordsArray);
                    System.IO.File.AppendAllText(LOG_FILE_PATH, "\n\tVariant[" + index + "]: \t" + shaderKeywordsStr);
                }
                System.IO.File.AppendAllText(LOG_FILE_PATH, "\n----------------------------- :p\n\n");
            }
        }
    }

    static bool HasKeywordsToStrip(string[] keywordsToCheck, ShaderKeywordSet shaderKeywordSet)
    {

        foreach (var temp in keywordsToCheck)
        {
            ShaderKeyword keyword = new ShaderKeyword(temp);
            if (shaderKeywordSet.IsEnabled(keyword))
            {
                return true;
            }
        }

        return false;
    }
}
