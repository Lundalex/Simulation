using UnityEngine;
using Unity.Mathematics;
using System;

// Import utils from Resources.cs
using Resources;
using UnityEditor;
public class Simulation : MonoBehaviour
{
    [Header("Simulation settings")]
    public int ParticlesNum = 30000; 
    public int MaxInfluenceRadius = 2;
    public float TargetDensity = 2.0f;
    public float PressureMultiplier = 3000;
    public float NearPressureMultiplier = 12.0f;
    [Range(0, 1)] public float Damping = 0.7f;
    [Range(0, 3.0f)] public float PassiveDamping = 0.0f;
    [Range(0, 0.1f)] public float LookAheadFactor = 0.017f;
    [Range(0, 5.0f)] public float StateThresholdPadding = 3.0f;
    public float Viscosity = 1.5f;
    public float SpringStiffness = 5.0f;
    public float TolDeformation = 0.0f;
    public float Plasticity = 3.0f;
    public float Gravity = 5.0f;
    public float SpringCapacitySafety;
    [Range(0, 3)] public int MaxChunkSearchSafety = 1;

    [Header("Boundrary settings")]
    public int Width = 300;
    public int Height = 200;
    public int Depth = 50;
    public float BorderPadding = 4.0f;

    [Header("Render settings")]
    public bool FixedTimeStep = true;
    public bool RenderMarchingSquares = false;
    public float TimeStep = 0.02f;
    public float ProgramSpeed = 2.0f;
    public int SubTimeStepsNum = 3;

    [Header("Interaction settings")]
    public float MaxInteractionRadius = 40.0f;
    public float InteractionAttractionPower = 3.5f;
    public float InteractionFountainPower = 0.0f;
    public float InteractionTemperaturePower = 0.0f;

    [Header("References")]
    public SimulationShaderHelper shaderHelper;
    public ComputeShader pSimShader;
    public ComputeShader ssShader;
    public ComputeShader ipsShader;

    // Non-inpector-accessible variables

    // ThreadSize settings for compute shaders
    [NonSerialized] public const int renderShaderThreadSize = 32; // /32, AxA thread groups
    [NonSerialized] public const int pSimShaderThreadSize = 256; // /1024
    [NonSerialized] public const int ssShaderThreadSize = 512; // /1024
    [NonSerialized] public const int ipsShaderThreadSize = 512; // /1024

    // Bitonic mergesort
    public ComputeBuffer SpatialLookupBuffer;
    public ComputeBuffer StartIndicesBuffer;

    // Inter-particle springs
    public ComputeBuffer SpringCapacitiesBuffer;
    public ComputeBuffer SpringStartIndicesBuffer_dbA; // Result A
    public ComputeBuffer SpringStartIndicesBuffer_dbB; // Result B
    public ComputeBuffer SpringStartIndicesBuffer_dbC; // Support
    public ComputeBuffer ParticleSpringsCombinedBuffer; // [[Last frame springs], [New frame springs]]

    // Particle data
    public ComputeBuffer PDataBuffer;
    public ComputeBuffer PTypesBuffer;

    // Constants
    [NonSerialized] public int MaxInfluenceRadiusSqr;
    [NonSerialized] public float InvMaxInfluenceRadius;
    [NonSerialized] public int4 ChunksNum; // x,y,z,x*y
    [NonSerialized] public int ChunksNumAll;
    [NonSerialized] public int ChunksNumAllNextPow2;
    [NonSerialized] public int ParticlesNum_NextPow2;
    [NonSerialized] public int ParticleSpringsCombinedHalfLength;

    // Particle data
    private PData[] PData;
    [NonSerialized] public PType[] PTypes;

    // Other
    private float DeltaTime;
    private bool FrameBufferCycle = true;
    private bool ProgramStarted = false;

    public void ScriptSetup ()
    {
        SetConstants();
        InitializeArrays();
        SetPTypesData();

        for (int i = 0; i < ParticlesNum; i++) {
            PData[i].Position = Utils.GetParticleSpawnPosition(i, ParticlesNum, Width, Height, Depth);
        }

        InitializeBuffers();
        shaderHelper.SetPSimShaderBuffers(pSimShader);
        shaderHelper.SetSSShaderBuffers(ssShader);
        shaderHelper.SetIPSShaderBuffer(ipsShader);

        shaderHelper.UpdatePSimShaderVariables(pSimShader);
        shaderHelper.UpdateSSShaderVariables(ssShader);
        shaderHelper.UpdateIPSShaderVariables(ipsShader);

        ProgramStarted = true;
    }

    public void RunTimeSteps(int timeStepsNum)
    {
        for (int timeStepCount = 0; timeStepCount < timeStepsNum; timeStepCount++)
        {
            UpdateShaderTimeStep();

            RunSSShader();
            RunIPSShader();

            for (int subTimeStepCount = 0; subTimeStepCount < SubTimeStepsNum; subTimeStepCount++)
            {
                pSimShader.SetBool("TransferSpringData", subTimeStepCount == 0);

                RunPSimShader();

                ComputeHelper.DispatchKernel (pSimShader, "UpdatePositions", ParticlesNum, pSimShaderThreadSize);
            }
        }
    }

    private void OnValidate()
    {
        if (ProgramStarted)
        {
            SetConstants();
            UpdateSettings();
        }
    }

    public void UpdateSettings()
    {
        SetPTypesData();
        PTypesBuffer.SetData(PTypes);

        shaderHelper.UpdatePSimShaderVariables(pSimShader);
        shaderHelper.UpdateSSShaderVariables(ssShader);
        shaderHelper.UpdateIPSShaderVariables(ipsShader);
    }
    
    public void UpdateShaderTimeStep()
    {
        DeltaTime = GetDeltaTime();
        
        Vector2 mouseWorldPos = Utils.GetMousePosNormalised() * new Vector2(Width, Height);
        // bool2(Left?, Right?)
        bool2 mousePressed = Utils.GetMousePressed();

        pSimShader.SetFloat("DeltaTime", DeltaTime);
        pSimShader.SetFloat("MouseX", mouseWorldPos.x);
        pSimShader.SetFloat("MouseY", mouseWorldPos.y);
        pSimShader.SetBool("LMousePressed", mousePressed.x);
        pSimShader.SetBool("RMousePressed", mousePressed.y);

        FrameBufferCycle = !FrameBufferCycle;
        ssShader.SetBool("FrameBufferCycle", FrameBufferCycle);
        pSimShader.SetBool("FrameBufferCycle", FrameBufferCycle);
    }

    float GetDeltaTime()
    {
        return FixedTimeStep
        ? TimeStep / SubTimeStepsNum
        : Time.deltaTime * ProgramSpeed / SubTimeStepsNum;
    }

    void SetConstants()
    {
        Func.NextDivisible(ref Height, MaxInfluenceRadius);
        Func.NextDivisible(ref Width, MaxInfluenceRadius);
        Func.NextDivisible(ref Depth, MaxInfluenceRadius);

        MaxInfluenceRadiusSqr = MaxInfluenceRadius * MaxInfluenceRadius;
        InvMaxInfluenceRadius = 1.0f / MaxInfluenceRadius;
        ChunksNum.x = Width / MaxInfluenceRadius;
        ChunksNum.y = Height / MaxInfluenceRadius;
        ChunksNum.z = Depth / MaxInfluenceRadius;
        ChunksNum.w = ChunksNum.x * ChunksNum.y;
        ChunksNumAll = ChunksNum.x * ChunksNum.y * ChunksNum.z;
        ParticlesNum_NextPow2 = Func.NextPow2(ParticlesNum);
        ParticleSpringsCombinedHalfLength = (int)(ParticlesNum * SpringCapacitySafety * 0.5);
    }

    void SetPTypesData()
    {
        PTypes = new PType[6];
        float IR_1 = 2.0f;
        float IR_2 = 2.0f;
        int FSG_1 = 1;
        int FSG_2 = 2;
        PTypes[0] = new PType // Solid
        {
            FluidSpringsGroup = 1,

            SpringPlasticity = 0,
            SpringTolDeformation = 0.1f,
            SpringStiffness = 2000,

            ThermalConductivity = 1.0f,
            SpecificHeatCapacity = 10.0f,
            FreezeThreshold = Utils.CelciusToKelvin(0.0f),
            VaporizeThreshold = Utils.CelciusToKelvin(100.0f),

            Pressure = 3000,
            NearPressure = 5,

            Mass = 1,
            TargetDensity = TargetDensity,
            Damping = Damping,
            PassiveDamping = 0.0f,
            Viscosity = 5.0f,
            Stickyness = 2.0f,
            Gravity = Gravity,

            InfluenceRadius = 2,
            colorG = 0.5f
        };
        PTypes[1] = new PType // Liquid
        {
            FluidSpringsGroup = FSG_1,

            SpringPlasticity = Plasticity,
            SpringTolDeformation = TolDeformation,
            SpringStiffness = SpringStiffness,

            ThermalConductivity = 1.0f,
            SpecificHeatCapacity = 10.0f,
            FreezeThreshold = Utils.CelciusToKelvin(0.0f),
            VaporizeThreshold = Utils.CelciusToKelvin(100.0f),
            
            Pressure = PressureMultiplier,
            NearPressure = NearPressureMultiplier,

            Mass = 1,
            TargetDensity = TargetDensity,
            Damping = Damping,
            PassiveDamping = PassiveDamping,
            Viscosity = Viscosity,
            Stickyness = 2.0f,
            Gravity = Gravity,

            InfluenceRadius = IR_1,
            colorG = 0.0f
        };
        PTypes[2] = new PType // Gas
        {
            FluidSpringsGroup = 0,

            SpringPlasticity = -1,
            SpringTolDeformation = -1,
            SpringStiffness = -1,

            ThermalConductivity = 3.0f,
            SpecificHeatCapacity = 10.0f,
            FreezeThreshold = Utils.CelciusToKelvin(0.0f),
            VaporizeThreshold = Utils.CelciusToKelvin(100.0f),

            Pressure = 200,
            NearPressure = 0,

            Mass = 0.1f,
            TargetDensity = 0,
            Damping = Damping,
            PassiveDamping = PassiveDamping,
            Viscosity = Viscosity,
            Stickyness = 2.0f,
            Gravity = Gravity * 0.1f,

            InfluenceRadius = IR_1,
            colorG = 0.3f
        };

        PTypes[3] = new PType // Solid
        {
            FluidSpringsGroup = FSG_2,

            SpringPlasticity = Plasticity,
            SpringTolDeformation = TolDeformation,
            SpringStiffness = SpringStiffness,

            ThermalConductivity = 7.0f,
            SpecificHeatCapacity = 15.0f,
            FreezeThreshold = Utils.CelciusToKelvin(999.0f),
            VaporizeThreshold = Utils.CelciusToKelvin(-999.0f),

            Pressure = PressureMultiplier,
            NearPressure = NearPressureMultiplier,

            Mass = 1,
            TargetDensity = TargetDensity * 1.5f,
            Damping = Damping,
            PassiveDamping = PassiveDamping,
            Viscosity = Viscosity,
            Stickyness = 4.0f,
            Gravity = Gravity,

            InfluenceRadius = IR_2,
            colorG = 0.9f
        };
        PTypes[4] = new PType // Liquid
        {
            FluidSpringsGroup = FSG_2,

            SpringPlasticity = Plasticity,
            SpringTolDeformation = TolDeformation,
            SpringStiffness = SpringStiffness,

            ThermalConductivity = 7.0f,
            SpecificHeatCapacity = 15.0f,
            FreezeThreshold = Utils.CelciusToKelvin(-999.0f),
            VaporizeThreshold = Utils.CelciusToKelvin(999.0f),

            Pressure = PressureMultiplier,
            NearPressure = NearPressureMultiplier,

            Mass = 1,
            TargetDensity = TargetDensity * 1.5f,
            Damping = Damping,
            PassiveDamping = PassiveDamping,
            Viscosity = Viscosity,
            Stickyness = 4.0f,
            Gravity = Gravity,

            InfluenceRadius = IR_2,
            colorG = 1.0f
        };
        PTypes[5] = new PType // Gas
        {
            FluidSpringsGroup = FSG_2,

            SpringPlasticity = Plasticity,
            SpringTolDeformation = TolDeformation,
            SpringStiffness = SpringStiffness,

            ThermalConductivity = 7.0f,
            SpecificHeatCapacity = 15.0f,
            FreezeThreshold = Utils.CelciusToKelvin(-999.0f),
            VaporizeThreshold = Utils.CelciusToKelvin(999.0f),

            Pressure = PressureMultiplier,
            NearPressure = NearPressureMultiplier,

            Mass = 1,
            TargetDensity = TargetDensity * 1.5f,
            Damping = Damping,
            PassiveDamping = PassiveDamping,
            Viscosity = Viscosity,
            Stickyness = 4.0f,
            Gravity = Gravity,

            InfluenceRadius = IR_2,
            colorG = 0.9f
        };
    }

    void InitializeArrays()
    {
        PData = new PData[ParticlesNum];

        for (int i = 0; i < ParticlesNum; i++)
        {
            if (i < ParticlesNum * 0.5f)
            {
                PData[i] = new PData
                {
                    PredPosition = 0,
                    Position = 0,
                    Velocity = 0,
                    LastVelocity = 0,
                    Density = 0.0f,
                    NearDensity = 0.0f,
                    Temperature = Utils.CelciusToKelvin(20.0f),
                    TemperatureExchangeBuffer = 0.0f,
                    LastChunkKey_PType_POrder = 1 * ChunksNumAll // flattened equivelant to PType = 1
                };
            }
            else
            {
                PData[i] = new PData
                {
                    PredPosition = 0,
                    Position = 0,
                    Velocity = 0,
                    LastVelocity = 0,
                    Density = 0,
                    NearDensity = 0,
                    Temperature = Utils.CelciusToKelvin(80.0f),
                    TemperatureExchangeBuffer = 0.0f,
                    LastChunkKey_PType_POrder = (3 + 1) * ChunksNumAll // flattened equivelant to PType = 3+1
                };
            }
        }
    }

    void InitializeBuffers()
    {
        ComputeHelper.CreateStructuredBuffer<PData>(ref PDataBuffer, PData);
        ComputeHelper.CreateStructuredBuffer<PType>(ref PTypesBuffer, PTypes);

        ComputeHelper.CreateStructuredBuffer<int2>(ref SpatialLookupBuffer, ParticlesNum_NextPow2);
        ComputeHelper.CreateStructuredBuffer<int>(ref StartIndicesBuffer, ChunksNumAll);
        ComputeHelper.CreateStructuredBuffer<int2>(ref SpringCapacitiesBuffer, ChunksNumAll);
        ComputeHelper.CreateStructuredBuffer<int>(ref SpringStartIndicesBuffer_dbA, ChunksNumAll);
        ComputeHelper.CreateStructuredBuffer<int>(ref SpringStartIndicesBuffer_dbB, ChunksNumAll);
        ComputeHelper.CreateStructuredBuffer<int>(ref SpringStartIndicesBuffer_dbC, ChunksNumAll);
        ComputeHelper.CreateStructuredBuffer<Spring>(ref ParticleSpringsCombinedBuffer, (int)(ParticlesNum * SpringCapacitySafety));
    }

    void RunSSShader()
    {
        ComputeHelper.SpatialSort(ssShader, ParticlesNum, ssShaderThreadSize);
    }

    void RunIPSShader()
    {
        int threadGroupsNum = Utils.GetThreadGroupsNum(ChunksNumAll, ssShaderThreadSize);

        ComputeHelper.DispatchKernel (ipsShader, "PopulateChunkSizes", threadGroupsNum);
        ComputeHelper.DispatchKernel (ipsShader, "PopulateSpringCapacities", threadGroupsNum);
        ComputeHelper.DispatchKernel (ipsShader, "CopySpringCapacities", threadGroupsNum);

        // Calculate prefix sums
        bool StepBufferCycle = false;
        for (int offset = 1; offset < ChunksNumAll; offset *= 2)
        {
            StepBufferCycle = !StepBufferCycle;

            ipsShader.SetBool("StepBufferCycle", StepBufferCycle);
            ipsShader.SetInt("Offset", offset);

            ComputeHelper.DispatchKernel (ipsShader, "ParallelPrefixSumScan", threadGroupsNum);
        }

        if (StepBufferCycle == true) { ComputeHelper.DispatchKernel (ipsShader, "CopySpringStartIndicesBuffer", threadGroupsNum); } // copy to result buffer if necessary
    }

    void RunPSimShader()
    {
        ComputeHelper.DispatchKernel (pSimShader, "PreCalculations", ParticlesNum, pSimShaderThreadSize);
        ComputeHelper.DispatchKernel (pSimShader, "CalculateDensities", ParticlesNum, pSimShaderThreadSize);
        ComputeHelper.DispatchKernel (pSimShader, "ParticleForces", ParticlesNum, pSimShaderThreadSize);
    }

    void OnDestroy()
    {
        ComputeHelper.Release(
            SpatialLookupBuffer,
            StartIndicesBuffer,
            PDataBuffer,
            PTypesBuffer,
            SpringCapacitiesBuffer,
            SpringStartIndicesBuffer_dbA,
            SpringStartIndicesBuffer_dbB,
            SpringStartIndicesBuffer_dbC,
            ParticleSpringsCombinedBuffer
        );
    }
}