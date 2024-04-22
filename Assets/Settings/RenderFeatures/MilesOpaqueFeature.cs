using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MilesOpaqueFeature : ScriptableRendererFeature
{
    public class MilesOpaquePass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Debug.Log("MilesOpaquePass Executing...");
            //CameraSetup(renderin, ref renderingData);
        }

        private static void CameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }
    }
    MilesOpaquePass milesOpaquePass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(milesOpaquePass);
    }

    public override void Create()
    {
        milesOpaquePass = new MilesOpaquePass();
    }
}


