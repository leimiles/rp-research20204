#ifndef F2P_GENERIC_INPUT_INCLUDED
#define F2P_GENERIC_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _ST;
CBUFFER_END

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

#endif