struct Ray
{
    float3 origin;
    float3 pos;
    float3 dir;
};
struct ScanInfo
{
    float nearDst;
    float3 normal;
    int materialKey;
};
struct TraceInfo
{
    float3 rayColor;
    float3 incomingLight;
};

// Shader input structs
struct TriObject
{
    float3 pos;
    float3 rot;
    float3 lastRot;
    float containedRadius;
    int triStart;
    int triEnd;
};
struct Tri // Triangle
{
    float3 vA;
    float3 vB;
    float3 vC;
    float3 normal;
    int materialKey;
    int parentKey;
};
struct Sphere
{
    float3 pos;
    float radius;
    int materialKey;
};
struct Material2
{
    float3 color;
    float3 specularColor;
    float brightness;
    float smoothness;
};