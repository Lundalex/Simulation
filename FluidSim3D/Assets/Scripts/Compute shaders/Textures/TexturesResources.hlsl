#include "../Constants.hlsl"

float sqr(float a)
{
	return a * a;
}

float avg(float a, float b) // float version
{
    return .5 * (a + b);
}
float2 avg(float2 a, float2 b) // float2 version
{
    return .5 * (a + b);
}
float3 avg(float3 a, float3 b) // float3 version
{
    return .5 * (a + b);
}

float dot2(float3 a) // float3 version
{
    return dot(a, a);
}
float dot2(float2 a) // float2 version
{
    return dot(a, a);
}

uint NextRandom(inout uint state)
{
    state = state * 747796405 + 2891336453;
    uint result = ((state >> ((state >> 28) + 4)) ^ state) * 277803737;
    result = (result >> 22) ^ result;
    return result;
}

float randNormalized(inout uint state)
{
    return NextRandom(state) / 4294967295.0; // 2^32 - 1
}

float randValueNormalDistribution(inout uint state)
{
    float theta = 2 * PI * randNormalized(state);
    float rho = sqrt(-2 * log(randNormalized(state)));
    return rho * cos(theta);
}

// Expensive!
float3 randPointOnUnitSphere(inout uint state)
{
    float x = randValueNormalDistribution(state);
    float y = randValueNormalDistribution(state);
    float z = randValueNormalDistribution(state);
    return normalize(float3(x, y, z));
}

float2 randPointInCircle(inout uint state)
{
    float angle = randNormalized(state) * 2 * PI;
    float2 pointOnCircle = float2(cos(angle), sin(angle));
    return pointOnCircle * sqrt(randNormalized(state));
}