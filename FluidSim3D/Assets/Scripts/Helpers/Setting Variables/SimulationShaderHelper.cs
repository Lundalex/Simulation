using UnityEngine;
public class SimulationShaderHelper : MonoBehaviour
{
    public Simulation sim;
    public void SetPSimShaderBuffers(ComputeShader pSimShader)
    {
        // Kernel PreCalculations
        pSimShader.SetBuffer(0, "PDataB", sim.PDataBuffer);
        pSimShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);
    
        // Kernel PreCalculations
        pSimShader.SetBuffer(1, "SpatialLookup", sim.SpatialLookupBuffer);
        pSimShader.SetBuffer(1, "StartIndices", sim.StartIndicesBuffer);

        pSimShader.SetBuffer(1, "PDataB", sim.PDataBuffer);
        pSimShader.SetBuffer(1, "PTypes", sim.PTypesBuffer);

        pSimShader.SetBuffer(2, "ParticleSpringsCombined", sim.ParticleSpringsCombinedBuffer);

        pSimShader.SetBuffer(3, "PDataB", sim.PDataBuffer);
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

        pSimShader.SetBuffer(4, "PDataB", sim.PDataBuffer);
        pSimShader.SetBuffer(4, "PTypes", sim.PTypesBuffer);

        pSimShader.SetBuffer(4, "SpringCapacities", sim.SpringCapacitiesBuffer);
        pSimShader.SetBuffer(4, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        pSimShader.SetBuffer(4, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        pSimShader.SetBuffer(4, "ParticleSpringsCombined", sim.ParticleSpringsCombinedBuffer);

        pSimShader.SetBuffer(5, "PDataB", sim.PDataBuffer);
        pSimShader.SetBuffer(5, "PTypes", sim.PTypesBuffer);
        pSimShader.SetBuffer(5, "SpringCapacities", sim.SpringCapacitiesBuffer);
    }

    public void SetSortShaderBuffers(ComputeShader sortShader)
    {
        sortShader.SetBuffer(0, "SpatialLookup", sim.SpatialLookupBuffer);

        sortShader.SetBuffer(0, "PDataB", sim.PDataBuffer);
        sortShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);

        sortShader.SetBuffer(1, "SpatialLookup", sim.SpatialLookupBuffer);

        sortShader.SetBuffer(1, "PDataB", sim.PDataBuffer);
        sortShader.SetBuffer(1, "PTypes", sim.PTypesBuffer);

        sortShader.SetBuffer(2, "StartIndices", sim.StartIndicesBuffer);

        sortShader.SetBuffer(3, "SpatialLookup", sim.SpatialLookupBuffer);
        sortShader.SetBuffer(3, "StartIndices", sim.StartIndicesBuffer);
        sortShader.SetBuffer(3, "PTypes", sim.PTypesBuffer);
        sortShader.SetBuffer(3, "PDataB", sim.PDataBuffer);

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
    }

    public void UpdatePSimShaderVariables(ComputeShader pSimShader)
    {
        pSimShader.SetInt("MaxInfluenceRadiusSqr", sim.MaxInfluenceRadiusSqr);
        pSimShader.SetFloat("InvMaxInfluenceRadius", sim.InvMaxInfluenceRadius);
        pSimShader.SetVector("ChunksNum", new Vector4(sim.ChunksNum.x, sim.ChunksNum.y, sim.ChunksNum.z, sim.ChunksNum.w));
        pSimShader.SetInt("Width", sim.Width);
        pSimShader.SetInt("Height", sim.Height);
        pSimShader.SetInt("Depth", sim.Depth);
        pSimShader.SetInt("ParticlesNum", sim.ParticlesNum);
        pSimShader.SetInt("ParticleSpringsCombinedHalfLength", sim.ParticleSpringsCombinedHalfLength);
        pSimShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        pSimShader.SetFloat("LookAheadFactor", sim.LookAheadFactor);
        pSimShader.SetFloat("StateThresholdPadding", sim.StateThresholdPadding);
        pSimShader.SetFloat("BorderPadding", sim.BorderPadding);
        pSimShader.SetFloat("MaxInteractionRadius", sim.MaxInteractionRadius);
        pSimShader.SetFloat("InteractionAttractionPower", sim.InteractionAttractionPower);
        pSimShader.SetFloat("InteractionFountainPower", sim.InteractionFountainPower);
        pSimShader.SetFloat("InteractionTemperaturePower", sim.InteractionTemperaturePower);
    }

    public void UpdateSortShaderVariables(ComputeShader sortShader)
    {
        sortShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        sortShader.SetVector("ChunksNum", new Vector4(sim.ChunksNum.x, sim.ChunksNum.y, sim.ChunksNum.z, sim.ChunksNum.w));
        sortShader.SetInt("ChunksNumAll", sim.ChunksNumAll);
        sortShader.SetInt("ChunkNumNextPow2", sim.ChunksNumAllNextPow2);
        sortShader.SetInt("ParticlesNum", sim.ParticlesNum);
        sortShader.SetInt("ParticlesNum_NextPow2", sim.ParticlesNum_NextPow2);
    }
}