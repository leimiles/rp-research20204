using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MilesOpaqueFeature : ScriptableRendererFeature
{
    class MilesOpaquePass : ScriptableRenderPass
    {
        private static readonly ShaderTagId k_ShaderTagId = new ShaderTagId("MilesForward");
        static FilteringSettings m_FilteringSettings;
        static DrawingSettings m_DrawingSettings;
        public MilesOpaquePass()
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
            renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_DrawingSettings = RenderingUtils.CreateDrawingSettings(k_ShaderTagId, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            context.DrawRenderers(renderingData.cullResults, ref m_DrawingSettings, ref m_FilteringSettings);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            // todo: why this render pass can't invalidate its depth?

        }

    }

    MilesOpaquePass milesDepthPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(milesDepthPass);
    }

    public override void Create()
    {
        milesDepthPass = new MilesOpaquePass();
    }
}
