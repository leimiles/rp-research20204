using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MileSkyFeature : ScriptableRendererFeature
{
    class MileSkyPass : ScriptableRenderPass
    {
        Mesh skyMesh;
        Material skyMaterial;
        int skySize;
        Matrix4x4 localMatrix;
        public MileSkyPass(Mesh skyMesh, Material skyMaterial, int skySize = 50000)
        {
            this.skyMesh = skyMesh;
            this.skyMaterial = skyMaterial;
            this.skySize = skySize;
            this.localMatrix = Matrix4x4.identity;
            localMatrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one * skySize);
            profilingSampler = new ProfilingSampler(nameof(MileSkyPass));
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (skyMaterial != null && skyMesh != null)
            {
                var cmd = CommandBufferPool.Get("DrawMileSky");
                cmd.DrawMesh(skyMesh, localMatrix, skyMaterial);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
        }
    }
    [SerializeField]
    Mesh skyMesh;
    [SerializeField]
    int skySize = 1;
    [SerializeField]
    Material skyMaterial;

    MileSkyPass mileSkyPass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(mileSkyPass);
    }

    public override void Create()
    {
        mileSkyPass = new MileSkyPass(skyMesh, skyMaterial, skySize);
    }
}


