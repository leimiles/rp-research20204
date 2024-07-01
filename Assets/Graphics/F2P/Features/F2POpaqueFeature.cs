using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class F2POpaqueFeature : ScriptableRendererFeature
{
    [SerializeField]
    string lightModeTag = "F2P";
    [SerializeField]
    LayerMask layerMask = 1;
    [SerializeField]
    int stencilReference = 0;
    ShaderTagId[] shaderTagIds = new ShaderTagId[1];
    StencilState stencilState = StencilState.defaultValue;
    static readonly string profilerTag = "F2POpaquePass";
    F2POpaquePass f2POpaquePass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        f2POpaquePass.Setup(ref renderer);
        renderer.EnqueuePass(f2POpaquePass);
    }

    public override void Create()
    {
        shaderTagIds[0] = new ShaderTagId(lightModeTag);
        if (stencilReference != 0)
        {
            stencilState.SetPassOperation(StencilOp.Replace);
        }
        f2POpaquePass = new F2POpaquePass(profilerTag, shaderTagIds, true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, layerMask, stencilState, stencilReference);
    }

    public class F2POpaquePass : DrawObjectsPass
    {
        private ScriptableRenderer m_Renderer = null;
        private RTHandle m_ColorRT;
        public F2POpaquePass(string profilerTag, ShaderTagId[] shaderTagIds, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
            : base(profilerTag, shaderTagIds, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
        {
            //...
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {

        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //ConfigureColorStoreAction(RenderBufferStoreAction.DontCare);
            //ConfigureDepthStoreAction(RenderBufferStoreAction.DontCare);
            //ConfigureClear(ClearFlag.Color, Color.black);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

            //RenderingUtils.ReAllocateIfNeeded(ref m_ColorRT, renderingData.cameraData.cameraTargetDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "m_ColorRT");
            if (renderingData.cameraData.renderType == CameraRenderType.Base)
            {
                ConfigureClear(ClearFlag.Color | ClearFlag.Depth, Color.black);     // make sure we dont't "load" anything for this pass

            }
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay)
            {
                ConfigureClear(ClearFlag.Depth, Color.black);   // for overlay camera, make sure we do "load" everything before this camera
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }


        public void Setup(ref ScriptableRenderer scriptableRenderer)
        {
            m_Renderer = scriptableRenderer;
        }

    }
}
