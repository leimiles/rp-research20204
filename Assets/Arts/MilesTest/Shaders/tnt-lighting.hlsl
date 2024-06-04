#ifndef TNT_LIGHTING_INCLUDED
#define TNT_LIGHTING_INCLUDED

#define invPI   0.3183h
#define INV_FG  0.0625h
#define MEDIUMP_FLT_MAX 65504.0h

#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)





struct TNTLightingData
{
    half3 giColor;
    half3 mainLightColor;
};

half3 LightingPBR()
{

}

// we dont' deal with alpha test for tnt base shading
half4 CalculateFinalColor(TNTLightingData lightingData, half3 albedo)
{
    half4 lightingColor = 1;
    return lightingColor;
}

half4 TNTFragment()
{
    TNTLightingData lightingData = (TNTLightingData)1;
    return CalculateFinalColor(lightingData, 1);
}

half2 LightingFuncGGX_FV(half ldoth, half roughness)
{
    half alpha = roughness * roughness; // don't use pow(x, y)

    half f0_a = 1.0h;
    half f0_b = pow(1.0h - ldoth, 5.0h);

    half k2 = alpha * alpha * 0.25h;
    half k2_inverse = 1.0h - k2;

    half vis = 1.0h / (ldoth * ldoth * k2_inverse + k2);

    return half2(f0_a * vis, f0_b * vis);
}

// fully rough shading
void Mobile_PBR_ComputeDirectLightOp5(half ndotv, half fZero, half3 normal, half3 lightDir, half3 viewDir,
half3 lightColor, half roughness, out half3 outDiffuse, out half3 outSpecular)
{
    // Compute halfway vector.
    half3 halfVec = normalize(lightDir + viewDir);

    // Compute ndotl, ndoth,  vdoth terms which are needed later.
    half ndotl = max(dot(normal, lightDir), 0.0h);
    half ndoth = max(dot(normal, halfVec), 0.0h);
    half hdotv = max(dot(viewDir, halfVec), 0.0h);


    // Compute diffuse using energy-conserving Lambert.
    // Alternatively, use Oren-Nayar for really rough
    // materials or if you have lots of processing power ...
    outDiffuse = half3(ndotl, ndotl, ndotl) * lightColor;

    //D, GGX normaal Distribution function
    const half D = invPI;

    /*
    // F,G
    half ldoth = max( dot(lightDir, halfVec),  0.0h);
    lvec2 FV_helper = LightingFuncGGX_FV(ldoth,roughness);
    half FV = fZero*FV_helper.x + (1.0h-fZero)*FV_helper.y;
    half specular = ldoth * D * FV;
    */

    // Compute Fresnel function via Schlick's approximation.
    half fresnel = fZero + (1.0h - fZero) * pow(2.0h, (-5.5547h * hdotv - 6.9831h) * hdotv);


    //G Shchlick GGX Gometry shadowing term,  k = alpha/2
    const half k = 0.5h;

    // UE4 way to optimise shlick GGX Gometry shadowing term
    //http://graphicrants.blogspot.co.uk/2013/08/specular-brdf-reference.html
    half G_V = ndotv + sqrt((ndotv - ndotv * k) * ndotv + k);
    half G_L = ndotl + sqrt((ndotl - ndotl * k) * ndotl + k);
    // the max here is to avoid division by 0 that may cause some small glitches.
    half G = 1.0h / max(G_V * G_L, 0.01h);

    half specular = D * G * ndotl;

    outSpecular = half3(specular, specular, specular) * lightColor;
}


void Mobile_PBR_ComputeDirectLightOp3(half3 normal, half3 lightDir, half3 viewDir,
half3 lightColor, half roughness,
out half3 outDiffuse, out half3 outSpecular)
{
    // Compute halfway vector.
    half3 halfVec = normalize(lightDir + viewDir);
    half ndotl = max(dot(normal, lightDir), 0.0h);
    half ndoth = max(dot(normal, halfVec), 0.0h);
    half3 nxh = cross(normal, halfVec);
    half OneMinusNoHSqr = dot(nxh, nxh);

    // diffuse Lambert BRDF
    outDiffuse = half3(ndotl, ndotl, ndotl) * lightColor;

    // spercular BRDF,D
    half alpha = roughness * roughness;
    half n = ndoth * alpha;
    half p = alpha / (OneMinusNoHSqr + n * n);
    half D = saturateMediump(p * p);
    // specular BRDF(f(x) = (roughness * 0.25h + 0.25h) * D
    // half specular = (roughness * specularAttn + specularAttn) * D;
    // FG model:G = Vis_SmithJointApprox ,F = F_Schlick
    // if n = l = v
    // G = 0.5 / rcp(v_smith_V + v_smithL) = 0.5/2 = 0.25
    // F = 50 * SpecularColor.g * Fc + (1 -Fc) * SpecularColor = SpecularColor ,(Fc=0)
    // G*F/4(n.l)(n.v) = G * F /4 = SpecularColor/16
    // specularBRDF = D * F * G / 4(n.l)(n.v) = D / 16.0h
    half specular = D * INV_FG;
    outSpecular = half3(specular, specular, specular) * lightColor;
}


void Classic_PBR_ComputeDirectLight(half3 normal, half3 lightDir, half3 viewDir,
half3 lightColor, half fZero, half roughness, half ndotv,
out half3 outDiffuse, out half3 outSpecular)
{
    // Compute halfway vector.
    half3 halfVec = normalize(lightDir + viewDir);

    // Compute ndotl, ndoth,  vdoth terms which are needed later.
    half ndotl = max(dot(normal, lightDir), 0.0h);
    half ndoth = max(dot(normal, halfVec), 0.0h);
    half hdotv = max(dot(viewDir, halfVec), 0.0h);


    // Compute diffuse using energy-conserving Lambert.
    // Alternatively, use Oren-Nayar for really rough
    // materials or if you have lots of processing power ...
    outDiffuse = half3(ndotl, ndotl, ndotl) * lightColor;

    //cook-torrence, microfacet BRDF : http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf

    half alpha = roughness * roughness;

    //D, GGX normaal Distribution function
    half alpha2 = alpha * alpha;
    half sum = ((ndoth * ndoth) * (alpha2 - 1.0h) + 1.0h);
    half denom = PI * sum * sum;
    half D = alpha2 / denom;

    /*
    // F,G
    half ldoth = max(dot(lightDir, halfVec), 0.0h);
    half2 FV_helper = LightingFuncGGX_FV(ldoth, roughness);
    half FV = fZero * FV_helper.x + (1.0h - fZero) * FV_helper.y;
    half specular = ldoth * D * FV;
    */

    // Compute Fresnel function via Schlick's approximation.
    half fresnel = fZero + (1.0h - fZero) * pow(2.0h, (-5.55473h * hdotv - 6.98316h) * hdotv);


    //G Shchlick GGX Gometry shadowing term,  k = alpha/2
    half k = alpha * 0.5h;

    //classic Schlick ggx

    half G_V = ndotv / (ndotv * (1.0h - k) + k);
    half G_L = ndotl / (ndotl * (1.0h - k) + k);
    half G = (G_V * G_L);

    half specular = (D * fresnel * G) / (4.0h * ndotv);

    specular = saturate(specular);



    /*

    // UE4 way to optimise shlick GGX Gometry shadowing term
    //http://graphicrants.blogspot.co.uk/2013/08/specular-brdf-reference.html
    half G_V = ndotv + sqrt((ndotv - ndotv * k) * ndotv + k);
    half G_L = ndotl + sqrt((ndotl - ndotl * k) * ndotl + k);
    // the max here is to avoid division by 0 that may cause some small glitches.
    half G = 1.0h / max(G_V * G_L, 0.01h);

    half specular = D * fresnel * G * ndotl;
    */


    outSpecular = half3(specular, specular, specular) * lightColor;
}




#endif