#ifndef RAYMARCH_H_INCLUDED
#define RAYMARCH_H_INCLUDED

static const float MAX_DIST = 5000;
const int ITERATIONS = 100;
const float Pi = 3.14159;
#define MAX_OBJ_COUNT 150
#define TEXTURE_SIZE 512.0
#define THREADX 8
#define THREADY 8
#define THREADZ 1
#define THREAD_NUMBER (THREADX * THREADY * THREADZ)
#define QUEUE_SIZE 10
#define MATERIAL_MAX 20
uint threadId;

RWTexture2D<float4> Result;
Texture2D<float4> ScreenData;
Texture2D<float> DepthData;

struct RayMarchingObject
{
    int type;
    int group;
    float3 position;
    float3 rotation;
    float3 scale;
    float otherParams[12];
    int3 repetitionCount;
    int3 repetitionCarve1;
    int3 repetitionCarve2;
    float3 repetitionRotation;
    float3 repetitionOffset;
    int fixArtifacts;
};

struct RayMarchingMaterial
{
    float3 color;
    float roughness;
    float specular;
    float3 specularColor;
    int texFixed;
    float3 texStretch;
};

struct MaterialMix
{
    float3 color;
    float roughness;
    float specular;
    float3 specularColor;
    float3 texColor;
};

Texture2DArray textures;

RWStructuredBuffer<RayMarchingObject> Objects;
RWStructuredBuffer<RayMarchingMaterial> Materials;
int nrOfObjects;
int nrOfLights;
int lightsOffset;

float p1;
float p2;
float p3;
float p4;
float p5;

float _ShadowsMinDistance;
float _ShadowsMaxDistance;
float _ShadowsMinStep;
float _ShadowsMaxStep;
float _ShadowsIterations;
float _ShadowsSoftness;

float _AmbientOcclusionIterations;
float _AmbientOcclusionIntensity;
float _AmbientOcclusionStep;

groupshared float objProcQueue[THREAD_NUMBER][QUEUE_SIZE];
groupshared float objMaterialCoefs[THREAD_NUMBER][MATERIAL_MAX];
/* if leafe => i - object's index (minus - negative distance)
*  else     => i - group function's index,
*  k - smin or smax coefficient;
*
*  1   - min,    -1   - -min,
*  2   - max,    -2   - -max,
*  3.k - smin,   -3.k - -smin,
*  4.k - smax,   -4.k - -smax,
*
* ------------------------  Example  ------------------------
*
*  0  4  0 -2  2 -1   -  obj or func index         |    1
*  0  0  1  1  3  3   -  parent [i]                |   / \
*  0  2  0  2  0  0   -  count of childs           |  2   3  
*  0  5  2  5  4  5   -  last element in subtree   |     / \
* [0][1][2][3][4][5]  -  [i]                       |    4  -5
*/

float4 groupsTree[MAX_OBJ_COUNT * 2];
int groupsTreeSize;

float3 _Sierpinski_p[1];
float3 _Sierpinski_r;
float3 _Sierpinski_s;

float3 _Sierpinski_fr1;
float3 _Sierpinski_fr2;
float _Sierpinski_fs;

int is_VR;

float3 LeftEye;
float3 RightEye;

float4x4 _LeftEyeInvViewMatrix;
float4x4 _RightEyeInvViewMatrix;
float4x4 _LeftEyeInvProjectionMatrix;
float4x4 _RightEyeInvProjectionMatrix;

float3 _CameraFoward;
float4x4 _FrustumCornersES;

sampler2D _MainTex;
float4 _MainTex_TexelSize;

//float4x4 _CameraInvViewMatrix;
//float4x4 _CameraInvProjectionMatrix;
float3 _CameraWS;
sampler2D _CameraDepthTexture;

float2 Resolution;

float3 _Object1;
float3 _Object2;

struct rayInfo
{
    float dist;
    float3 color;
};

float mod(float a, float b)
{
    return a - floor(a / b);
}

float3 rotate(float3 p, float3 ang)
{
    float3x3 rotx, roty, rotz;
    rotx = float3x3(float3(1.0, 0.0, 0.0),
                                float3(0.0, cos(ang.x), -sin(ang.x)),
                                float3(0.0, sin(ang.x), cos(ang.x))
                                );
    roty = float3x3(float3(cos(ang.y), 0.0, sin(ang.y)),
                                float3(0.0, 1.0, 0.0),
                                float3(-sin(ang.y), 0.0, cos(ang.y))
                                );
    rotz = float3x3(float3(cos(ang.z), -sin(ang.z), 0.0),
                                float3(sin(ang.z), cos(ang.z), 0.0),
                                float3(0.0, 0.0, 1.0)
                                );

    return mul(mul(mul(p, roty), rotx), rotz); // Order is defined by Unity's rotation specific: x = [-90, 90]
}

float ndot(float2 a, float2 b)
{
    return a.x * b.x - a.y * b.y;
}

float dot2(float2 a)
{
    return dot(a, a);
}

float dot2(float3 a)
{
    return dot(a, a);
}

float min3(float3 a)
{
    return min(a.x, min(a.y, a.z));
}

float3 infNormNormalize(float3 n)
{
    n = abs(n);
    if (n.x >= n.y && n.x >= n.z)
        return float3(n.x, 0, 0);
    if (n.y >= n.x && n.y >= n.z)
        return float3(0, n.y, 0);
    if (n.z >= n.x && n.z >= n.y)
        return float3(0, 0, n.z);
    return float3(1, 1, 1);
}

float3 oneNormNormalize(float3 n)
{
    return n / (abs(n.x) + abs(n.y) + abs(n.z));
}

MaterialMix materialMul(MaterialMix m, float k)
{
    m.color *= k;
    m.roughness *= k;
    m.specular *= k;
    m.specularColor *= k;
    m.texColor *= k;
    return m;
}

MaterialMix materialAdd(MaterialMix a, MaterialMix b)
{
    a.color += b.color;
    a.roughness += b.roughness;
    a.specular += b.specular;
    a.specularColor += b.specularColor;
    a.texColor += b.texColor;
    return a;
}

MaterialMix materialMix(MaterialMix a, MaterialMix b, float k)
{
    return materialAdd(materialMul(a, k), materialMul(b, 1 - k));

}

#endif
