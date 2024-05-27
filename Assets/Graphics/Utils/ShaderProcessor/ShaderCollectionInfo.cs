using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FunnyShaderCollections", menuName = "Shader/Funny Shader Collection", order = 1)]
public class ShaderCollectionInfo : ScriptableObject
{
    [Serializable]
    public class ShaderInfo
    {
        public Shader shader;
        public string[] keywordsToStrip;
    }
    public List<ShaderInfo> shaderInfos;

}

