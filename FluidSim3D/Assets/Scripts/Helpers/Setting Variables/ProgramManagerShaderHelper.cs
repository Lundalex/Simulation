using Unity.Mathematics;
using UnityEngine;

// Import utils from Resources.cs
using Resources;
public class ProgramManagerShaderHelper : MonoBehaviour
{
    public ProgramManager manager;

// --- SHADER BUFFERS ---

    public void SetSSShaderBuffers (ComputeShader ssShader)
    {
        ssShader.SetBuffer(0, "Points", manager.B_Points);
        ssShader.SetBuffer(0, "SpatialLookup", manager.B_SpatialLookup);

        ssShader.SetBuffer(1, "SpatialLookup", manager.B_SpatialLookup);

        ssShader.SetBuffer(2, "SpatialLookup", manager.B_SpatialLookup);
        ssShader.SetBuffer(2, "StartIndices", manager.B_StartIndices);
    }

// --- SHADER SETTINGS / VARIABLES ---

    public void SetSSSettings (ComputeShader ssShader)
    {
        // Num constants
        ssShader.SetVector("NumChunks", new Vector4(manager.NumChunks.x, manager.NumChunks.y, manager.NumChunks.z, manager.NumChunks.w));
        ssShader.SetInt("NumChunksAll", manager.NumChunksAll);
        ssShader.SetInt("NumPoints", manager.NumPoints);
        ssShader.SetInt("NumPoints_NextPow2", Func.NextPow2(manager.NumPoints_NextPow2));

        // World settings
        ssShader.SetFloat("CellSize", manager.CellSizeSL);
    }

    public void UpdateSortIterationVariables (ComputeShader ssShader, int blockLen, bool brownPinkSort)
    {
        ssShader.SetBool("BrownPinkSort", brownPinkSort);
        ssShader.SetInt("BlockLen", blockLen);
    }
}