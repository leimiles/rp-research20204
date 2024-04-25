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
        public MilesDepthPass()
        {
            base.profilingSampler = new ProfilingSampler(nameof(MilesDepthPass));
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, 0);
            base.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecutePass(context, ref renderingData);
        }

        private static void ExecutePass(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("DrawMilesDepth");
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            var drawSettings = RenderingUtils.CreateDrawingSettings(k_ShaderTagId, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            drawSettings.perObjectData = PerObjectData.None;
            context.DrawSkybox(renderingData.cameraData.camera);
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);

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
