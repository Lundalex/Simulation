struct Spring
{
    int linkedA;
    int linkedB;
    float restLength;
};
struct PType
{
    int fluidSpringsGroup;

    float springPlasticity;
    float springTolDeformation;
    float springStiffness;

    float thermalConductivity;
    float specificHeatCapacity;
    float freezeThreshold;
    float vaporizeThreshold;

    float pressure;
    float nearPressure;

    float mass;
    float targetDensity;
    float damping;
    float passiveDamping;
    float viscosity;
    float stickyness;
    float gravity;

    float influenceRadius;
    float colorG;
};
struct PData
{
    float3 predPos;
    float3 pos;
    float3 vel;
    float3 lastVel;
    float density;
    float nearDensity;
    float temp; // kelvin
    float tempExchangeBuffer;
    int lastChunkKey_PType_POrder; // composed 3 int structure
    // POrder is dynamic, 
    // 0 <= LastChunkKey <= ChunkNum
    // 0 <= PType <= PTypesNum
};