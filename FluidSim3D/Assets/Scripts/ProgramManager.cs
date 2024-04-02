using UnityEngine;
using Unity.Mathematics;

// Import utils from Resources.cs
using Resources;

public class ProgramManager : MonoBehaviour
{
    public float POffsetZ;
    public float PRadius;
    public int PMaterialKey;
    public new Renderer renderer;
    public Simulation sim;
    public TextureCreator texture;
    public ComputeShader dtShader;
    private const int dtShaderThreadSize = 512; // /1024
    private bool ProgramStarted = false;

    void Start()
    {
        
    }

    void Update()
    {
        sim.RunTimeSteps();

        if (!ProgramStarted)
        {
            dtShader.SetBuffer(0, "PData", sim.PDataBuffer);
            dtShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);
            dtShader.SetBuffer(0, "Spheres", renderer.B_Spheres);

            dtShader.SetFloat("ParticlesNum", sim.ParticlesNum);
            dtShader.SetFloat("OffsetZ", POffsetZ);
            dtShader.SetFloat("Radius", PRadius);
            dtShader.SetFloat("MaterialKey", PMaterialKey);
            dtShader.SetFloat("ChunksNumAll", sim.ChunksNumAll);
            dtShader.SetFloat("PTypesNum", sim.PTypes.Length);

            ProgramStarted = !ProgramStarted;
        }

        ComputeHelper.DispatchKernel(dtShader, "TransferParticlePositionData", sim.ParticlesNum, dtShaderThreadSize);
        
        renderer.UpdateRendererData();
    }
}