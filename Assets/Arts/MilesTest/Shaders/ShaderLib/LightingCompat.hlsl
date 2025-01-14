
/*Common function for light calculations*/


/*
* Computes light direction
* lightType should be 0.0,1.0,2.0, repectively for Directional, point and spot lights.
* Outputs the light direction and the light half vector.
*/
void lightComputeDir(in vec3 worldPos, in float lightType, in vec4 position, out vec4 lightDir, out vec3 lightVec)
{
    float posLight = step(0.5, lightType);
    vec3 tempVec = position.xyz * sign(posLight - 0.5) - (worldPos * posLight);
    lightVec = tempVec;
    float dist = length(tempVec);
    #ifdef SRGB
        lightDir.w = (1.0 - position.w * dist) / (1.0 + position.w * dist * dist);
        lightDir.w = clamp(lightDir.w, 1.0 - posLight, 1.0);
    #else
        lightDir.w = clamp(1.0 - position.w * dist * posLight, 0.0, 1.0);
    #endif
    lightDir.xyz = tempVec / vec3(dist);
}

/*
* Computes the spot falloff for a spotlight
*/
float computeSpotFalloff(in vec4 lightDirection, in vec3 lightVector)
{
    vec3 L = normalize(lightVector);
    vec3 spotdir = normalize(lightDirection.xyz);
    float curAngleCos = dot(-L, spotdir);
    float innerAngleCos = floor(lightDirection.w) * 0.001;
    float outerAngleCos = fract(lightDirection.w);
    float innerMinusOuter = innerAngleCos - outerAngleCos;
    float falloff = clamp((curAngleCos - outerAngleCos) / innerMinusOuter, step(lightDirection.w, 0.001), 1.0);

    #ifdef SRGB
        // Use quadratic falloff (notice the ^4)
        return pow(clamp((curAngleCos - outerAngleCos) / innerMinusOuter, 0.0, 1.0), 4.0);
    #else
        // Use linear falloff
        return falloff;
    #endif
}