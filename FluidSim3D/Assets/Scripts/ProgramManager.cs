using UnityEngine;
using Unity.Mathematics;

// Import utils from Resources.cs
using Resources;

public class ProgramManager : MonoBehaviour
{
    public int TimeStepsPerRenderFrame;
    public float RotationSpeed;
    public float PRadius;
    public float3 Rot;
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
        sim.RunTimeSteps(TimeStepsPerRenderFrame);

        if (!ProgramStarted)
        {
            dtShader.SetBuffer(0, "PDataB", sim.PDataBuffer);
            dtShader.SetBuffer(0, "PTypes", sim.PTypesBuffer);
            dtShader.SetBuffer(0, "Spheres", renderer.B_Spheres);

            ProgramStarted = !ProgramStarted;
        }

        dtShader.SetFloat("ParticlesNum", sim.ParticlesNum);
        dtShader.SetFloat("Radius", PRadius);
        dtShader.SetFloat("ChunksNumAll", sim.ChunksNumAll);
        dtShader.SetFloat("PTypesNum", sim.PTypes.Length);
        Rot.y += RotationSpeed * Time.deltaTime;
        dtShader.SetVector("Rot", new Vector3(Rot.x, Rot.y, Rot.z));

        ComputeHelper.DispatchKernel(dtShader, "TransferParticlePositionData", sim.ParticlesNum, dtShaderThreadSize);
        
        renderer.UpdateRendererData();
    }
}