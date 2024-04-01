using UnityEngine;
using Unity.Mathematics;

// Import utils from Resources.cs
using Resources;

public class TextureCreator : MonoBehaviour
{
    [Header("Noise settings")]
    public int3 NoiseResolution = new(512, 512, 256);
    public int NoiseCellSize = 128;
    public float LerpFactor = 0.15f; // TEMP
    public int inputB = 3; // TEMP
    public float NoisePixelSize = 0.7f;
    public bool RenderNoiseTextures = true;
    private bool LateStart = true;

    [Header("References")]
    public ComputeShader ngShader;
    public ComputeShader rmShader;
    public TextureHelper textureHelper;

    void Start ()
    {
        textureHelper.UpdateScriptTextures(NoiseResolution, 1);
    }

    void Update()
    {
        if (LateStart) { InitNoiseTextures(); LateStart = !LateStart;}
    }

    // Creates a Cloud-like 3D texture
    void InitNoiseTextures()
    {
        // -- CLOUD TEXTURE --

        // Perlin noise
        RenderTexture perlin = TextureHelper.CreateTexture(NoiseResolution, 1);
        textureHelper.SetPerlin(ref perlin, NoiseResolution, NoiseCellSize, Func.RandInt(0, 999999));

        // Init voronoi textures
        RenderTexture voronoi0 = TextureHelper.CreateTexture(NoiseResolution, 1);
        RenderTexture voronoi1 = TextureHelper.CreateTexture(NoiseResolution, 1);
        RenderTexture voronoi2 = TextureHelper.CreateTexture(NoiseResolution, 1);
        RenderTexture voronoi3 = TextureHelper.CreateTexture(NoiseResolution, 1);

        // Set voronoi textures
        textureHelper.SetVoronoi(ref voronoi0, NoiseResolution, NoiseCellSize, Func.RandInt(0, 999999));
        textureHelper.SetVoronoi(ref voronoi1, NoiseResolution, NoiseCellSize / 2, Func.RandInt(0, 999999));
        textureHelper.SetVoronoi(ref voronoi2, NoiseResolution, NoiseCellSize / 4, Func.RandInt(0, 999999));
        textureHelper.SetVoronoi(ref voronoi3, NoiseResolution, NoiseCellSize / 8, Func.RandInt(0, 999999));

        // Invert voronoi noises
        textureHelper.Invert(ref voronoi0, NoiseResolution);
        textureHelper.Invert(ref voronoi1, NoiseResolution);
        textureHelper.Invert(ref voronoi2, NoiseResolution);
        textureHelper.Invert(ref voronoi3, NoiseResolution);

        // Lower brightness values
        textureHelper.ChangeBrightness(ref voronoi0, NoiseResolution, 0.25f);
        textureHelper.ChangeBrightness(ref voronoi1, NoiseResolution, 0.25f);
        textureHelper.ChangeBrightness(ref voronoi2, NoiseResolution, 0.25f);
        textureHelper.ChangeBrightness(ref voronoi3, NoiseResolution, 0.25f);

        // Sum up textures into voronoi0
        textureHelper.AddBrightnessByTexture(ref voronoi0, voronoi1, NoiseResolution);
        textureHelper.AddBrightnessByTexture(ref voronoi2, voronoi3, NoiseResolution);
        textureHelper.AddBrightnessByTexture(ref voronoi0, voronoi2, NoiseResolution);

        // Blend voronoi and perlin
        textureHelper.Blend(ref voronoi0, perlin, NoiseResolution, LerpFactor);

        // Extra effects
        textureHelper.AddBrightnessFixed(ref voronoi0, NoiseResolution, -0.2f);
        textureHelper.ChangeBrightness(ref voronoi0, NoiseResolution, 1.25f);
        textureHelper.GaussianBlur(ref voronoi0, NoiseResolution, 3, 5);
        
        rmShader.SetTexture(1, "NoiseA", voronoi0);
        rmShader.SetTexture(1, "NoiseB", TextureHelper.CreateTexture(NoiseResolution, 1)); // Nothing to display for NoiseB
    }
}