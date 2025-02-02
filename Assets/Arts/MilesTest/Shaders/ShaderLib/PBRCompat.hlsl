#extension GL_ARB_shader_texture_lod:enable
#ifndef PI
    #define PI 3.14159265358979323846264
#endif
#ifndef NB_PROBES
    #define NB_PROBES 0
#endif

//Specular fresnel computation
vec3 F_Shlick(float vh, vec3 F0)
{
    float fresnelFact = pow(2.0, (-5.55473 * vh - 6.98316) * vh);
    return mix(F0, vec3(1.0, 1.0, 1.0), fresnelFact);
}

vec3 sphericalHarmonics(const in vec3 normal, const vec3 sph[9])
{
    float x = normal.x;
    float y = normal.y;
    float z = normal.z;

    vec3 result = (
        sph[0] +

        sph[1] * y +
        sph[2] * z +
        sph[3] * x +

        sph[4] * y * x +
        sph[5] * y * z +
        sph[6] * (3.0 * z * z - 1.0) +
        sph[7] * (z * x) +
        sph[8] * (x * x - y * y)
    );

    return max(result, vec3(0.0));
}


float PBR_ComputeDirectLight(vec3 normal, vec3 lightDir, vec3 viewDir,
vec3 lightColor, vec3 fZero, float roughness, float ndotv,
out vec3 outDiffuse, out vec3 outSpecular)
{
    // Compute halfway vector.
    vec3 halfVec = normalize(lightDir + viewDir);

    // Compute ndotl, ndoth,  vdoth terms which are needed later.
    float ndotl = max(dot(normal, lightDir), 0.0);
    float ndoth = max(dot(normal, halfVec), 0.0);
    float hdotv = max(dot(viewDir, halfVec), 0.0);

    // Compute diffuse using energy-conserving Lambert.
    // Alternatively, use Oren-Nayar for really rough
    // materials or if you have lots of processing power ...
    outDiffuse = vec3(ndotl) * lightColor;

    //cook-torrence, microfacet BRDF : http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf
    
    float alpha = roughness * roughness;

    //D, GGX normaal Distribution function
    float alpha2 = alpha * alpha;
    float sum = ((ndoth * ndoth) * (alpha2 - 1.0) + 1.0);
    float denom = PI * sum * sum;
    float D = alpha2 / denom;

    // Compute Fresnel function via Schlick's approximation.
    vec3 fresnel = F_Shlick(hdotv, fZero);
    
    //G Shchlick GGX Gometry shadowing term,  k = alpha/2
    float k = alpha * 0.5;

    /*
    //classic Schlick ggx
    float G_V = ndotv / (ndotv * (1.0 - k) + k);
    float G_L = ndotl / (ndotl * (1.0 - k) + k);
    float G = ( G_V * G_L );
    
    float specular =(D* fresnel * G) /(4 * ndotv);
    */
    
    // UE4 way to optimise shlick GGX Gometry shadowing term
    //http://graphicrants.blogspot.co.uk/2013/08/specular-brdf-reference.html
    float G_V = ndotv + sqrt((ndotv - ndotv * k) * ndotv + k);
    float G_L = ndotl + sqrt((ndotl - ndotl * k) * ndotl + k);
    // the max here is to avoid division by 0 that may cause some small glitches.
    float G = 1.0 / max(G_V * G_L, 0.01);

    float specular = D * G * ndotl;
    
    outSpecular = vec3(specular) * fresnel * lightColor;
    return hdotv;
}

vec3 integrateBRDFApprox(const in vec3 specular, float roughness, float NoV)
{
    const vec4 c0 = vec4(-1, -0.0275, -0.572, 0.022);
    const vec4 c1 = vec4(1, 0.0425, 1.04, -0.04);
    vec4 r = roughness * c0 + c1;
    float a004 = min(r.x * r.x, exp2(-9.28 * NoV)) * r.x + r.y;
    vec2 AB = vec2(-1.04, 1.04) * a004 + r.zw;
    return specular * AB.x + AB.y;
}

// from Sebastien Lagarde https://seblagarde.files.wordpress.com/2015/07/course_notes_moving_frostbite_to_pbr_v32.pdf page 69
vec3 getSpecularDominantDir(const in vec3 N, const in vec3 R, const in float realRoughness)
{
    vec3 dominant;

    float smoothness = 1.0 - realRoughness;
    float lerpFactor = smoothness * (sqrt(smoothness) + realRoughness);
    // The result is not normalized as we fetch in a cubemap
    dominant = mix(N, R, lerpFactor);

    return dominant;
}

vec3 ApproximateSpecularIBL(samplerCube envMap, sampler2D integrateBRDF, vec3 SpecularColor, float Roughness, float ndotv, vec3 refVec, float nbMipMaps)
{
    float Lod = sqrt(Roughness) * (nbMipMaps - 1.0);
    vec3 PrefilteredColor = textureCubeLod(envMap, refVec.xyz, Lod).rgb;
    vec2 EnvBRDF = texture2D(integrateBRDF, vec2(Roughness, ndotv)).rg;
    return PrefilteredColor * (SpecularColor * EnvBRDF.x + EnvBRDF.y);
}

vec3 ApproximateSpecularIBLPolynomial(samplerCube envMap, vec3 SpecularColor, float Roughness, float ndotv, vec3 refVec, float nbMipMaps)
{
    float Lod = sqrt(Roughness) * (nbMipMaps - 1.0);
    vec3 PrefilteredColor = textureCubeLod(envMap, refVec.xyz, Lod).rgb;
    return PrefilteredColor * integrateBRDFApprox(SpecularColor, Roughness, ndotv);
}


float renderProbe(vec3 viewDir, vec3 worldPos, vec3 normal, vec3 norm, float Roughness, vec4 diffuseColor, vec4 specularColor, float ndotv, vec3 ao, mat4 lightProbeData, vec3 shCoeffs[9], samplerCube prefEnvMap, inout vec3 color)
{

    // lightProbeData is a mat4 with this layout
    //   3x3 rot mat|
    //      0  1  2 |  3
    // 0 | ax bx cx | px | )
    // 1 | ay by cy | py | probe position
    // 2 | az bz cz | pz | )
    // --|----------|
    // 3 | sx sy sz   sp | -> 1/probe radius + nbMipMaps
    //    --scale--
    // parallax fix for spherical / obb bounds and probe blending from
    // from https://seblagarde.wordpress.com/2012/09/29/image-based-lighting-approaches-and-parallax-corrected-cubemap/
    vec3 rv = reflect(-viewDir, normal);
    vec4 probePos = lightProbeData[3];
    float invRadius = fract(probePos.w);
    float nbMipMaps = probePos.w - invRadius;
    vec3 direction = worldPos - probePos.xyz;
    float ndf = 0.0;

    if (lightProbeData[0][3] != 0.0)
    {
        // oriented box probe
        // mat3_sub our compat wrapper for mat3(mat4)
        mat3 wToLocalRot = inverse(mat3_sub(lightProbeData));

        vec3 scale = vec3(lightProbeData[0][3], lightProbeData[1][3], lightProbeData[2][3]);
        #if NB_PROBES >= 2
            // probe blending
            // compute fragment position in probe local space
            vec3 localPos = wToLocalRot * worldPos;
            localPos -= probePos.xyz;
            // compute normalized distance field
            vec3 localDir = abs(localPos);
            localDir /= scale;
            ndf = max(max(localDir.x, localDir.y), localDir.z);
        #endif
        // parallax fix
        vec3 rayLs = wToLocalRot * rv;
        rayLs /= scale;

        vec3 positionLs = worldPos - probePos.xyz;
        positionLs = wToLocalRot * positionLs;
        positionLs /= scale;

        vec3 unit = vec3(1.0);
        vec3 firstPlaneIntersect = (unit - positionLs) / rayLs;
        vec3 secondPlaneIntersect = (-unit - positionLs) / rayLs;
        vec3 furthestPlane = max(firstPlaneIntersect, secondPlaneIntersect);
        float distance = min(min(furthestPlane.x, furthestPlane.y), furthestPlane.z);

        vec3 intersectPositionWs = worldPos + rv * distance;
        rv = intersectPositionWs - probePos.xyz;
    }
    else
    {
        // spherical probe
        // paralax fix
        rv = invRadius * direction + rv;

        #if NB_PROBES >= 2
            // probe blending
            float dist = sqrt(dot(direction, direction));
            ndf = dist * invRadius;
        #endif
    }

    vec3 indirectDiffuse = vec3(0.0);
    vec3 indirectSpecular = vec3(0.0);
    indirectDiffuse = sphericalHarmonics(normal.xyz, shCoeffs) * diffuseColor.rgb;
    vec3 dominantR = getSpecularDominantDir(normal, rv.xyz, Roughness * Roughness);
    indirectSpecular = ApproximateSpecularIBLPolynomial(prefEnvMap, specularColor.rgb, Roughness, ndotv, dominantR, nbMipMaps);

    #ifdef HORIZON_FADE
        //horizon fade from http://marmosetco.tumblr.com/post/81245981087
        float horiz = dot(rv, norm);
        float horizFadePower = 1.0 - Roughness;
        horiz = clamp(1.0 + horizFadePower * horiz, 0.0, 1.0);
        horiz *= horiz;
        indirectSpecular *= vec3(horiz);
    #endif

    vec3 indirectLighting = (indirectDiffuse + indirectSpecular) * ao;

    color = indirectLighting * step(0.0, probePos.w);
    return ndf;
}