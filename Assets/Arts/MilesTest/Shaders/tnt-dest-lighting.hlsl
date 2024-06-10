#ifndef TNT_DEST_LIGHTING_INCLUDED
#define TNT_DEST_LIGHTING_INCLUDED

#define invPI   0.3183h
#define INV_FG  0.0625h
#define MEDIUMP_FLT_MAX 65504.0h
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

void TNTLighting(half3 normal, half3 lightDir, half3 viewDir, half3 lightColor, half fZero, half roughness, half ndotv, out half3 outDiffuse, out half3 outSpecular)
{

    half3 halfVec = normalize(lightDir + viewDir);

    half ndotl = max(dot(normal, lightDir), 0.0h);
    half ndoth = max(dot(normal, halfVec), 0.0h);
    half hdotv = max(dot(viewDir, halfVec), 0.0h);

    outDiffuse = half3(ndotl, ndotl, ndotl) * lightColor;


    half alpha = roughness * roughness;

    half alpha2 = alpha * alpha;
    half sum = ((ndoth * ndoth) * (alpha2 - 1.0h) + 1.0h);
    half denom = PI * sum * sum;
    half D = alpha2 / denom;


    // Compute Fresnel function via Schlick's approximation.
    half fresnel = fZero + (1.0h - fZero) * pow(2.0h, (-5.55473h * hdotv - 6.98316h) * hdotv);
    half k = alpha * 0.5h;

    half G_V = ndotv / (ndotv * (1.0h - k) + k);
    half G_L = ndotl / (ndotl * (1.0h - k) + k);
    half G = (G_V * G_L);

    half specular = (D * fresnel * G) / (4.0h * ndotv);

    specular = saturate(specular);
    outSpecular = half3(specular, specular, specular) * lightColor;
}




#endif