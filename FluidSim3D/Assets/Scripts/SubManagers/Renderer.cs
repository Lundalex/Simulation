using UnityEngine;
using Unity.Mathematics;
using System;

// Import utils from Resources.cs
using Resources;
public class Renderer : MonoBehaviour
{
#region Inspector
    [Header("Screen")]
    public float fieldOfView = 70.0f;
    public int2 Resolution = new(1920, 1080);
    public bool AccumulateFrames = false;

    [Header("Ray Marcher")]
    public int MaxStepCount = 60;
    public int RaysPerPixel = 2;
    public float HitThreshold = 0.01f;
    [Range(0.0f, 1.0f)] public float ScatterProbability = 1.0f;
    [Range(0.0f, 2.0f)] public float DefocusStrength = 0.0f;
    public float focalPlaneFactor = 16.7f; // focalPlaneFactor must be positive
    public float MaxStepSize = 0.15f;
    public float TriMeshSafety = 0.2f;
    [Range(1.0f, 3.0f)] public float DynamicTrisSafety;
    public int FrameCount = 0;
    [Range(1, 1000)] public int ChunksPerObject = 50;

    [Header("Scene")]
    public float3 MinWorldBounds = new(-40.0f, -40.0f, -40.0f);
    public float3 MaxWorldBounds = new(40.0f, 40.0f, 40.0f);
    public float CellSize = 1.0f;
    public float CellSizeMS = 1.0f;
    public float ThresholdMS = 0.5f;

    [Header("Objects")]
    public FluidRenderStyle fluidRenderStyle;
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
    public ComputeShader ppShader;
    [NonSerialized] public RenderTexture T_GridDensities;
    [NonSerialized] public RenderTexture T_SurfaceCells;
    [NonSerialized] public RenderTexture T_Result;
    [NonSerialized] public RenderTexture renderTexture; // Texture drawn to screen
    public RendererShaderHelper shaderHelper;
    public TextureCreator textureCreator;
    public TextureHelper textureHelper;
    public Simulation sim;
    public ProgramManager manager;
    public FileImporter fileImporter;
#endregion

#region Shader Settings
    private const int rmShaderThreadSize = 8; // /32
    private const int ppShaderThreadSize = 8; // /32
    private const int pcShaderThreadSize = 512; // / 1024
    private const int pcShaderThreadSize2 = 8; // /~10
    private const int ssShaderThreadSize = 512; // / 1024
    private const int msShaderThreadSize = 8; // /~10
    private const int msShaderThreadSize2 = 512; //1024
#endregion

#region Scene Objects
    public TriObject[] TriObjects;
    public Tri[] Tris;
    public Sphere[] Spheres;
    public Material2[] Materials;
    public ComputeBuffer B_TriObjects;
    public ComputeBuffer B_Tris;
    public ComputeBuffer B_Spheres;
    public ComputeBuffer B_Materials;
#endregion

#region Spatial Sort
    public ComputeBuffer B_SpatialLookup;
    public ComputeBuffer B_StartIndices;
    public ComputeBuffer B_SafeDistances;
    public ComputeBuffer AC_OccupiedChunks;
    public ComputeBuffer AC_SurfaceCells;
    public ComputeBuffer AC_FluidTriMesh;
    public ComputeBuffer CB_A;
#endregion

#region Other
    private FluidRenderStyle lastFluidRenderStyle;
    private bool ProgramStarted = false;
    private bool SettingsChanged = true;
#endregion

#region Run Time Set Variables
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
#endregion

#region Renderer
    public void ScriptSetup()
    {
        SetupCamera();

        InitializeSceneObjects();

        UpdateSpheres(1, true, false);
        UpdateTris(1, true, false);

        ResetBuffersAndSettings();

        FrameCount = 0;
        ProgramStarted = true;
    }

    void SetupCamera()
    {
        Camera.main.cullingMask = 0;
    }

    void InitializeSceneObjects()
    {
        InitializeSpheres();
        InitializeTriMeshes();
        InitializeMaterials();
        SetConstants(true);
    }

    void InitializeSpheres()
    {
        // Set Spheres data
        ReservedNumSpheres = SpheresInput.Length;
        Spheres ??= new Sphere[SpheresInput.Length];

        for (int i = 0; i < SpheresInput.Length; i++)
        {
            Spheres[i] = CreateSphereFromInput(SpheresInput[i], i == 0 ? 1 : 0);
        }
    }

    Sphere CreateSphereFromInput(Vector4 input, int materialKey)
    {
        return new Sphere
        {
            pos = new float3(input.x, input.y, input.z),
            radius = input.w,
            materialKey = materialKey
        };
    }

    void InitializeTriMeshes()
    {
        Tris = new Tri[1]; // OR: Tris = fileImporter.LoadOBJ(0, 2.0f);
        ReservedNumTris = Tris.Length;
        SetTriObjectsData();
    }

    void SetTriObjectsData()
    {
        // Set new TriObjects data
        TriObjects = new TriObject[1];
        for (int i = 0; i < TriObjects.Length; i++)
        {
            TriObjects[i] = CreateTriObject(OBJ_Pos, OBJ_Rot);
        }
        CopyPreviousTriObjectsData();
        ComputeHelper.CreateStructuredBuffer<TriObject>(ref B_TriObjects, TriObjects);
    }

    TriObject CreateTriObject(float3 position, float3 rotation)
    {
        return new TriObject
        {
            pos = position,
            rot = rotation,
            lastRot = 0,
            containedRadius = 0.0f,
            triStart = 0,
            triEnd = NumTris - 1,
        };
    }

    void CopyPreviousTriObjectsData()
    {
        if (NumTriObjects != 0)
        {
            TriObject[] LastTriObjects = new TriObject[NumTriObjects];
            B_TriObjects.GetData(LastTriObjects);

            for (int i = 0; i < TriObjects.Length; i++)
            {
                TriObjects[i].lastRot = LastTriObjects[i].lastRot;
            }
        }
    }

    void InitializeMaterials()
    {
        // Set Materials data
        Materials ??= new Material2[MatTypesInput1.Length];
        for (int i = 0; i < MatTypesInput1.Length; i++)
        {
            Materials[i] = CreateMaterialFromInput(MatTypesInput1[i], MatTypesInput2[i].x);
        }
        ComputeHelper.CreateStructuredBuffer(ref B_Materials, Materials);
    }

    Material2 CreateMaterialFromInput(Vector4 typeInput, float smoothness)
    {
        return new Material2
        {
            color = new float3(typeInput.x, typeInput.y, typeInput.z),
            specularColor = new float3(1, 1, 1),
            brightness = typeInput.w,
            smoothness = smoothness
        };
    }

    void SetConstants(bool init = false)
    {
        SetNumVariables(init);
        CalculateChunkGrid();
    }

    void SetNumVariables(bool init)
    {
        if (init)
        {
            DynamicNumSpheres = 1;
            DynamicNumTris = 1;
        }
        NumTriObjects = TriObjects.Length;
        NumSpheres = ReservedNumSpheres + DynamicNumSpheres;
        NumTris = ReservedNumTris + DynamicNumTris;
        NumObjects = NumSpheres + NumTris;
        NumObjects_NextPow2 = Func.NextPow2(NumObjects);
    }

    void CalculateChunkGrid()
    {
        float3 ChunkGridDiff = MaxWorldBounds - MinWorldBounds;
        NumChunks = CalculateNumChunks4(ChunkGridDiff, CellSize);
        NumChunksAll = NumChunks.x * NumChunks.y * NumChunks.z;
        NumCellsMS = CalculateNumChunks3(ChunkGridDiff, CellSizeMS);

        ChunkGridOffset = new float3(
            Mathf.Max(-MinWorldBounds.x, 0.0f),
            Mathf.Max(-MinWorldBounds.y, 0.0f),
            Mathf.Max(-MinWorldBounds.z, 0.0f));
    }

    int4 CalculateNumChunks4(float3 chunkGridDiff, float chunkSize)
    {
        int xChunks = Mathf.CeilToInt(chunkGridDiff.x / chunkSize);
        int yChunks = Mathf.CeilToInt(chunkGridDiff.y / chunkSize);
        int zChunks = Mathf.CeilToInt(chunkGridDiff.z / chunkSize);
        int wChunks = xChunks * yChunks;
        return new int4(xChunks, yChunks, zChunks, wChunks);
    }

    int3 CalculateNumChunks3(float3 chunkGridDiff, float chunkSize)
    {
        int xChunks = Mathf.CeilToInt(chunkGridDiff.x / chunkSize);
        int yChunks = Mathf.CeilToInt(chunkGridDiff.y / chunkSize);
        int zChunks = Mathf.CeilToInt(chunkGridDiff.z / chunkSize);
        return new int3(xChunks, yChunks, zChunks);
    }

    public void UpdateSpheres(int newDynamicNumSpheres, bool overrideCheck = false, bool resetAll = true)
    {
        if (newDynamicNumSpheres > DynamicNumSpheres || overrideCheck)
        {
            DynamicNumSpheres = newDynamicNumSpheres;
            NumSpheres = ReservedNumSpheres + DynamicNumSpheres;
            NumObjects = NumSpheres + NumTris;
            NumObjects_NextPow2 = Func.NextPow2(NumObjects);

            if (resetAll)
            {
                ResetBuffersAndSettings();
            }

            UpdateSpheresBuffer();
            // Debug.Log("New NumSpheres: " + NumSpheres);
            // Debug.Log("New NumObjects: " + NumObjects);
            shaderHelper.UpdateSphereSettings(manager.dtShader, pcShader, rmShader, ssShader);
        }
    }

    void UpdateSpheresBuffer()
    {
        ComputeHelper.CreateStructuredBuffer<Sphere>(ref B_Spheres, NumSpheres);
        CopyLastSpheresData();
        B_Spheres.SetData(Spheres);
    }

    void CopyLastSpheresData()
    {
        Sphere[] lastSpheres = Spheres;
        Spheres = new Sphere[NumSpheres];
        Array.Copy(lastSpheres, Spheres, Math.Min(lastSpheres.Length, Spheres.Length));
    }

    void UpdateTris(int newDynamicNumTris, bool overrideCheck = false, bool resetAll = true)
    {
        if (newDynamicNumTris > DynamicNumTris || overrideCheck)
        {
            DynamicNumTris = Mathf.Max(newDynamicNumTris, Mathf.CeilToInt(newDynamicNumTris * DynamicTrisSafety));
            NumTris = ReservedNumTris + DynamicNumTris;
            NumObjects = NumSpheres + NumTris;
            NumObjects_NextPow2 = Func.NextPow2(NumObjects);

            if (resetAll)
            {
                ResetBuffersAndSettings();
            }

            UpdateTrisBuffer();
            // Debug.Log("New NumTris: " + NumTris);
            // Debug.Log("New NumObjects: " + NumObjects);
            shaderHelper.UpdateTriSettings(mcShader, pcShader, rmShader, ssShader);
        }
    }

    void UpdateTrisBuffer()
    {
        ComputeHelper.CreateStructuredBuffer<Tri>(ref B_Tris, NumTris);
        CopyLastTrisData();
        B_Tris.SetData(Tris);
    }

    void CopyLastTrisData()
    {
        Tri[] lastTris = Tris;
        Tris = new Tri[NumTris];
        Array.Copy(lastTris, Tris, Math.Min(lastTris.Length, Tris.Length));
    }

    void ResetBuffersAndSettings()
    {
        SetConstants();
        ResetBuffersAndTextures();
        SetShaderBuffersAndSettings();
    }

    void ResetBuffersAndTextures()
    {
        OnDestroy();

        InitializeStructuredBuffers();
        InitializeAppendBuffers();
        InitializeTextures();
    }

    void InitializeStructuredBuffers()
    {
        B_Spheres = ComputeHelper.CreateStructuredBuffer<Sphere>(Spheres);
        B_Materials = ComputeHelper.CreateStructuredBuffer<Material2>(Materials);

        B_Tris = ComputeHelper.CreateStructuredBuffer<Tri>(Tris);
        B_TriObjects = ComputeHelper.CreateStructuredBuffer<TriObject>(TriObjects);

        B_SpatialLookup = ComputeHelper.CreateStructuredBuffer<int2>(Func.NextPow2(NumObjects * ChunksPerObject));
        B_StartIndices = ComputeHelper.CreateStructuredBuffer<int>(NumChunksAll);
        B_SafeDistances = ComputeHelper.CreateStructuredBuffer<float>(NumChunksAll);
    }

    void InitializeAppendBuffers()
    {
        CB_A = ComputeHelper.CreateCountBuffer();
        AC_OccupiedChunks = ComputeHelper.CreateAppendBuffer<int2>(Func.NextPow2(NumObjects * ChunksPerObject));
        AC_SurfaceCells = ComputeHelper.CreateAppendBuffer<int3>(Func.NextPow2((int)(NumCellsMS.x * NumCellsMS.y * NumCellsMS.z * TriMeshSafety)));
        AC_FluidTriMesh = ComputeHelper.CreateAppendBuffer<Tri2>(Func.NextPow2((int)(NumCellsMS.x * NumCellsMS.y * NumCellsMS.z * TriMeshSafety * 3)));
    }

    void InitializeTextures()
    {
        T_GridDensities = TextureHelper.CreateTexture(NumCellsMS, 1);
        T_SurfaceCells = TextureHelper.CreateTexture(NumCellsMS - 1, 1);
        T_Result = TextureHelper.CreateTexture(Resolution, 3);
        renderTexture = TextureHelper.CreateTexture(Resolution, 3);
    }

    void SetShaderBuffersAndSettings()
    {
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

        // PostProcessing
        shaderHelper.UpdatePPVariables(ppShader);
        shaderHelper.SetPPSettings(ppShader);
    }

    public void UpdateRendererData()
    {
        FrameCount++;
        shaderHelper.UpdateRMVariables(rmShader);
        shaderHelper.UpdateNGVariables(ngShader);
        shaderHelper.UpdatePPVariables(ppShader);
    }

    private void OnValidate()
    {
        if (ProgramStarted)
        {
            InitializeSceneObjects();

            // Reset variables / buffers on a new chosen fluid render style
            if (fluidRenderStyle != lastFluidRenderStyle)
            {
                if (fluidRenderStyle != FluidRenderStyle.ParticleSpheres) UpdateSpheres(1, true);
                if (fluidRenderStyle != FluidRenderStyle.IsoSurfaceMesh) UpdateTris(1, true);
                lastFluidRenderStyle = fluidRenderStyle;
            }

            FrameCount = 0;
            SettingsChanged = true;
        }
    }

    // COMPUTE SHADER DISPATCH

    public void RunSSShader()
    {
        ComputeHelper.DispatchKernel(ssShader, "PrepStartIndices", NumChunksAll, ssShaderThreadSize);

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
        if (SettingsChanged) SettingsChanged = false; ComputeHelper.DispatchKernel(pcShader, "SetLastRotations", NumTriObjects, pcShaderThreadSize);

        // Safe distances
        ComputeHelper.DispatchKernel(pcShader, "SafeDistancesPass", NumChunks.xyz, pcShaderThreadSize2);

        // var test = ComputeHelper.GetStructuredBufferData<float>(B_SafeDistances);
        // int a = 0;
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
    }

    public void RunPPShader()
    {
        if (AccumulateFrames) ComputeHelper.DispatchKernel(ppShader, "AccumulateFrames", Resolution, ppShaderThreadSize);
        else textureHelper.Copy(ref renderTexture, T_Result, Resolution);

        if (textureCreator.RenderNoiseTextures) ComputeHelper.DispatchKernel(ppShader, "RenderNoiseTextures", Resolution, ppShaderThreadSize);
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (fluidRenderStyle == FluidRenderStyle.IsoSurfaceMesh) RunMCShader(); // MarchingCubes
        RunPCShader(); // PreCalc
        RunSSShader(); // SpatialSort
        RunRMShader(); // RayMarcher
        RunPPShader(); // PostProcessing

        Graphics.Blit(renderTexture, dest);
    }

    void OnDestroy()
    {
        ComputeHelper.Release(B_TriObjects, B_Tris, B_Spheres, B_Materials, B_SpatialLookup, B_StartIndices, B_SafeDistances, AC_OccupiedChunks, AC_SurfaceCells, AC_FluidTriMesh, CB_A);
    }
#endregion
}