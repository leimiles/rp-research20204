Shader "SoFunny/Utils/MaterialID"
{
    Properties
    {
        [HideInInspector][PerRendererData]_ColorID ("ColorID", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Overlay" "Queue" = "Overlay" }

        Pass
        {
            Name "MID"
            Tags { "LightMode" = "UniversalForward" }

            //Blend One One
            //BlendOp Add
            HLSLPROGRAM
            #pragma target 4.5
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct attributes
            {
                float3 positionOS : POSITION;
                half3 normalOS : NORMAL;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorID;
            CBUFFER_END

            struct varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half3 viewDirWS : TEXCOORD1;
                //half3 vertexSH : TEXCOORD2;

            };

            varyings vert(attributes input)
            {
                varyings o = (varyings)0;
                VertexPositionInputs vpi = GetVertexPositionInputs(input.positionOS);
                o.positionCS = vpi.positionCS;
                VertexNormalInputs vni = GetVertexNormalInputs(input.normalOS);
                o.normalWS = vni.normalWS;
                o.viewDirWS = GetWorldSpaceNormalizeViewDir(vpi.positionWS);
                //o.vertexSH = SampleSH(vni.normalWS);
                return o;
            }

            half4 frag(varyings i) : SV_Target
            {
                //_ColorID.rgb *= i.vertexSH.rgb;
                return _ColorID;
            }
            ENDHLSL
        }
    }
}
