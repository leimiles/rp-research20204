
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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
    class MaterialPropertyBlockData
    {
        public MIDMode m_MaterialIDMode;
        public MaterialPropertyBlock materialPropertyBlock;
        static Material errorMaterial;
        public MaterialPropertyBlockData()
        {
            m_MaterialIDMode = MIDMode.Off;
            materialPropertyBlock = new MaterialPropertyBlock();
            errorMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/InternalErrorShader"));
        }
        public static void SetColorByMaterials(Renderer[] renderers)
        {
            if (renderers != null && renderers.Length > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    //if (renderer == null) return;       // todo: check why render will be null even if renders is not
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        renderer.GetPropertyBlock(m_MaterialPropertyBlockData.materialPropertyBlock, i);
                        // use the propertyName "_ColorID", because it's not often to see it in shaders
                        m_MaterialPropertyBlockData.materialPropertyBlock.SetColor(Shader.PropertyToID("_ColorID"), MIDManager.GetColor(renderer.sharedMaterials[i] == null ? errorMaterial : renderer.sharedMaterials[i], renderer.gameObject));
                        renderer.SetPropertyBlock(m_MaterialPropertyBlockData.materialPropertyBlock, i);
                    }
                }
            }

        }

        public void Clear(Renderer[] renderers)
        {
            if (renderers != null && renderers.Length > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    //if (renderer == null) return;       // todo: check why render will be null even if renders is not
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        renderer.GetPropertyBlock(m_MaterialPropertyBlockData.materialPropertyBlock, i);
                        m_MaterialPropertyBlockData.materialPropertyBlock.Clear();
                        renderer.SetPropertyBlock(m_MaterialPropertyBlockData.materialPropertyBlock, i);
                    }
                }
            }
        }
    }

    public MIDMode m_MaterialIDMode = MIDMode.Off;
    private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    private Material material;
    private MIDPass m_MIDPass;
    static private MaterialPropertyBlockData m_MaterialPropertyBlockData;
    static Renderer[] renderers;
    static string m_SceneName;        // scenename if scene's changed
    public override void Create()
    {
#if UNITY_EDITOR
        if (material == null)
        {
            material = CoreUtils.CreateEngineMaterial(Shader.Find("SoFunny/Utils/MaterialID"));
        }
        m_MIDPass = new MIDPass(material);
        m_MIDPass.renderPassEvent = renderPassEvent;
        if (m_MaterialPropertyBlockData == null)
        {
            m_MaterialPropertyBlockData = new MaterialPropertyBlockData();
        }
        SetRenders(SceneManager.GetActiveScene().name);
        SetColors(m_MaterialIDMode);
#endif
    }

#if UNITY_EDITOR
    void SetRenders(string sceneName)
    {
        if (isActive)
        {
            if (m_SceneName == null || m_SceneName != sceneName || renderers == null)
            {
                m_SceneName = sceneName;
                renderers = GameObject.FindObjectsOfType<Renderer>();
            }
        }
    }

    void SetColors(MIDMode mode)
    {
        if (isActive) return;
        switch (mode)
        {
            case MIDMode.ByMaterial:
                if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByMaterial || !isActive)
                {
                    m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByMaterial;
                    MIDManager.Clear();
                    MaterialPropertyBlockData.SetColorByMaterials(renderers);
                }
                break;
            case MIDMode.ByShader:
                if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByShader || !isActive)
                {
                    m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByShader;
                    MIDManager.Clear();
                    Debug.Log("set colors by shaders");
                }

                break;
            case MIDMode.ByShaderAndKeywords:
                if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByShaderAndKeywords || !isActive)
                {
                    m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByShaderAndKeywords;
                    MIDManager.Clear();
                    Debug.Log("set colors by shader and keywords");
                }

                break;
            case MIDMode.ByMesh:
                if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByMesh || !isActive)
                {
                    m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByMesh;
                    MIDManager.Clear();
                    Debug.Log("set colors by mesh");
                }

                break;
            default:
                if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.Off || !isActive)
                {
                    m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.Off;
                    MIDManager.Clear();
                    Debug.Log("color cleared");
                }
                break;
        }
    }
#endif
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

#if UNITY_EDITOR
        if (renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            SetRenders(SceneManager.GetActiveScene().name);
            if (m_MaterialIDMode != MIDMode.Off)
            {
                renderer.EnqueuePass(m_MIDPass);
            }

            /*
            switch (m_MaterialIDMode)
            {
                case MIDMode.ByMaterial:
                    if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByMaterial)
                    {
                        m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByMaterial;
                        MaterialPropertyBlockData.Reset(renderers);
                        Debug.Log("bymaterial");
                    }
                    renderer.EnqueuePass(m_MIDPass);
                    break;
                case MIDMode.ByShader:
                    if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByShader)
                    {
                        m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByShader;
                        MaterialPropertyBlockData.Reset(renderers);
                        Debug.Log("byshader");
                    }
                    break;
                case MIDMode.ByShaderAndKeywords:
                    if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByShaderAndKeywords)
                    {
                        m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByShaderAndKeywords;
                        MaterialPropertyBlockData.Reset(renderers);
                        Debug.Log("byshaderandkeywords");
                    }
                    break;
                case MIDMode.ByMesh:
                    if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.ByMesh)
                    {
                        m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.ByMesh;
                        MaterialPropertyBlockData.Reset(renderers);
                        Debug.Log("bymesh");
                    }
                    break;
                default:
                    if (m_MaterialPropertyBlockData.m_MaterialIDMode != MIDMode.Off)
                    {
                        m_MaterialPropertyBlockData.m_MaterialIDMode = MIDMode.Off;
                        m_MaterialPropertyBlockData.Clear();
                        //MIDManager.Clear();
                        Debug.Log("off");
                    }

                    break;
            }
            */

        }
#endif
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
    }
}
