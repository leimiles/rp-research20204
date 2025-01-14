#ifndef HLSL_COMPAT
#define HLSL_COMPAT

#if defined GL_ES
    #  define hfloat highp float
    #  define hvec2  highp vec2
    #  define hvec3  highp vec3
    #  define hvec4  highp vec4
    #  define lfloat lowp float
    #  define lvec2 lowp vec2
    #  define lvec3 lowp vec3
    #  define lvec4 lowp vec4
#else
    #  define hfloat float
    #  define hvec2  vec2
    #  define hvec3  vec3
    #  define hvec4  vec4
    #  define lfloat float
    #  define lvec2  vec2
    #  define lvec3  vec3
    #  define lvec4  vec4
#endif

#if __VERSION__ >= 130
    #ifdef GL_ES
        out highp vec4 outFragColor;
    #else
        out vec4 outFragColor;
    #endif
    #  define texture1D texture
    #  define texture2D texture
    #  define texture3D texture
    #  define textureCube texture
    #  define texture2DLod textureLod
    #  define textureCubeLod textureLod
    #  define texture2DArray texture
    #if defined VERTEX_SHADER
        #    define varying out
        #    define attribute in
    #elif defined FRAGMENT_SHADER
        #    define varying in
        #    define gl_FragColor outFragColor
    #endif
#else
    #  define isnan(val) !(val < 0.0 || val > 0.0 || val == 0.0)
#endif

#if __VERSION__ == 110
    mat3 mat3_sub(mat4 m)
    {
        return mat3(m[0].xyz, m[1].xyz, m[2].xyz);
    }
#else
    #define mat3_sub mat3
#endif

#if __VERSION__ <= 140
    float determinant(mat2 m)
    {
        return m[0][0] * m[1][1] - m[1][0] * m[0][1];
    }

    float determinant(mat3 m)
    {
        return +m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1])
        - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
        + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);
    }
#endif

#if __VERSION__ <= 130
    mat2 inverse(mat2 m)
    {
        return mat2(m[1][1], -m[0][1], -m[1][0], m[0][0]) / determinant(m);
    }

    mat3 inverse(mat3 m)
    {
        return mat3(
            + (m[1][1] * m[2][2] - m[2][1] * m[1][2]),
            - (m[1][0] * m[2][2] - m[2][0] * m[1][2]),
            + (m[1][0] * m[2][1] - m[2][0] * m[1][1]),
            - (m[0][1] * m[2][2] - m[2][1] * m[0][2]),
            + (m[0][0] * m[2][2] - m[2][0] * m[0][2]),
            - (m[0][0] * m[2][1] - m[2][0] * m[0][1]),
            + (m[0][1] * m[1][2] - m[1][1] * m[0][2]),
            - (m[0][0] * m[1][2] - m[1][0] * m[0][2]),
            + (m[0][0] * m[1][1] - m[1][0] * m[0][1])) / determinant(m);
    }
#endif

#endif