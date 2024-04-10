using UnityEngine;
public class SimulationShaderHelper : MonoBehaviour
{
    public Simulation sim;
    public void SetPSimShaderBuffers (ComputeShader pSimShader)
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

    public void SetSSShaderBuffers (ComputeShader ssShader)
    {
        ssShader.SetBuffer(0, "SpatialLookup", sim.SpatialLookupBuffer);

        ssShader.SetBuffer(0, "PDataB", sim.PDataBuffer);
        ssShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);

        ssShader.SetBuffer(1, "SpatialLookup", sim.SpatialLookupBuffer);

        ssShader.SetBuffer(1, "PDataB", sim.PDataBuffer);
        ssShader.SetBuffer(1, "PTypes", sim.PTypesBuffer);

        ssShader.SetBuffer(2, "StartIndices", sim.StartIndicesBuffer);

        ssShader.SetBuffer(3, "SpatialLookup", sim.SpatialLookupBuffer);
        ssShader.SetBuffer(3, "StartIndices", sim.StartIndicesBuffer);
        ssShader.SetBuffer(3, "PTypes", sim.PTypesBuffer);
        ssShader.SetBuffer(3, "PDataB", sim.PDataBuffer);
    }

    public void SetIPSShaderBuffer (ComputeShader ipsShader)
    {
        ipsShader.SetBuffer(0, "SpatialLookup", sim.SpatialLookupBuffer);
        ipsShader.SetBuffer(0, "StartIndices", sim.StartIndicesBuffer);
        ipsShader.SetBuffer(0, "SpringCapacities", sim.SpringCapacitiesBuffer);

        ipsShader.SetBuffer(1, "SpringCapacities", sim.SpringCapacitiesBuffer);

        ipsShader.SetBuffer(2, "SpringCapacities", sim.SpringCapacitiesBuffer);
        ipsShader.SetBuffer(2, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        ipsShader.SetBuffer(2, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        ipsShader.SetBuffer(2, "SpringStartIndices_dbC", sim.SpringStartIndicesBuffer_dbC);

        ipsShader.SetBuffer(3, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        ipsShader.SetBuffer(3, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        ipsShader.SetBuffer(3, "SpringStartIndices_dbC", sim.SpringStartIndicesBuffer_dbC);

        ipsShader.SetBuffer(4, "SpringStartIndices_dbA", sim.SpringStartIndicesBuffer_dbA);
        ipsShader.SetBuffer(4, "SpringStartIndices_dbB", sim.SpringStartIndicesBuffer_dbB);
        ipsShader.SetBuffer(4, "SpringStartIndices_dbC", sim.SpringStartIndicesBuffer_dbC);
    }

    public void UpdatePSimShaderVariables (ComputeShader pSimShader)
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

    public void UpdateSSShaderVariables (ComputeShader ssShader)
    {
        ssShader.SetInt("MaxInfluenceRadius", sim.MaxInfluenceRadius);
        ssShader.SetVector("ChunksNum", new Vector4(sim.ChunksNum.x, sim.ChunksNum.y, sim.ChunksNum.z, sim.ChunksNum.w));
        ssShader.SetInt("ChunksNumAll", sim.ChunksNumAll);
        ssShader.SetInt("ChunkNumNextPow2", sim.ChunksNumAllNextPow2);
        ssShader.SetInt("ParticlesNum", sim.ParticlesNum);
    }

    public void UpdateIPSShaderVariables (ComputeShader ipsShader)
    {
        ipsShader.SetVector("ChunksNum", new Vector4(sim.ChunksNum.x, sim.ChunksNum.y, sim.ChunksNum.z, sim.ChunksNum.w));
        ipsShader.SetInt("ChunksNumAll", sim.ChunksNumAll);
        ipsShader.SetInt("ParticlesNum", sim.ParticlesNum);
    }
}