using UnityEngine;
using Unity.Mathematics;
using System;

// Import utils from Resources.cs
using Resources;

public class Renderer : MonoBehaviour
{
    [Header("Render settings")]
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
    public int FrameCount = 0;
    [Range(1, 1000)] public int ChunksPerObject = 50;

    [Header("Scene settings")]
    public float3 MinWorldBounds = new(-40.0f, -40.0f, -40.0f);
    public float3 MaxWorldBounds = new(40.0f, 40.0f, 40.0f);
    public float CellSize = 1.0f;

    [Header("Scene objects")]
    public bool RenderTris = true;
    public float3 OBJ_Pos;
    public float3 OBJ_Rot;
    public float4[] SpheresInput; // xyz: pos; w: radii
    public float4[] MatTypesInput1; // xyz: emissionColor; w: emissionStrength
    public float4[] MatTypesInput2; // x: smoothness

    [Header("References")]
    public ComputeShader rmShader;
    public ComputeShader pcShader;
    public ComputeShader ssShader;
    public ComputeShader ngShader;
    [NonSerialized] public RenderTexture renderTexture; // Texture drawn to screen
    public RendererShaderHelper shaderHelper;
    public TextureCreator textureCreator;
    public Simulation sim;
    public Mesh LoadOBJMesh;

    // Shader settings
    private const int rmShaderThreadSize = 8; // /32
    private const int pcShaderThreadSize = 512; // / 1024
    private const int ssShaderThreadSize = 512; // / 1024

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
    public ComputeBuffer CB_A;
    private bool ProgramStarted = false;
    private bool SettingsChanged = true;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;

    // Constants calculated at start
    [NonSerialized] public int NumObjects;
    [NonSerialized] public int NumSpheres;
    [NonSerialized] public int NumTriObjects;
    [NonSerialized] public int NumTris;
    [NonSerialized] public int NumObjects_NextPow2;
    [NonSerialized] public int4 NumChunks;
    [NonSerialized] public int NumChunksAll;
    [NonSerialized] public float3 ChunkGridOffset;

    void Awake ()
    {
        Camera.main.cullingMask = 0;
        FrameCount = 0;
        lastCameraPosition = transform.position;

        SetSceneObjects();
        LoadOBJ();

        SetConstants();

        InitBuffers();

        // PreCalc
        shaderHelper.SetPCShaderBuffers(pcShader);

        // SpatialSort
        shaderHelper.SetSSSettings(ssShader);
        shaderHelper.SetPCSettings(pcShader);

        // NoiseGenerator
        shaderHelper.SetNGShaderTextures(ngShader);
        shaderHelper.SetNGSettings(ngShader);

        // RayMarcher
        shaderHelper.UpdateRMVariables(rmShader);
        shaderHelper.SetRMSettings(rmShader);

        ProgramStarted = true;
    }
    
    void LoadOBJ()
    {
        Vector3[] vertices = LoadOBJMesh.vertices;
        int[] triangles = LoadOBJMesh.triangles;
        int triNum = triangles.Length / 3;

        // Set Tris data
        Tris = new Tri[triNum];
        for (int triCount = 0; triCount < triNum; triCount++)
        {
            int triCount3 = 3 * triCount;
            int indexA = triangles[triCount3];
            int indexB = triangles[triCount3 + 1];
            int indexC = triangles[triCount3 + 2];

            Tris[triCount] = new Tri
            {
                vA = vertices[indexA] * 1.5f,
                vB = vertices[indexB] * 1.5f,
                vC = vertices[indexC] * 1.5f,
                normal = new float3(0.0f, 0.0f, 0.0f), // init data
                materialKey = 0,
                parentKey = 0,
            };
        }
        ComputeHelper.CreateStructuredBuffer<Tri>(ref B_Tris, Tris);

        SetTriObjectData();
    }

    void SetConstants()
    {
        NumSpheres = Spheres.Length;
        NumTris = Tris.Length;
        NumObjects = NumSpheres + NumTris;
        NumObjects_NextPow2 = Func.NextPow2(NumObjects);

        NumTriObjects = TriObjects.Length;

        float3 ChunkGridDiff = MaxWorldBounds - MinWorldBounds;
        NumChunks = new(Mathf.CeilToInt(ChunkGridDiff.x / CellSize),
                        Mathf.CeilToInt(ChunkGridDiff.y / CellSize),
                        Mathf.CeilToInt(ChunkGridDiff.z / CellSize), 0);
        NumChunks.w = NumChunks.x * NumChunks.y;
        NumChunksAll = NumChunks.x * NumChunks.y * NumChunks.z;

        ChunkGridOffset = new float3(
            Mathf.Max(-MinWorldBounds.x, 0.0f),
            Mathf.Max(-MinWorldBounds.y, 0.0f),
            Mathf.Max(-MinWorldBounds.z, 0.0f)
        );
    }

    void InitBuffers()
    {
        ComputeHelper.CreateStructuredBuffer<int2>(ref B_SpatialLookup, Func.NextPow2(NumObjects * ChunksPerObject));
        ComputeHelper.CreateStructuredBuffer<int>(ref B_StartIndices, NumChunksAll);

        ComputeHelper.CreateAppendBuffer<int2>(ref AC_OccupiedChunks, Func.NextPow2(NumObjects * ChunksPerObject));
        ComputeHelper.CreateCountBuffer(ref CB_A);

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

    void LateUpdate()
    {
        if (transform.position != lastCameraPosition || transform.rotation != lastCameraRotation)
        {
            FrameCount = 0;

            SetTriObjectData();
            SetSceneObjects();
            shaderHelper.SetRMSettings(rmShader);

            lastCameraPosition = transform.position;
            lastCameraRotation = transform.rotation;
        }
    }

    private void OnValidate()
    {
        if (ProgramStarted)
        {
            FrameCount = 0;

            SetTriObjectData();
            SetSceneObjects();
            shaderHelper.SetRMSettings(rmShader);

            SettingsChanged = true;
        }
    }

    void SetSceneObjects()
    {
        // Set Spheres data
        Spheres = new Sphere[sim.ParticlesNum];
        // for (int i = 0; i < SpheresInput.Length; i++)
        // {
        //     Spheres[i] = new Sphere
        //     {
        //         pos = new float3(SpheresInput[i].x, SpheresInput[i].y, SpheresInput[i].z),
        //         radius = SpheresInput[i].w,
        //         materialKey = i == 0 ? 1 : 0
        //     };
        // }
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

    void RunRMShader()
    {
        ComputeHelper.DispatchKernel(rmShader, "TraceRays", Resolution, rmShaderThreadSize);

        if (textureCreator.RenderNoiseTextures) {ComputeHelper.DispatchKernel(rmShader, "RenderNoiseTextures", Resolution, rmShaderThreadSize); }
    }
    
    void RunSSShader()
    {
        // Fill OccupiedChunks
        AC_OccupiedChunks.SetCounterValue(0);
        ComputeHelper.DispatchKernel(ssShader, "CalcSphereChunkKeys", NumSpheres, ssShaderThreadSize);
        if (RenderTris) { ComputeHelper.DispatchKernel(ssShader, "CalcTriChunkKeys", NumTris, ssShaderThreadSize); } 

        // Get OccupiedChunks length
        // THIS IS QUITE EXPENSIVE SINCE IT REQUIRES DATA TO BE SENT FROM THE GPU TO THE CPU!
        int OC_len = ComputeHelper.GetAppendBufferCount(AC_OccupiedChunks, CB_A);
        Func.NextPow2(ref OC_len); // NextPow2() since bitonic merge sort requires pow2 array length

        // Set NextPow2(OccupiedChunks count) in shader
        ssShader.SetInt("OC_len", OC_len);

        // Copy OccupiedChunks -> SpatialLookup
        ComputeHelper.DispatchKernel(ssShader, "PopulateSpatialLookup", OC_len, ssShaderThreadSize);

        // Sort SpatialLookup
        int basebBlockLen = 2;
        while (basebBlockLen != 2*OC_len) // basebBlockLen == len is the last outer iteration
        {
            int blockLen = basebBlockLen;
            while (blockLen != 1) // blockLen == 2 is the last inner iteration
            {
                bool brownPinkSort = blockLen == basebBlockLen;

                shaderHelper.UpdateSortIterationVariables(ssShader, blockLen, brownPinkSort);

                ComputeHelper.DispatchKernel(ssShader, "SortIteration", OC_len / 2, ssShaderThreadSize);

                blockLen /= 2;
            }
            basebBlockLen *= 2;
        }

        // Set StartIndices
        ComputeHelper.DispatchKernel(ssShader, "PopulateStartIndices", OC_len, ssShaderThreadSize);
    }

    void RunPCShader()
    {
        ComputeHelper.DispatchKernel(pcShader, "CalcTriNormals", NumTris, pcShaderThreadSize);
        ComputeHelper.DispatchKernel(pcShader, "SetLastRotations", NumTriObjects, pcShaderThreadSize);
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Main program loop
        if (SettingsChanged) { RunPCShader(); SettingsChanged = false; } // PreCalc
        RunSSShader(); // SpatialSort
        RunRMShader(); // RayMarcher

        Graphics.Blit(renderTexture, dest);
    }

    void OnDestroy()
    {
        ComputeHelper.Release(B_TriObjects, B_Tris, B_Spheres, B_Materials, B_SpatialLookup, B_StartIndices, AC_OccupiedChunks, CB_A);
    }
}