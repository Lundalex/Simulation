using Unity.Mathematics;
using UnityEngine;

// Import utils from Resources.cs
using Resources;
public class RendererShaderHelper : MonoBehaviour
{
    public Renderer render;
    public TextureCreator texture;
    public TextureHelper th;
    public ProgramManager manager;

// --- SHADER BUFFERS ---

    public void SetRMShaderBuffers (ComputeShader rmShader)
    {
        rmShader.SetBuffer(0, "TriObjects", render.B_TriObjects);
        rmShader.SetBuffer(0, "Tris", render.B_Tris);
        rmShader.SetBuffer(0, "Spheres", render.B_Spheres);
        rmShader.SetBuffer(0, "Materials", render.B_Materials);
        rmShader.SetBuffer(0, "SpatialLookup", render.B_SpatialLookup);
        rmShader.SetBuffer(0, "StartIndices", render.B_StartIndices);
        rmShader.SetBuffer(0, "SafeDistances", render.B_SafeDistances);

        rmShader.SetTexture(0, "Result", render.T_Result);
    }

    public void SetPPShaderBuffers (ComputeShader ppShader)
    {
        // Noise textures are set by TextureCreator

        ppShader.SetTexture(0, "Result", render.T_Result);
        ppShader.SetTexture(0, "AccResult", render.renderTexture);

        ppShader.SetTexture(1, "AccResult", render.renderTexture);
    }

    public void SetPCShaderBuffers (ComputeShader pcShader)
    {
        pcShader.SetBuffer(0, "TriObjects", render.B_TriObjects);
        pcShader.SetBuffer(0, "Tris", render.B_Tris);

        pcShader.SetBuffer(1, "TriObjects", render.B_TriObjects);

        pcShader.SetBuffer(2, "StartIndices", render.B_StartIndices);
        pcShader.SetBuffer(2, "SafeDistances", render.B_SafeDistances);
    }

    public void SetSSShaderBuffers (ComputeShader ssShader)
    {
        ssShader.SetBuffer(0, "Spheres", render.B_Spheres);
        ssShader.SetBuffer(0, "OccupiedChunksAPPEND", render.AC_OccupiedChunks);

        ssShader.SetBuffer(1, "TriObjects", render.B_TriObjects);
        ssShader.SetBuffer(1, "Tris", render.B_Tris);
        ssShader.SetBuffer(1, "OccupiedChunksAPPEND", render.AC_OccupiedChunks);

        ssShader.SetBuffer(2, "StartIndices", render.B_StartIndices);

        ssShader.SetBuffer(3, "OccupiedChunksCONSUME", render.AC_OccupiedChunks);
        ssShader.SetBuffer(3, "SpatialLookup", render.B_SpatialLookup);

        ssShader.SetBuffer(4, "SpatialLookup", render.B_SpatialLookup);

        ssShader.SetBuffer(5, "SpatialLookup", render.B_SpatialLookup);
        ssShader.SetBuffer(5, "StartIndices", render.B_StartIndices);
    }

    public void SetNGShaderTextures (ComputeShader ngShader)
    {
        ngShader.SetTexture(0, "VectorMap", th.T_VectorMap);

        ngShader.SetTexture(1, "VectorMap", th.T_VectorMap);

        ngShader.SetTexture(2, "PointsMap", th.T_PointsMap);

        ngShader.SetTexture(3, "PointsMap", th.T_PointsMap);
    }

    public void SetMSShaderBuffers (ComputeShader msShader)
    {
        msShader.SetBuffer(0, "SpatialLookup", manager.B_SpatialLookup);
        msShader.SetBuffer(0, "StartIndices", manager.B_StartIndices);
        msShader.SetBuffer(0, "Points", manager.B_Points);
        msShader.SetTexture(0, "GridDensities", render.T_GridDensities);

        msShader.SetBuffer(1, "SurfaceCellsAPPEND", render.AC_SurfaceCells);
        msShader.SetTexture(1, "GridDensities", render.T_GridDensities);
        msShader.SetTexture(1, "SurfaceCells", render.T_SurfaceCells);

        msShader.SetTexture(2, "GridDensities", render.T_GridDensities);
        msShader.SetBuffer(2, "SurfaceCellsCONSUME", render.AC_SurfaceCells);
        msShader.SetBuffer(2, "FluidTriMeshAPPEND", render.AC_FluidTriMesh);

        msShader.SetBuffer(3, "Tris", render.B_Tris);

        msShader.SetBuffer(4, "FluidTriMeshCONSUME", render.AC_FluidTriMesh);
        msShader.SetBuffer(4, "Tris", render.B_Tris);
    }

// --- SHADER SETTINGS / VARIABLES ---

    public void SetRMSettings (ComputeShader rmShader)
    {
        SetRMShaderBuffers(rmShader);

        rmShader.SetFloat("MaxStepSize", Mathf.Max(0.05f, render.MaxStepSize)); // MaxStepSize == 0 will cause a crash

        rmShader.SetVector("NumChunks", new Vector4(render.NumChunks.x, render.NumChunks.y, render.NumChunks.z, render.NumChunks.w));
        rmShader.SetInt("NumTriObjects", render.TriObjects.Length);
        rmShader.SetInt("NumTris", render.Tris.Length);
        rmShader.SetInt("NumObjects", render.NumObjects);
        rmShader.SetInt("NumSpheres", render.Spheres.Length);
        rmShader.SetInt("NumMaterials", render.Materials.Length);

        rmShader.SetVector("Resolution", new Vector2(render.Resolution.x, render.Resolution.y));
        rmShader.SetVector("MinWorldBounds", new Vector3(render.MinWorldBounds.x, render.MinWorldBounds.y, render.MinWorldBounds.z));
        rmShader.SetVector("MaxWorldBounds", new Vector3(render.MaxWorldBounds.x, render.MaxWorldBounds.y, render.MaxWorldBounds.z));
        rmShader.SetVector("ChunkGridOffset", new Vector3(render.ChunkGridOffset.x, render.ChunkGridOffset.y, render.ChunkGridOffset.z));
        rmShader.SetFloat("CellSize", render.CellSize);

        // Ray setup settings
        rmShader.SetInt("MaxStepCount", render.MaxStepCount);
        rmShader.SetInt("RaysPerPixel", render.RaysPerPixel);
        rmShader.SetFloat("HitThreshold", render.HitThreshold);
        rmShader.SetFloat("ScatterProbability", render.ScatterProbability);
        rmShader.SetFloat("DefocusStrength", render.DefocusStrength);

        // Screen settings
        float aspectRatio = render.Resolution.x / render.Resolution.y;
        float fieldOfViewRad = render.fieldOfView * Mathf.Deg2Rad;
        float viewSpaceHeight = Mathf.Tan(fieldOfViewRad * 0.5f);
        float viewSpaceWidth = aspectRatio * viewSpaceHeight;
        rmShader.SetFloat("viewSpaceWidth", viewSpaceWidth);
        rmShader.SetFloat("viewSpaceHeight", viewSpaceHeight);

        rmShader.SetFloat("focalPlaneFactor", render.focalPlaneFactor);
    }

    public void SetPPSettings (ComputeShader ppShader)
    {
        SetPPShaderBuffers(ppShader);

        // Noise settings
        ppShader.SetVector("NoiseResolution", new Vector3(texture.NoiseResolution.x, texture.NoiseResolution.y, texture.NoiseResolution.z));
        ppShader.SetFloat("NoisePixelSize", texture.NoisePixelSize);
    }

    public void SetPCSettings (ComputeShader pcShader)
    {
        pcShader.SetInt("NumTris", render.NumTris);
        pcShader.SetInt("NumTriObjects", render.NumTriObjects);
        pcShader.SetVector("NumChunks", new Vector4(render.NumChunks.x, render.NumChunks.y, render.NumChunks.z, render.NumChunks.w));
    }

    public void SetSSSettings (ComputeShader ssShader)
    {
        SetSSShaderBuffers(ssShader);

        // Num constants
        ssShader.SetVector("NumChunks", new Vector4(render.NumChunks.x, render.NumChunks.y, render.NumChunks.z, render.NumChunks.w));
        ssShader.SetInt("NumChunksAll", render.NumChunksAll);
        ssShader.SetInt("NumSpheres", render.NumSpheres);
        ssShader.SetInt("NumObjects", render.NumObjects);
        ssShader.SetInt("NumObjects_NextPow2", Func.NextPow2(render.NumObjects));

        // World settings
        ssShader.SetVector("MinWorldBounds", new Vector3(render.MinWorldBounds.x, render.MinWorldBounds.y, render.MinWorldBounds.z));
        ssShader.SetVector("MaxWorldBounds", new Vector3(render.MaxWorldBounds.x, render.MaxWorldBounds.y, render.MaxWorldBounds.z));
        ssShader.SetVector("ChunkGridOffset", new Vector3(render.ChunkGridOffset.x, render.ChunkGridOffset.y, render.ChunkGridOffset.z));
        ssShader.SetFloat("CellSize", render.CellSize);
    }

    public void SetNGSettings (ComputeShader ngShader)
    {
        ngShader.SetVector("NoiseResolution", new Vector3(texture.NoiseResolution.x, texture.NoiseResolution.y, texture.NoiseResolution.z));
    }

    public void SetMSShaderSettings (ComputeShader msShader)
    {
        msShader.SetFloat("CellSizeMS", render.CellSizeMS);
        msShader.SetFloat("Threshold", render.ThresholdMS);

        msShader.SetVector("NumChunks", new Vector4(manager.NumChunks.x, manager.NumChunks.y, manager.NumChunks.z, manager.NumChunks.w));
        msShader.SetFloat("CellSizeSL", manager.CellSizeSL);
        msShader.SetVector("ChunkGridOffset", new Vector3(render.ChunkGridOffset.x, render.ChunkGridOffset.y, render.ChunkGridOffset.z));
    }

    public void UpdatePPVariables (ComputeShader ppShader)
    {
        // Frame set variables
        ppShader.SetInt("FrameCount", render.FrameCount);
    }

    public void UpdateRMVariables (ComputeShader rmShader)
    {
        // Frame set variables
        rmShader.SetInt("FrameRand", Func.RandInt(0, 999999));
        rmShader.SetInt("FrameCount", render.FrameCount);

        // Camera position
        float3 worldSpaceCameraPos = transform.position;
        rmShader.SetVector("WorldSpaceCameraPos", new Vector3(worldSpaceCameraPos.x, worldSpaceCameraPos.y, worldSpaceCameraPos.z));

        // Camera orientation
        float3 cameraRot = transform.rotation.eulerAngles * Mathf.Deg2Rad;
        rmShader.SetVector("CameraRotation", new Vector3(cameraRot.x, cameraRot.y, cameraRot.z));
        // Changes made for better intuitivity
        float temp = cameraRot.x;
        cameraRot.x = cameraRot.y;
        cameraRot.y = -temp;
        cameraRot.z = -cameraRot.z;

        // Camera transform matrix
        float cosX = Mathf.Cos(cameraRot.x);
        float sinX = Mathf.Sin(cameraRot.x);
        float cosY = Mathf.Cos(cameraRot.y);
        float sinY = Mathf.Sin(cameraRot.y);
        float cosZ = Mathf.Cos(cameraRot.z);
        float sinZ = Mathf.Sin(cameraRot.z);
        // Combined camera transform
        // Unity only allows setting 4x4 matrices (will get converted to 3x3 automatically in shader)
        float4x4 CameraTransform = new float4x4(
            cosY * cosZ,                             cosY * sinZ,                           -sinY, 0.0f,
            sinX * sinY * cosZ - cosX * sinZ,   sinX * sinY * sinZ + cosX * cosZ,  sinX * cosY, 0.0f,
            cosX * sinY * cosZ + sinX * sinZ,   cosX * sinY * sinZ - sinX * cosZ,  cosX * cosY, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f
        );

        rmShader.SetMatrix("CameraTransform", CameraTransform);
    }

    public void UpdateNGVariables (ComputeShader ngShader)
    {
        // Frame set variables
        ngShader.SetInt("FrameRand", UnityEngine.Random.Range(0, 999999));
        ngShader.SetInt("FrameCount", render.FrameCount);
    }

    public void UpdateTriSettings (ComputeShader msShader, ComputeShader pcShader, ComputeShader rmShader, ComputeShader ssShader)
    {
        msShader.SetInt("DynamicNumTris", render.DynamicNumTris);
        msShader.SetInt("ReservedNumTris", render.ReservedNumTris);

        rmShader.SetInt("NumTriObjects", render.TriObjects.Length);
        rmShader.SetInt("NumTris", render.NumTris);
        rmShader.SetInt("NumObjects", render.NumObjects);

        pcShader.SetInt("NumTris", render.NumTris);

        ssShader.SetInt("NumObjects", render.NumObjects);
        ssShader.SetInt("NumObjects_NextPow2", Func.NextPow2(render.NumObjects));

        ssShader.SetBuffer(1, "Tris", render.B_Tris);
        pcShader.SetBuffer(0, "Tris", render.B_Tris);
        rmShader.SetBuffer(0, "Tris", render.B_Tris);
        msShader.SetBuffer(4, "Tris", render.B_Tris);
        msShader.SetBuffer(3, "Tris", render.B_Tris);
    }

    public void UpdateSphereSettings (ComputeShader dtShader, ComputeShader pcShader, ComputeShader rmShader, ComputeShader ssShader)
    {
        dtShader.SetInt("ReservedNumSpheres", render.ReservedNumSpheres);
        dtShader.SetInt("NumSpheres", render.NumSpheres);

        rmShader.SetInt("NumSpheres", render.NumSpheres);
        rmShader.SetInt("NumObjects", render.NumObjects);

        ssShader.SetInt("NumSpheres", render.NumSpheres);
        ssShader.SetInt("NumObjects", render.NumObjects);
        ssShader.SetInt("NumObjects_NextPow2", Func.NextPow2(render.NumObjects));

        pcShader.SetInt("NumSpheres", render.NumSpheres);

        rmShader.SetBuffer(0, "Spheres", render.B_Spheres);
        ssShader.SetBuffer(0, "Spheres", render.B_Spheres);
        dtShader.SetBuffer(1, "Spheres", render.B_Spheres);
    }
}