using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MilesDepthFeature : ScriptableRendererFeature
{
    class MilesDepthPass : ScriptableRenderPass
    {
        private static readonly ShaderTagId k_ShaderTagId = new ShaderTagId("MilesDepth");
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

    }

    public override void Create()
    {

    }
}
