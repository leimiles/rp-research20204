Shader "SoFunny/TNT/Test"
{
    Properties
    {
        // override properties, this shader don't support alphaclip
        _BaseColor ("Base Color", Color) = (1, 1, 0, 1)
        [NoScaleOffset]_BaseMap ("Base Map", 2D) = "white" { }
        [NoScaleOffset]_NormalMap ("Normal Map", 2D) = "bump" { }
        [NoScaleOffset]_MAREMap ("Metallic AO Roughness EmissiveMask Map", 2D) = "black" { }
        _ST ("Scale And Offset", Vector) = (1, 1, 0, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "RenderType" = "Opaque" "IgnoreProjector" = "True" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "tnt-lighting.hlsl"

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
                half2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half2 uv : TEXCOORD0;
                half4 normalWS : TEXCOORD2;     // w = viewDir.x
                half4 tangentWS : TEXCOORD3;    // w = viewDir.y
                half4 bitangentWS : TEXCOORD4;  // w = viewDir.z
                half3 sh : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ST;
            CBUFFER_END

            TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);        SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MAREMap);        SAMPLER(sampler_MAREMap);

            // this code is used when material property override enabled, must use float4
            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _ST)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

                static float4 unity_DOTS_Sampled_BaseColor;
                static float4 unity_DOTS_Sampled_ST;

                void SetupDOTSLitMaterialPropertyCaches()
                {
                    unity_DOTS_Sampled_BaseColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
                    unity_DOTS_Sampled_ST = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _ST);
                }

                #undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
                #define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSLitMaterialPropertyCaches()

                #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
                #define _ST          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _ST)
            #endif

            // only what we need for tnt surface
            struct TNTSurfaceData
            {
                half3 albedo;
                half3 normalTS;
                half4 metalic_occlusion_roughness_emissionMask;
            };

            inline void InitializeTNTSurfaceData(half2 uv, out TNTSurfaceData outTNTSurfaceData)
            {
                outTNTSurfaceData = (TNTSurfaceData)0;
                outTNTSurfaceData.albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb * _BaseColor.rgb;
                outTNTSurfaceData.normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));
                outTNTSurfaceData.metalic_occlusion_roughness_emissionMask = half4(0.0h, 1.0h, 0.8h, 0.0h);
            }

            void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
            {
                inputData = (InputData)0;
                //inputData.positionWS = 0;   // no need for now
                half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
                inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
                inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = SafeNormalize(viewDirWS);
                //inputData.shadowCoord = 0; // no need for now
                //inputData.fogCoord = 0; //    no need for now
                //inputData.vertexLighting = 0    // no need for now
                inputData.bakedGI = SampleSHPixel(input.sh, inputData.normalWS);
                //inputData.normalizedScreenSpaceUV = 0;  // no need for now
                //inputData.shadowMask = 0;    // no need for now

            }

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                VertexPositionInputs vpi = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs vni = GetVertexNormalInputs(v.normalOS);
                o.uv = TRANSFORM_TEX(v.texcoord, _BaseMap);
                o.uv.xy = o.uv.xy * _ST.xy + _ST.zw;
                half3 viewDirWS = GetWorldSpaceViewDir(vpi.positionWS);
                o.normalWS = half4(vni.normalWS, viewDirWS.x);
                o.tangentWS = half4(vni.tangentWS, viewDirWS.y);
                o.bitangentWS = half4(vni.bitangentWS, viewDirWS.z);
                o.sh = SampleSHVertex(o.normalWS);
                o.positionCS = vpi.positionCS;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                TNTSurfaceData surfaceData;
                InitializeTNTSurfaceData(i.uv, surfaceData);

                InputData inputData;
                InitializeInputData(i, surfaceData.normalTS, inputData);

                return half4(surfaceData.albedo * inputData.bakedGI, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "MilesDepth"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On   // must output z-value
            ColorMask R // one channel output

            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ST;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                VertexPositionInputs vpi = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vpi.positionCS;

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                return 0;
            }

            ENDHLSL
        }
    }
}