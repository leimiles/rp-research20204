Shader "MileStudio/Test/UberTransparent"
{
    Properties
    {
        // override properties
        _BaseColor ("Base Color", Color) = (1, 1, 0, 0.5)
        // uniform properties
        _Scale ("Scale", Range(1, 3)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "RenderType" = "Opaque" "IgnoreProjector" = "True" }
        Pass
        {
            Name "MilesForward"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            // #pragma shader_feature _A
            // #pragma shader_feature _B
            // #pragma shader_feature _C
            // #pragma shader_feature _D

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            //#pragma multi_compile _APPLE _BANANA
            //#pragma multi_compile _CAR _DAD
            //#pragma multi_compile _EGG _FUCK
            //#pragma multi_compile _GIRL _HI
            //#pragma multi_compile _ILL _JACK

            #ifndef HAVE_VFX_MODIFICATION
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
                    #pragma target 3.5 DOTS_INSTANCING_ON
                #else
                    #pragma target 4.5 DOTS_INSTANCING_ON
                #endif
            #endif // HAVE_VFX_MODIFICATION

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Scale;
            CBUFFER_END

            // this code is used when material property override enabled
            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
            #endif

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                // scale with _Scale
                float4 scaleX = float4(_Scale, 0, 0, 0);
                float4 scaleY = float4(0, _Scale, 0, 0);
                float4 scaleZ = float4(0, 0, _Scale, 0);
                float4 scaleW = float4(0, 0, 0, 1);
                float4x4 scaleMatrix = float4x4(scaleX, scaleY, scaleZ, scaleW);
                v.positionOS.xyz = mul(scaleMatrix, float4(v.positionOS.xyz, 1)).xyz;
                // scale with _Scale

                VertexPositionInputs vpi = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs vni = GetVertexNormalInputs(v.normalOS);
                o.normalWS = vni.normalWS;
                o.positionCS = vpi.positionCS;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {

                UNITY_SETUP_INSTANCE_ID(i);
                half4 color = _BaseColor;
                return color;
                //SampleSH()
                half3 colorSH = SampleSH(i.normalWS);
                color.rgb *= colorSH;
                return color;

                /*
                // useless for now
                #ifdef _APPLE
                    color = half4(1, 0, 0, 0);
                #endif

                #ifdef _BANANA
                    color = half4(0, 1, 0, 0);
                #endif

                #ifdef _CAR
                    color = half4(0, 0, 1, 0);
                #endif

                #ifdef _DAD
                    color = half4(0, 0, 0, 0);
                #endif
                return color;
                */
            }
            ENDHLSL
        }
    }
}