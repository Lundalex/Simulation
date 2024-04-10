using UnityEngine;
using Unity.Mathematics;

// Import utils from Resources.cs
using Resources;
using System;
using System.Drawing;

public class ProgramManager : MonoBehaviour
{
    public float CellSizeSL;
    public int TimeStepsPerRenderFrame;
    public float RotationSpeed;
    public float ParticleSpheresRadius;
    public float3 Rot;
    public Renderer render;
    public Simulation sim;
    public TextureCreator texture;
    public ComputeShader dtShader;
    public ComputeShader ssShader;
    public ProgramManagerShaderHelper shaderHelper;
    private const int dtShaderThreadSize = 512; // /1024
    private const int ssShaderThreadSize = 512; // /1024
    private bool ProgramStarted = false;
    [NonSerialized] public int4 NumChunks;
    [NonSerialized] public int NumChunksAll;
    [NonSerialized] public int NumPoints;
    [NonSerialized] public int NumPoints_NextPow2;
    public ComputeBuffer B_Points;
    public ComputeBuffer B_SpatialLookup;
    public ComputeBuffer B_StartIndices;

    void Awake()
    {
        InitBuffers();

        sim.ScriptSetup();
        texture.ScriptSetup();
        render.ScriptSetup();

        shaderHelper.SetSSShaderBuffers(ssShader);
        shaderHelper.SetSSSettings(ssShader);
    }

    void Update()
    {
        sim.RunTimeSteps(TimeStepsPerRenderFrame);

        if (!ProgramStarted)
        {
            dtShader.SetBuffer(0, "PDataB", sim.PDataBuffer);
            dtShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);
            dtShader.SetBuffer(0, "Points", B_Points);

            dtShader.SetBuffer(1, "Points", B_Points);
            dtShader.SetBuffer(1, "Spheres", render.B_Spheres);

            ProgramStarted = !ProgramStarted;
        }

        dtShader.SetFloat("ParticlesNum", sim.ParticlesNum);
        dtShader.SetFloat("Radius", ParticleSpheresRadius);
        dtShader.SetFloat("ChunksNumAll", sim.ChunksNumAll);
        dtShader.SetFloat("PTypesNum", sim.PTypes.Length);
        dtShader.SetVector("ChunkGridOffset", new Vector3(render.ChunkGridOffset.x, render.ChunkGridOffset.y, render.ChunkGridOffset.z));

        dtShader.SetInt("ReservedNumSpheres", render.ReservedNumSpheres);
        dtShader.SetInt("NumSpheres", sim.ParticlesNum + render.ReservedNumSpheres);

        Rot.y += RotationSpeed * Time.deltaTime;
        dtShader.SetVector("Rot", new Vector3(Rot.x, Rot.y, Rot.z));

        TransferParticleData();

        if (render.fluidRenderStyle == FluidRenderStyle.IsoSurfaceMesh) RunSSShader();
        else if (render.fluidRenderStyle == FluidRenderStyle.ParticleSpheres) TransferParticleSpheres();

        render.UpdateRendererData();
    }

    void InitBuffers()
    {
        NumPoints = sim.ParticlesNum;
        NumPoints_NextPow2 = Func.NextPow2(NumPoints);

        float3 ChunkGridDiff = render.MaxWorldBounds - render.MinWorldBounds;

        NumChunks = new(Mathf.CeilToInt(ChunkGridDiff.x / CellSizeSL),
                        Mathf.CeilToInt(ChunkGridDiff.y / CellSizeSL),
                        Mathf.CeilToInt(ChunkGridDiff.z / CellSizeSL), 0);
        NumChunks.w = NumChunks.x * NumChunks.y;
        NumChunksAll = NumChunks.x * NumChunks.y * NumChunks.z;

        ComputeHelper.CreateStructuredBuffer<float3>(ref B_Points, NumPoints);
        ComputeHelper.CreateStructuredBuffer<int2>(ref B_SpatialLookup, Func.NextPow2(NumPoints_NextPow2));
        ComputeHelper.CreateStructuredBuffer<int>(ref B_StartIndices, NumChunksAll);
    }

    void TransferParticleData()
    {
        ComputeHelper.DispatchKernel(dtShader, "TransferParticlePositionData", NumPoints, dtShaderThreadSize);
    }

    public void RunSSShader()
    {
        // Sort points (for processing by MS shader)
        ComputeHelper.SpatialSort(ssShader, NumPoints, ssShaderThreadSize);
    }

    public void TransferParticleSpheres()
    {
        render.UpdateSpheres(sim.ParticlesNum);
        ComputeHelper.DispatchKernel(dtShader, "TransferPointsData", NumPoints, dtShaderThreadSize);

        // TEMP! ! ! ! ! ! !
        float3[] test = new float3[NumPoints];
        B_Points.GetData(test);
        render.Spheres = new Sphere[sim.ParticlesNum+1];
        render.B_Spheres.GetData(render.Spheres);
        int a = 0;
    }

    void OnDestroy()
    {
        ComputeHelper.Release(B_SpatialLookup, B_StartIndices, B_Points);
    }
}