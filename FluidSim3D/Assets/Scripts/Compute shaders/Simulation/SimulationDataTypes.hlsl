struct SpringStruct
{
    int PLinkedA;
    int PLinkedB;
    float RestLength;
};
struct PTypeStruct
{
    int FluidSpringsGroup;

    float SpringPlasticity;
    float SpringTolDeformation;
    float SpringStiffness;

    float ThermalConductivity;
    float SpecificHeatCapacity;
    float FreezeThreshold;
    float VaporizeThreshold;

    float Pressure;
    float NearPressure;

    float Mass;
    float TargetDensity;
    float Damping;
    float PassiveDamping;
    float Viscosity;
    float Stickyness;
    float Gravity;

    float InfluenceRadius;
    float colorG;
};
struct PDataStruct
{
    float3 PredPosition;
    float3 Position;
    float3 Velocity;
    float3 LastVelocity;
    float Density;
    float NearDensity;
    float Temperature; // kelvin
    float TemperatureExchangeBuffer;
    int LastChunkKey_PType_POrder; // composed 3 int structure
    // POrder is dynamic, 
    // 0 <= LastChunkKey <= ChunkNum
    // 0 <= PType <= PTypesNum
};