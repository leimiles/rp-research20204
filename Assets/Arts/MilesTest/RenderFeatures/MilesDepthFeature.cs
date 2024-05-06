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
        static FilteringSettings m_FilteringSettings;
        static DrawingSettings m_DrawingSettings;
        private RTHandle destinationRT;
        public MilesDepthPass()
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
            renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_DrawingSettings = RenderingUtils.CreateDrawingSettings(k_ShaderTagId, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            context.DrawRenderers(renderingData.cullResults, ref m_DrawingSettings, ref m_FilteringSettings);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.memoryless = RenderTextureMemoryless.Color;
            RenderingUtils.ReAllocateIfNeeded(ref destinationRT, desc);
            ConfigureTarget(destinationRT);
        }
    }

    MilesDepthPass milesDepthPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(milesDepthPass);
    }

    public override void Create()
    {
        milesDepthPass = new MilesDepthPass();
    }
}
