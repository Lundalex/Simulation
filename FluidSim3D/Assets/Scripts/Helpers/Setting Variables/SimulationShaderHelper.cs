using UnityEngine;
public class SimulationShaderHelper : MonoBehaviour
{
    public Simulation sim;
    public void SetPSimShaderBuffers(ComputeShader pSimShader)
    {
        // Kernel PreCalculations
        pSimShader.SetBuffer(0, "PData", sim.PDataBuffer);
        pSimShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);
    
        // Kernel PreCalculations
        pSimShader.SetBuffer(1, "SpatialLookup", sim.SpatialLookupBuffer);
        pSimShader.SetBuffer(1, "StartIndices", sim.StartIndicesBuffer);

        pSimShader.SetBuffer(1, "PData", sim.PDataBuffer);
        pSimShader.SetBuffer(1, "PTypes", sim.PTypesBuffer);

        pSimShader.SetBuffer(2, "ParticleSpringsCombined", sim.ParticleSpringsCombinedBuffer);

        pSimShader.SetBuffer(3, "PData", sim.PDataBuffer);
        pSimShader.SetBuffer(3, "PTypes", sim.PTypesBuffer);
        pSimShader.SetBuffer(3, "SpatialLookup", sim.SpatialLookupBuffer);
        pSimShader.SetBuffer(3, "StartIndices", sim.StartIndicesBuffer);
        pSimShader.SetBuffer(3, "SpringCapacities", sim.SpringCapacitiesBuffer);
        pSimShader.SetBuffer(3, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        pSimShader.SetBuffer(3, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        pSimShader.SetBuffer(3, "ParticleSpringsCombined", sim.ParticleSpringsCombinedBuffer);
        
        // Kernel ParticleForces - 8/8 buffers
        pSimShader.SetBuffer(4, "SpatialLookup", sim.SpatialLookupBuffer);
        pSimShader.SetBuffer(4, "StartIndices", sim.StartIndicesBuffer);

        pSimShader.SetBuffer(4, "PData", sim.PDataBuffer);
        pSimShader.SetBuffer(4, "PTypes", sim.PTypesBuffer);

        pSimShader.SetBuffer(4, "SpringCapacities", sim.SpringCapacitiesBuffer);
        pSimShader.SetBuffer(4, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        pSimShader.SetBuffer(4, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        pSimShader.SetBuffer(4, "ParticleSpringsCombined", sim.ParticleSpringsCombinedBuffer);

        pSimShader.SetBuffer(5, "PData", sim.PDataBuffer);
        pSimShader.SetBuffer(5, "PTypes", sim.PTypesBuffer);
        pSimShader.SetBuffer(5, "SpringCapacities", sim.SpringCapacitiesBuffer);

        pSimShader.SetBuffer(6, "PData", sim.PDataBuffer);
        pSimShader.SetBuffer(6, "PTypes", sim.PTypesBuffer);
        pSimShader.SetBuffer(6, "SortedStickyRequests", sim.SortedStickyRequestsBuffer);
    }

    public void SetRbSimShaderBuffers(ComputeShader rbSimShader)
    {
        rbSimShader.SetBuffer(0, "RBVector", sim.RBVectorBuffer);
        rbSimShader.SetBuffer(0, "RBData", sim.RBDataBuffer);

        rbSimShader.SetBuffer(1, "RBVector", sim.RBVectorBuffer);
        rbSimShader.SetBuffer(1, "RBData", sim.RBDataBuffer);
        rbSimShader.SetBuffer(1, "TraversedChunksAPPEND", sim.TraversedChunks_AC_Buffer);

        // Maximum reached! (8)
        rbSimShader.SetBuffer(2, "PData", sim.PDataBuffer);
        rbSimShader.SetBuffer(2, "PTypes", sim.PTypesBuffer);
        rbSimShader.SetBuffer(2, "RBData", sim.RBDataBuffer);
        rbSimShader.SetBuffer(2, "RBVector", sim.RBVectorBuffer);
        rbSimShader.SetBuffer(2, "SpatialLookup", sim.SpatialLookupBuffer);
        rbSimShader.SetBuffer(2, "StartIndices", sim.StartIndicesBuffer);
        rbSimShader.SetBuffer(2, "TraversedChunksCONSUME", sim.TraversedChunks_AC_Buffer);
        rbSimShader.SetBuffer(2, "StickynessReqsAPPEND", sim.StickynessReqs_AC_Buffer);

        rbSimShader.SetBuffer(3, "RBData", sim.RBDataBuffer);
        rbSimShader.SetBuffer(3, "RBVector", sim.RBVectorBuffer);
    }

    public void SetRenderShaderBuffers(ComputeShader renderShader)
    {
        renderShader.SetBuffer(0, "SpatialLookup", sim.SpatialLookupBuffer);
        renderShader.SetBuffer(0, "StartIndices", sim.StartIndicesBuffer);

        renderShader.SetBuffer(0, "PData", sim.PDataBuffer);
        renderShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);

        renderShader.SetBuffer(0, "RBData", sim.RBDataBuffer);
        renderShader.SetBuffer(0, "RBVector", sim.RBVectorBuffer);
    }

    public void SetSortShaderBuffers(ComputeShader sortShader)
    {
        sortShader.SetBuffer(0, "SpatialLookup", sim.SpatialLookupBuffer);

        sortShader.SetBuffer(0, "PData", sim.PDataBuffer);
        sortShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);

        sortShader.SetBuffer(1, "SpatialLookup", sim.SpatialLookupBuffer);

        sortShader.SetBuffer(1, "PData", sim.PDataBuffer);
        sortShader.SetBuffer(1, "PTypes", sim.PTypesBuffer);

        sortShader.SetBuffer(2, "StartIndices", sim.StartIndicesBuffer);

        sortShader.SetBuffer(3, "SpatialLookup", sim.SpatialLookupBuffer);
        sortShader.SetBuffer(3, "StartIndices", sim.StartIndicesBuffer);
        sortShader.SetBuffer(3, "PTypes", sim.PTypesBuffer);
        sortShader.SetBuffer(3, "PData", sim.PDataBuffer);

        sortShader.SetBuffer(4, "SpatialLookup", sim.SpatialLookupBuffer);
        sortShader.SetBuffer(4, "StartIndices", sim.StartIndicesBuffer);
        sortShader.SetBuffer(4, "SpringCapacities", sim.SpringCapacitiesBuffer);

        sortShader.SetBuffer(5, "SpringCapacities", sim.SpringCapacitiesBuffer);

        sortShader.SetBuffer(6, "SpringCapacities", sim.SpringCapacitiesBuffer);
        sortShader.SetBuffer(6, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        sortShader.SetBuffer(6, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        sortShader.SetBuffer(6, "SpringStartIndices_dbC", sim.SpringStartIndicesBuffer_dbC);

        sortShader.SetBuffer(7, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        sortShader.SetBuffer(7, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        sortShader.SetBuffer(7, "SpringStartIndices_dbC", sim.SpringStartIndicesBuffer_dbC);

        sortShader.SetBuffer(8, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        sortShader.SetBuffer(8, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        sortShader.SetBuffer(8, "SpringStartIndices_dbC", sim.SpringStartIndicesBuffer_dbC);

        sortShader.SetBuffer(9, "StickynessReqsCONSUME", sim.StickynessReqs_AC_Buffer);
        sortShader.SetBuffer(9, "SortedStickyRequests", sim.SortedStickyRequestsBuffer);

        sortShader.SetBuffer(10, "SortedStickyRequests", sim.SortedStickyRequestsBuffer);
    }

    public void SetMarchingSquaresShaderBuffers(ComputeShader marchingSquaresShader)
    {
        marchingSquaresShader.SetBuffer(0, "MSPoints", sim.MSPointsBuffer);
        marchingSquaresShader.SetBuffer(0, "SpatialLookup", sim.SpatialLookupBuffer);
        marchingSquaresShader.SetBuffer(0, "StartIndices", sim.StartIndicesBuffer);

        marchingSquaresShader.SetBuffer(0, "PData", sim.PDataBuffer);
        marchingSquaresShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);
        
        marchingSquaresShader.SetBuffer(1, "Vertices", sim.VerticesBuffer);
        marchingSquaresShader.SetBuffer(1, "Triangles", sim.TrianglesBuffer);
        marchingSquaresShader.SetBuffer(1, "Colors", sim.ColorsBuffer);
        marchingSquaresShader.SetBuffer(1, "MSPoints", sim.MSPointsBuffer);
    }

    public void UpdatePSimShaderVariables(ComputeShader pSimShader)
    {
        pSimShader.SetInt("MaxInfluenceRadiusSqr", sim.MaxInfluenceRadiusSqr);
        pSimShader.SetFloat("InvMaxInfluenceRadius", sim.InvMaxInfluenceRadius);
        pSimShader.SetVector("ChunksNum", new Vector2(sim.ChunksNum.x, sim.ChunksNum.y));
        pSimShader.SetInt("Width", sim.Width);
        pSimShader.SetInt("Height", sim.Height);
        pSimShader.SetInt("ParticlesNum", sim.ParticlesNum);
        pSimShader.SetInt("ParticleSpringsCombinedHalfLength", sim.ParticleSpringsCombinedHalfLength);
        pSimShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        pSimShader.SetInt("SpawnDims", sim.SpawnDims);
        pSimShader.SetInt("TimeStepsPerRender", sim.TimeStepsPerRender);
        pSimShader.SetFloat("LookAheadFactor", sim.LookAheadFactor);
        pSimShader.SetFloat("StateThresholdPadding", sim.StateThresholdPadding);
        pSimShader.SetFloat("BorderPadding", sim.BorderPadding);
        pSimShader.SetFloat("MaxInteractionRadius", sim.MaxInteractionRadius);
        pSimShader.SetFloat("InteractionAttractionPower", sim.InteractionAttractionPower);
        pSimShader.SetFloat("InteractionFountainPower", sim.InteractionFountainPower);
        pSimShader.SetFloat("InteractionTemperaturePower", sim.InteractionTemperaturePower);
    }

    public void UpdateRbSimShaderVariables(ComputeShader rbSimShader)
    {
        rbSimShader.SetVector("ChunksNum", new Vector2(sim.ChunksNum.x, sim.ChunksNum.y));
        rbSimShader.SetInt("Width", sim.Width);
        rbSimShader.SetInt("Height", sim.Height);
        rbSimShader.SetInt("ParticlesNum", sim.ParticlesNum);
        rbSimShader.SetInt("RBodiesNum", sim.RBData.Length);
        rbSimShader.SetInt("RBVectorNum", sim.RBVector.Length);
        rbSimShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        rbSimShader.SetInt("MaxChunkSearchSafety", sim.MaxChunkSearchSafety);

        rbSimShader.SetFloat("Damping", sim.Damping);
        rbSimShader.SetFloat("Gravity", sim.Gravity);
        rbSimShader.SetFloat("RbElasticity", sim.RbElasticity);
        rbSimShader.SetFloat("BorderPadding", sim.BorderPadding);
    }

    public void UpdateRenderShaderVariables(ComputeShader renderShader)
    {
        renderShader.SetFloat("VisualParticleRadii", sim.VisualParticleRadii);
        renderShader.SetFloat("RBRenderThickness", sim.RBRenderThickness);
        renderShader.SetVector("Resolution", new Vector2(sim.Resolution.x, sim.Resolution.y));
        renderShader.SetInt("Width", sim.Width);
        renderShader.SetInt("Height", sim.Height);
        renderShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        renderShader.SetVector("ChunksNum", new Vector2(sim.ChunksNum.x, sim.ChunksNum.y));
        renderShader.SetInt("ParticlesNum", sim.ParticlesNum);
        renderShader.SetInt("RBodiesNum", sim.RBData.Length);
        renderShader.SetInt("RBVectorNum", sim.RBVector.Length);
        
    }

    public void UpdateSortShaderVariables(ComputeShader sortShader)
    {
        sortShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        sortShader.SetVector("ChunksNum", new Vector2(sim.ChunksNum.x, sim.ChunksNum.y));
        sortShader.SetInt("ChunksNumAll", sim.ChunksNumAll);
        sortShader.SetInt("ChunkNumNextPow2", sim.ChunksNumAllNextPow2);
        sortShader.SetInt("ParticlesNum", sim.ParticlesNum);
        sortShader.SetInt("ParticlesNum_NextPow2", sim.ParticlesNum_NextPow2);
    }

    public void UpdateMarchingSquaresShaderVariables(ComputeShader marchingSquaresShader)
    {   
        marchingSquaresShader.SetInt("MarchW", sim.MarchW);
        marchingSquaresShader.SetInt("MarchH", sim.MarchH);
        marchingSquaresShader.SetFloat("MSResolution", sim.MSResolution);
        marchingSquaresShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        marchingSquaresShader.SetVector("ChunksNum", new Vector2(sim.ChunksNum.x, sim.ChunksNum.y));
        marchingSquaresShader.SetInt("Width", sim.Width);
        marchingSquaresShader.SetInt("Height", sim.Height);
        marchingSquaresShader.SetInt("ParticlesNum", sim.ParticlesNum);
        marchingSquaresShader.SetFloat("MSvalMin", sim.MSvalMin);
        marchingSquaresShader.SetFloat("TriStorageLength", sim.TriStorageLength);
    }
}