using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

class MIDPass : ScriptableRenderPass
{
    private MaterialPropertyBlock materialPropertyBlock;
    private Material m_OverrideMaterial;
    public Renderer[] renderers;
    List<ShaderTagId> shaderTagIds = new List<ShaderTagId>
    {
        new("UniversalForward"),
        new("SRPDefaultUnlit"),
        new("UniversalForwardOnly")
    };
    FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
    private ProfilingSampler m_ProfilingSampler;
    public MIDPass(Material material)
    {
        this.m_OverrideMaterial = material;
        m_ProfilingSampler = new ProfilingSampler("MID Pass");
    }
    //public MIDFeature.MIDMode mIDMode;
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer commandBuffer = CommandBufferPool.Get();
        using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
        {
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
            Draw(context, ref renderingData);
        }
        context.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
        CommandBufferPool.Release(commandBuffer);
    }

    void Draw(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var drawSettings = RenderingUtils.CreateDrawingSettings(shaderTagIds, ref renderingData, SortingCriteria.RenderQueue);
        drawSettings.overrideMaterial = m_OverrideMaterial;
        context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
    }

    /*
    void DrawRenderersByMesh(ref CommandBuffer commandBuffer)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                return;
            }
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    renderer.GetPropertyBlock(materialPropertyBlock);
                    materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(mesh, renderer.gameObject));
                    renderer.SetPropertyBlock(materialPropertyBlock);
                    commandBuffer.DrawRenderer(renderer, material);
                }
                else
                {
                    Debug.Log(renderer.gameObject.name + " has no mesh");
                }
                continue;
            }
            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null)
            {
                Mesh mesh = skinnedMeshRenderer.sharedMesh;
                if (mesh != null)
                {
                    renderer.GetPropertyBlock(materialPropertyBlock);
                    materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(mesh, renderer.gameObject));
                    renderer.SetPropertyBlock(materialPropertyBlock);
                    commandBuffer.DrawRenderer(renderer, material);
                }
                else
                {
                    Debug.Log(renderer.gameObject.name + "has no mesh");
                }
                continue;
            }
        }
    }

    void DrawRenderersByMaterial(ref CommandBuffer commandBuffer)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                return;
            }
            if (renderer.sharedMaterials.Length > 1)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.GetPropertyBlock(materialPropertyBlock);
                    materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(renderer.sharedMaterials[i] == null ? errorMaterial : renderer.sharedMaterials[i], renderer.gameObject));
                    renderer.SetPropertyBlock(materialPropertyBlock);
                    commandBuffer.DrawRenderer(renderer, material, i);
                }
            }
            else
            {
                renderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(renderer.sharedMaterial == null ? errorMaterial : renderer.sharedMaterial, renderer.gameObject));
                renderer.SetPropertyBlock(materialPropertyBlock);
                commandBuffer.DrawRenderer(renderer, material);
            }
        }
    }

    void DrawRenderersByShader(ref CommandBuffer commandBuffer)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                return;
            }
            if (renderer.sharedMaterials.Length > 1)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.GetPropertyBlock(materialPropertyBlock);
                    materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(renderer.sharedMaterials[i] == null ? errorMaterial.shader : renderer.sharedMaterials[i].shader, renderer.gameObject));
                    renderer.SetPropertyBlock(materialPropertyBlock);
                    commandBuffer.DrawRenderer(renderer, material, i);
                }
            }
            else
            {
                renderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(renderer.sharedMaterial == null ? errorMaterial.shader : renderer.sharedMaterial.shader, renderer.gameObject));
                renderer.SetPropertyBlock(materialPropertyBlock);
                commandBuffer.DrawRenderer(renderer, material);
            }
        }
    }

    void DrawRenderersByShaderAndKeywords(ref CommandBuffer commandBuffer)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                return;
            }
            if (renderer.sharedMaterials.Length > 1)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.GetPropertyBlock(materialPropertyBlock);
                    string fullVariantName = MIDManager.GetFullVariantName(renderer.sharedMaterials[i] == null ? errorMaterial : renderer.sharedMaterials[i]);
                    materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(fullVariantName, renderer.gameObject));
                    renderer.SetPropertyBlock(materialPropertyBlock);
                    commandBuffer.DrawRenderer(renderer, material, i);
                }
            }
            else
            {
                renderer.GetPropertyBlock(materialPropertyBlock);
                string fullVariantName = MIDManager.GetFullVariantName(renderer.sharedMaterial == null ? errorMaterial : renderer.sharedMaterial);
                materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), MIDManager.GetColor(fullVariantName, renderer.gameObject));
                renderer.SetPropertyBlock(materialPropertyBlock);
                commandBuffer.DrawRenderer(renderer, material);
            }
        }
    }
    */
}
