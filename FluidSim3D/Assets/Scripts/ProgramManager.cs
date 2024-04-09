using UnityEngine;
using Unity.Mathematics;

// Import utils from Resources.cs
using Resources;
using System;

public class ProgramManager : MonoBehaviour
{
    public float CellSizeSL;
    public int TimeStepsPerRenderFrame;
    public float RotationSpeed;
    public float PRadius;
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

            ProgramStarted = !ProgramStarted;
        }

        dtShader.SetFloat("ParticlesNum", sim.ParticlesNum);
        dtShader.SetFloat("Radius", PRadius);
        dtShader.SetFloat("ChunksNumAll", sim.ChunksNumAll);
        dtShader.SetFloat("PTypesNum", sim.PTypes.Length);
        dtShader.SetVector("ChunkGridOffset", new Vector3(render.ChunkGridOffset.x, render.ChunkGridOffset.y, render.ChunkGridOffset.z));

        Rot.y += RotationSpeed * Time.deltaTime;
        dtShader.SetVector("Rot", new Vector3(Rot.x, Rot.y, Rot.z));

        TransferParticleData();
        RunSSShader();

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
        int len = NumPoints_NextPow2;

        // Copy OccupiedChunks -> SpatialLookup
        ComputeHelper.DispatchKernel(ssShader, "PopulateSpatialLookup", len, ssShaderThreadSize);

        // Sort SpatialLookup
        int basebBlockLen = 2;
        while (basebBlockLen != 2*len) // basebBlockLen == len is the last outer iteration
        {
            int blockLen = basebBlockLen;
            while (blockLen != 1) // blockLen == 2 is the last inner iteration
            {
                bool brownPinkSort = blockLen == basebBlockLen;

                shaderHelper.UpdateSortIterationVariables(ssShader, blockLen, brownPinkSort);

                ComputeHelper.DispatchKernel(ssShader, "SortIteration", len / 2, ssShaderThreadSize);

                blockLen /= 2;
            }
            basebBlockLen *= 2;
        }

        // Set StartIndices
        ComputeHelper.DispatchKernel(ssShader, "PopulateStartIndices", len, ssShaderThreadSize);
    }

    void OnDestroy()
    {
        ComputeHelper.Release(B_SpatialLookup, B_StartIndices, B_Points);
    }
}