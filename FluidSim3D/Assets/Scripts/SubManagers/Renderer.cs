using UnityEngine;
using Unity.Mathematics;
using System;

// Import utils from Resources.cs
using Resources;

public class Renderer : MonoBehaviour
{
    [Header("Render settings")]
    public FluidRenderStyle fluidRenderStyle;
    public float fieldOfView = 70.0f;
    public int2 Resolution = new(1920, 1080);

    [Header("RM settings")]
    public int MaxStepCount = 60;
    public int RaysPerPixel = 2;
    public float HitThreshold = 0.01f;
    [Range(0.0f, 1.0f)] public float ScatterProbability = 1.0f;
    [Range(0.0f, 2.0f)] public float DefocusStrength = 0.0f;
    public float focalPlaneFactor = 16.7f; // focalPlaneFactor must be positive
    public float MaxStepSize = 0.15f;
    public float TriMeshSafety = 0.2f;
    public int FrameCount = 0;
    [Range(1, 1000)] public int ChunksPerObject = 50;

    [Header("Scene settings")]
    public float3 MinWorldBounds = new(-40.0f, -40.0f, -40.0f);
    public float3 MaxWorldBounds = new(40.0f, 40.0f, 40.0f);
    public float CellSize = 1.0f;
    public float CellSizeMS = 1.0f;
    public float ThresholdMS = 0.5f;

    [Header("Scene objects")]
    public float3 OBJ_Pos;
    public float3 OBJ_Rot;
    public float4[] SpheresInput; // xyz: pos; w: radii
    public float4[] MatTypesInput1; // xyz: emissionColor; w: emissionStrength
    public float4[] MatTypesInput2; // x: smoothness

    [Header("References")]
    public ComputeShader rmShader;
    public ComputeShader pcShader;
    public ComputeShader ssShader;
    public ComputeShader mcShader;
    public ComputeShader ngShader;
    [NonSerialized] public RenderTexture renderTexture; // Texture drawn to screen
    [NonSerialized] public RenderTexture T_GridDensities;
    [NonSerialized] public RenderTexture T_SurfaceCells;
    public RendererShaderHelper shaderHelper;
    public TextureCreator textureCreator;
    public Simulation sim;
    public ProgramManager manager;
    public Mesh LoadOBJMesh;

    // Shader settings
    private const int rmShaderThreadSize = 8; // /32
    private const int pcShaderThreadSize = 512; // / 1024
    private const int ssShaderThreadSize = 512; // / 1024
    private const int msShaderThreadSize = 8; // /~10
    private const int msShaderThreadSize2 = 512; //1024

    // Non-inpector-accessible variables

    // Scene objects
    public TriObject[] TriObjects;
    public Tri[] Tris;
    public Sphere[] Spheres;
    public Material2[] Materials;
    public ComputeBuffer B_TriObjects;
    public ComputeBuffer B_Tris;
    public ComputeBuffer B_Spheres;
    public ComputeBuffer B_Materials;

    // Spatial sort
    public ComputeBuffer B_SpatialLookup;
    public ComputeBuffer B_StartIndices;
    public ComputeBuffer AC_OccupiedChunks;
    public ComputeBuffer AC_SurfaceCells;
    public ComputeBuffer AC_FluidTriMesh;
    public ComputeBuffer CB_A;
    private bool ProgramStarted = false;
    private bool SettingsChanged = true;

    // Constants calculated at start
    [NonSerialized] public int NumObjects;
    [NonSerialized] public int ReservedNumSpheres;
    [NonSerialized] public int DynamicNumSpheres;
    [NonSerialized] public int NumSpheres;
    [NonSerialized] public int NumTriObjects;
    [NonSerialized] public int ReservedNumTris;
    [NonSerialized] public int DynamicNumTris;
    [NonSerialized] public int NumTris;
    [NonSerialized] public int NumObjects_NextPow2;
    [NonSerialized] public int4 NumChunks;
    [NonSerialized] public int3 NumCellsMS;
    [NonSerialized] public int NumChunksAll;
    [NonSerialized] public float3 ChunkGridOffset;

    public void ScriptSetup()
    {
        Camera.main.cullingMask = 0;
        FrameCount = 0;

        SetSceneObjects();
        LoadOBJ();

        SetConstants(true);

        InitBuffers();

        // PreCalc
        shaderHelper.SetPCShaderBuffers(pcShader);

        // SpatialSort
        shaderHelper.SetSSSettings(ssShader);
        shaderHelper.SetPCSettings(pcShader);

        // NoiseGenerator
        shaderHelper.SetNGShaderTextures(ngShader);
        shaderHelper.SetNGSettings(ngShader);

        // Marching Squares
        shaderHelper.SetMSShaderBuffers(mcShader);
        shaderHelper.SetMSShaderSettings(mcShader);

        // RayMarcher
        shaderHelper.UpdateRMVariables(rmShader);
        shaderHelper.SetRMSettings(rmShader);

        ProgramStarted = true;
    }
    
    void LoadOBJ()
    {
        // Vector3[] vertices = LoadOBJMesh.vertices;
        // int[] triangles = LoadOBJMesh.triangles;
        // int triNum = triangles.Length / 3;
        // ReservedNumTris = triNum;

        // // Set Tris data
        // Tris = new Tri[triNum];
        // for (int triCount = 0; triCount < triNum; triCount++)
        // {
        //     int triCount3 = 3 * triCount;
        //     int indexA = triangles[triCount3];
        //     int indexB = triangles[triCount3 + 1];
        //     int indexC = triangles[triCount3 + 2];

        //     Tris[triCount] = new Tri
        //     {
        //         vA = vertices[indexA] * 1.5f,
        //         vB = vertices[indexB] * 1.5f,
        //         vC = vertices[indexC] * 1.5f,
        //         normal = new float3(0.0f, 0.0f, 0.0f), // init data
        //         materialKey = 1,
        //         parentKey = 0,
        //     };
        // }
        // ComputeHelper.CreateStructuredBuffer<Tri>(ref B_Tris, Tris);

        // NO LOAD
        Tris = new Tri[1];
        ComputeHelper.CreateStructuredBuffer<Tri>(ref B_Tris, 1);
        ReservedNumTris = 1;

        SetTriObjectData();
    }

    void SetConstants(bool overrideCheck = false)
    {
        NumTriObjects = TriObjects.Length;

        UpdateSpheres(30000, overrideCheck);

        UpdateTris(20000, overrideCheck); // IF THE FLUID MESH GLITCHES: INCREASE THIS NUMBER

        float3 ChunkGridDiff = MaxWorldBounds - MinWorldBounds;
        NumChunks = new(Mathf.CeilToInt(ChunkGridDiff.x / CellSize),
                        Mathf.CeilToInt(ChunkGridDiff.y / CellSize),
                        Mathf.CeilToInt(ChunkGridDiff.z / CellSize), 0);
        NumChunks.w = NumChunks.x * NumChunks.y;
        NumChunksAll = NumChunks.x * NumChunks.y * NumChunks.z;

        NumCellsMS = new(Mathf.CeilToInt(ChunkGridDiff.x / CellSizeMS),
                        Mathf.CeilToInt(ChunkGridDiff.y / CellSizeMS),
                        Mathf.CeilToInt(ChunkGridDiff.z / CellSizeMS));

        ChunkGridOffset = new float3(
            Mathf.Max(-MinWorldBounds.x, 0.0f),
            Mathf.Max(-MinWorldBounds.y, 0.0f),
            Mathf.Max(-MinWorldBounds.z, 0.0f)
        );

        // Vector3 a = new(1, 1.1f, 1);
        // Vector3 b = new(2, 1, 1.1f);
        // Vector3 c = new(2, 2, 1);
        // Vector3 p = new(1.5f, 1.9f, 1.1f);

        // Vector2 uv = Func.TriUV(a, b, c, p);
        // Debug.Log("UV Coordinates of Point p: " + uv);
    }

    void UpdateTris(int newDynamicNumTris, bool overrideCheck = false)
    {
        if (newDynamicNumTris > DynamicNumTris || overrideCheck)
        {
            DynamicNumTris = newDynamicNumTris;
            NumTris = ReservedNumTris + newDynamicNumTris;
            NumObjects = NumSpheres + NumTris;
            NumObjects_NextPow2 = Func.NextPow2(NumObjects);

            B_Tris.GetData(Tris);
            ComputeHelper.CreateStructuredBuffer<Tri>(ref B_Tris, NumTris);
            B_Tris.SetData(Tris);
            Tris = new Tri[NumTris];
            Debug.Log("New NumTris: " + NumTris);

            shaderHelper.UpdateTriSettings(mcShader, pcShader, rmShader, ssShader);
        }
    }

    public void UpdateSpheres(int newDynamicNumSpheres, bool overrideCheck = false)
    {
        if (newDynamicNumSpheres > DynamicNumSpheres || overrideCheck)
        {
            DynamicNumSpheres = newDynamicNumSpheres;
            NumSpheres = ReservedNumSpheres + newDynamicNumSpheres;
            NumObjects = NumSpheres + NumTris;
            NumObjects_NextPow2 = Func.NextPow2(NumObjects);

            B_Spheres.GetData(Spheres);
            ComputeHelper.CreateStructuredBuffer<Sphere>(ref B_Spheres, NumSpheres);
            B_Spheres.SetData(Spheres);
            Spheres = new Sphere[NumSpheres];
            Debug.Log("New NumSpheres: " + NumSpheres);

            shaderHelper.UpdateSphereSettings(manager.dtShader, pcShader, rmShader, ssShader);
        }
    }

    void InitBuffers()
    {
        ComputeHelper.CreateStructuredBuffer<int2>(ref B_SpatialLookup, Func.NextPow2(NumObjects * ChunksPerObject));
        ComputeHelper.CreateStructuredBuffer<int>(ref B_StartIndices, NumChunksAll);

        ComputeHelper.CreateAppendBuffer<int2>(ref AC_OccupiedChunks, Func.NextPow2(NumObjects * ChunksPerObject));
        ComputeHelper.CreateCountBuffer(ref CB_A);

        TextureHelper.CreateTexture(ref T_GridDensities, NumCellsMS, 1);
        TextureHelper.CreateTexture(ref T_SurfaceCells, NumCellsMS - 1, 1);
        ComputeHelper.CreateAppendBuffer<int3>(ref AC_SurfaceCells, Func.NextPow2((int)(NumCellsMS.x*NumCellsMS.y*NumCellsMS.z * TriMeshSafety)));
        ComputeHelper.CreateAppendBuffer<Tri2>(ref AC_FluidTriMesh, Func.NextPow2((int)(NumCellsMS.x*NumCellsMS.y*NumCellsMS.z * TriMeshSafety * 3)));

        TextureHelper.CreateTexture(ref renderTexture, Resolution, 3);
    }

    void SetTriObjectData()
    {
        // Set new TriObjects data
        TriObjects = new TriObject[1];
        for (int i = 0; i < TriObjects.Length; i++)
        {
            TriObjects[i] = new TriObject
            {
                pos = OBJ_Pos,
                rot = OBJ_Rot,
                lastRot = 0,
                containedRadius = 0.0f,
                triStart = 0,
                triEnd = NumTris - 1,
            };
        }

        // Fill in relevant previous TriObjects data
        if (NumTriObjects != 0)
        {
            TriObject[] LastTriObjects = new TriObject[NumTriObjects];
            B_TriObjects.GetData(LastTriObjects);

            for (int i = 0; i < TriObjects.Length; i++)
            {
                TriObjects[i].lastRot = LastTriObjects[i].lastRot;
            }
        }

        ComputeHelper.CreateStructuredBuffer<TriObject>(ref B_TriObjects, TriObjects);
    }

    public void UpdateRendererData()
    {
        FrameCount++;
        shaderHelper.UpdateRMVariables(rmShader);
        shaderHelper.UpdateNGVariables(ngShader);
    }

    private void OnValidate()
    {
        if (ProgramStarted)
        {
            FrameCount = 0;

            SetConstants();
            UpdateSettings();

            SettingsChanged = true;
        }
    }

    void UpdateSettings()
    {
        SetTriObjectData();
        SetSceneObjects();
        shaderHelper.SetRMSettings(rmShader);
    }

    void SetSceneObjects()
    {
        // Set Spheres data
        ReservedNumSpheres = SpheresInput.Length;
        Spheres = new Sphere[SpheresInput.Length];
        for (int i = 0; i < Spheres.Length; i++)
        {
            Spheres[i] = new Sphere
            {
                pos = new float3(SpheresInput[i].x, SpheresInput[i].y, SpheresInput[i].z),
                radius = SpheresInput[i].w,
                materialKey = i == 0 ? 1 : 0
            };
        }
        ComputeHelper.CreateStructuredBuffer<Sphere>(ref B_Spheres, Spheres);

        // Set Materials data
        Materials = new Material2[MatTypesInput1.Length];
        for (int i = 0; i < Materials.Length; i++)
        {
            Materials[i] = new Material2
            {
                color = new float3(MatTypesInput1[i].x, MatTypesInput1[i].y, MatTypesInput1[i].z),
                specularColor = new float3(1, 1, 1), // Specular color is currently set to white for all Material2 types
                brightness = MatTypesInput1[i].w,
                smoothness = MatTypesInput2[i].x
            };
        }
        ComputeHelper.CreateStructuredBuffer<Material2>(ref B_Materials, Materials);
    }
    
    public void RunSSShader()
    {
        // Fill OccupiedChunks
        AC_OccupiedChunks.SetCounterValue(0);
        ComputeHelper.DispatchKernel(ssShader, "CalcSphereChunkKeys", NumSpheres, ssShaderThreadSize);
        ComputeHelper.DispatchKernel(ssShader, "CalcTriChunkKeys", NumTris, ssShaderThreadSize);

        // Get OccupiedChunks length
        // GetAppendBufferCount() is expensive since it requires data to be sent from the GPU to the CPU!
        int occupiedChunksLength = ComputeHelper.GetAppendBufferCount(AC_OccupiedChunks, CB_A);

        ComputeHelper.SpatialSort(ssShader, occupiedChunksLength, ssShaderThreadSize);
    }

    public void RunPCShader()
    {
        ComputeHelper.DispatchKernel(pcShader, "CalcTriNormals", NumTris, pcShaderThreadSize);
        if (SettingsChanged) { SettingsChanged = false; ComputeHelper.DispatchKernel(pcShader, "SetLastRotations", NumTriObjects, pcShaderThreadSize); }
    }

    public void RunMCShader()
    {
        ComputeHelper.DispatchKernel(mcShader, "CalcGridDensities", NumCellsMS.xyz, msShaderThreadSize);
        AC_SurfaceCells.SetCounterValue(0);
        ComputeHelper.DispatchKernel(mcShader, "FindSurface", NumCellsMS.xyz, msShaderThreadSize);

        int SC_len = ComputeHelper.GetAppendBufferCount(AC_SurfaceCells, CB_A);

        AC_FluidTriMesh.SetCounterValue(0);
        ComputeHelper.DispatchKernel(mcShader, "GenerateFluidMesh", Mathf.Max(SC_len, 1), msShaderThreadSize2);

        int FTM_len = ComputeHelper.GetAppendBufferCount(AC_FluidTriMesh, CB_A);

        UpdateTris(FTM_len);

        ComputeHelper.DispatchKernel(mcShader, "DeleteFluidMesh", Mathf.Max(DynamicNumTris, 1), msShaderThreadSize2);
        ComputeHelper.DispatchKernel(mcShader, "TransferFluidMesh", Mathf.Max(FTM_len, 1), msShaderThreadSize2);
    }

    public void RunRMShader()
    {
        ComputeHelper.DispatchKernel(rmShader, "TraceRays", Resolution, rmShaderThreadSize);

        if (textureCreator.RenderNoiseTextures) {ComputeHelper.DispatchKernel(rmShader, "RenderNoiseTextures", Resolution, rmShaderThreadSize); }
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (fluidRenderStyle == FluidRenderStyle.IsoSurfaceMesh) RunMCShader(); // MarchingCubes
        RunPCShader(); // PreCalc
        RunSSShader(); // SpatialSort
        RunRMShader(); // RayMarcher

        Graphics.Blit(renderTexture, dest);
    }

    void OnDestroy()
    {
        ComputeHelper.Release(B_TriObjects, B_Tris, B_Spheres, B_Materials, B_SpatialLookup, B_StartIndices, AC_OccupiedChunks, AC_SurfaceCells, AC_FluidTriMesh, CB_A);
    }
}