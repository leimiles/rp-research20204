using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MIDFeature : ScriptableRendererFeature
{
    public enum MIDMode
    {
        Off,
        ByMaterial,     // renderer shows up with id color via different materials
        ByShader,        // renderer shows up with id color via different shaders
        ByShaderAndKeywords, // renderer shows up with id color via different shaders and different shader keywords
        ByMesh
    }
    public MIDMode midMode = MIDMode.Off;
    private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    private Material material;
    private Material errorMaterial;
    private MIDPass m_MIDPass;
    private MaterialPropertyBlock materialPropertyBlock_ByMaterial;
    private MaterialPropertyBlock materialPropertyBlock_ByShader;
    private MaterialPropertyBlock materialPropertyBlock_ByShaderWithKeywords;
    private MaterialPropertyBlock materialPropertyBlock_ByMesh;

    public override void Create()
    {

        if (material == null || errorMaterial == null)
        {
            material = CoreUtils.CreateEngineMaterial(Shader.Find("SoFunny/Utils/MaterialID"));
            errorMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/InternalErrorShader"));
        }
        m_MIDPass = new MIDPass(material);
        m_MIDPass.renderPassEvent = renderPassEvent;
        InitRenderers();

        materialPropertyBlock_ByMaterial = new MaterialPropertyBlock();
        SetColorsByMaterials();
        materialPropertyBlock_ByShader = new MaterialPropertyBlock();
        materialPropertyBlock_ByShaderWithKeywords = new MaterialPropertyBlock();
        materialPropertyBlock_ByMesh = new MaterialPropertyBlock();
        m_MIDPass = new MIDPass(material);

    }

    void InitRenderers()
    {
        if (m_MIDPass.renderers == null)
        {
            m_MIDPass.renderers = GameObject.FindObjectsOfType<Renderer>();
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.SceneView)
        {
#if UNITY_EDITOR
            switch (midMode)
            {
                case MIDMode.ByMaterial:
                    renderer.EnqueuePass(m_MIDPass);
                    break;
                case MIDMode.ByShader:
                    break;
                case MIDMode.ByShaderAndKeywords:
                    break;
                case MIDMode.ByMesh:
                    break;
                default:
                    materialPropertyBlock_ByMaterial.Clear();
                    MIDManager.Clear();
                    break;
            }
#endif
        }
    }


    public void SetColorsByMaterials()
    {
        if (m_MIDPass.renderers != null && m_MIDPass.renderers.Length > 0)
        {
            foreach (Renderer renderer in m_MIDPass.renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.GetPropertyBlock(materialPropertyBlock_ByMaterial, i);
                    // use the propertyName "_ColorID", because it's not often to see it in shaders
                    materialPropertyBlock_ByMaterial.SetColor(Shader.PropertyToID("_ColorID"), MIDManager.GetColor(renderer.sharedMaterials[i] == null ? errorMaterial : renderer.sharedMaterials[i], renderer.gameObject));
                    //Debug.Log(renderer.name + " - " + i + " - " + materialPropertyBlock_ByMaterial.GetColor(Shader.PropertyToID("_Color")));
                    renderer.SetPropertyBlock(materialPropertyBlock_ByMaterial, i);
                }
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
        CoreUtils.Destroy(errorMaterial);
    }
}
