using Unity.Mathematics;
using UnityEngine;
using System;
public class TextureHelper : MonoBehaviour
{
    public ComputeShader ngShader;
    public ComputeShader tcShader;
    private readonly int ngShaderThreadSize = 8; // /~10
    private readonly int tbShaderThreadSize = 8; // /~10
    [NonSerialized] public RenderTexture T_VectorMap;
    [NonSerialized] public RenderTexture T_PointsMap;
    private int3 LastResolution;
    private int LastCellSize;

#region Create Textures (2D + 3D)

    /// <summary>Creates a 3D render texture</summary>
    /// <remarks>Using (int3)resolution</remarks>
    /// <returns>Without ref</returns>
    public static RenderTexture CreateTexture(int3 resolution, int channels)
    {
        if (channels == 1)
        {
            RenderTexture texture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.R16) // 0.0-1.0 with linear accuracy, single channel
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = resolution.z,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();

            return texture;
        }

        else if (channels == 2)
        {
            RenderTexture texture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RGFloat)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = resolution.z,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();

            return texture;
        }

        else // channels == 3
        {
            RenderTexture texture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RGB111110Float)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = resolution.z,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();

            return texture;
        }
    }
    /// <summary>Creates a 3D render texture</summary>
    /// <remarks>Using (int3)resolution</remarks>
    /// <returns>-> ref texture</returns>
    public static void CreateTexture(ref RenderTexture texture, int3 resolution, int channels)
    {
        if (channels == 1)
        {
            texture = texture != null ? texture : new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.R16) // 0.0-1.0 with linear accuracy, single channel
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = resolution.z,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
        }

        else if (channels == 2)
        {
            texture = texture != null ? texture : new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RGFloat)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = resolution.z,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
        }

        else // channels == 3
        {
            texture = texture != null ? texture : new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RGB111110Float)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = resolution.z,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
        }
    }
    /// <summary>Creates a 2D render texture</summary>
    /// <remarks>Using (int2)resolution</remarks>
    /// <returns>Without ref</returns>
    public static RenderTexture CreateTexture(int2 resolution, int channels)
    {
        if (channels == 1)
        {
            RenderTexture texture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.R16)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
            return texture;
        }
        else if (channels == 2)
        {
            RenderTexture texture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RGFloat)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
            return texture;
        }
        else // channels == 3
        {
            RenderTexture texture = new RenderTexture(resolution.x, resolution.y, 24)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
            return texture;
        }
    }
    /// <summary>Creates a 2D render texture</summary>
    /// <remarks>Using (int3)resolution</remarks>
    /// <returns>-> ref texture</returns>
    public static void CreateTexture(ref RenderTexture texture, int2 resolution, int channels)
    {
        if (channels == 1)
        {
            texture = texture != null ? texture : new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.R16)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
        }
        else if (channels == 2)
        {
            texture = texture != null ? texture : new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RGFloat)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
        }
        else // channels == 3
        {
            texture = texture != null ? texture : new RenderTexture(resolution.x, resolution.y, 24)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
        }
    }
#endregion

#region Modify Textures (3D)

    /// <summary>Assigns a SHALLOW COPY (no reference) of a 3D render texture</summary>
    public void Copy (ref RenderTexture texture, RenderTexture textureA, int3 resolution)
    {
        tcShader.SetTexture(0, "Texture_A", textureA);
        tcShader.SetTexture(0, "Texture_Output", texture);

        ComputeHelper.DispatchKernel (tcShader, "Copy_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Interpolates between two 3D render textures</summary>
    public void Blend (ref RenderTexture textureOutput, RenderTexture textureA, int3 resolution, float lerpWeight = 0.5f)
    {
        RenderTexture texCopy = CreateTexture(resolution, 1);
        Copy(ref texCopy, textureOutput, resolution);

        tcShader.SetFloat("LerpWeight", lerpWeight);

        tcShader.SetTexture(1, "Texture_A", textureA);
        tcShader.SetTexture(1, "Texture_B", texCopy);
        tcShader.SetTexture(1, "Texture_Output", textureOutput);

        ComputeHelper.DispatchKernel(tcShader, "Blend_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Invert a 3D render texture</summary>
    public void Invert (ref RenderTexture texture, int3 resolution)
    {
        tcShader.SetTexture(2, "Texture_Output", texture);

        ComputeHelper.DispatchKernel(tcShader, "Invert_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Saturate a 3D render texture</summary>
    public void Saturate (ref RenderTexture texture, int3 resolution)
    {
        tcShader.SetTexture(3, "Texture_Output", texture);

        ComputeHelper.DispatchKernel(tcShader, "Saturate_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Multiply the brightness values for each pixel in a 3D render texture</summary>
    public void ChangeBrightness (ref RenderTexture texture, int3 resolution, float brightnessFactor)
    {
        tcShader.SetTexture(4, "Texture_Output", texture);

        tcShader.SetFloat("BrightnessFactor", brightnessFactor);

        ComputeHelper.DispatchKernel(tcShader, "ChangeBrightness_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Add/subtract the brightness values for each pixel in a 3D render texture by a specific amount</summary>
    public void AddBrightnessFixed (ref RenderTexture texture, int3 resolution, float brightnessAddOn)
    {
        tcShader.SetTexture(5, "Texture_Output", texture);

        tcShader.SetFloat("BrightnessAddOn", brightnessAddOn);

        ComputeHelper.DispatchKernel(tcShader, "AddBrightnessFixed_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Add the brightness values for each pixel in a 3D render texture to the corresponding pixels in another 3D render texture</summary>
    public void AddBrightnessByTexture (ref RenderTexture texture, RenderTexture textureA, int3 resolution, float brightnessFactor = 1.0f)
    {
        tcShader.SetTexture(6, "Texture_A", textureA);
        tcShader.SetTexture(6, "Texture_Output", texture);

        tcShader.SetFloat("BrightnessFactor", brightnessFactor);

        ComputeHelper.DispatchKernel(tcShader, "AddBrightnessByTexture_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Subtract the brightness values for each pixel in a 3D render texture to the corresponding pixels in another 3D render texture</summary>
    public void SubtractBrightnessByTexture (ref RenderTexture texture, RenderTexture textureA, int3 resolution, float brightnessFactor = 1.0f)
    {
        tcShader.SetTexture(7, "Texture_A", textureA);
        tcShader.SetTexture(7, "Texture_Output", texture);

        tcShader.SetFloat("BrightnessFactor", brightnessFactor);

        ComputeHelper.DispatchKernel(tcShader, "SubtractBrightnessByTexture_3D_F1", resolution, tbShaderThreadSize);
    }

    /// <summary>Apply a gaussian blur effect to a 3D render texture</summary>
    public void GaussianBlur (ref RenderTexture texture, int3 resolution, int smoothingRadius = 2, int iterations = 1)
    {
        for (int i = 0; i < iterations; i++)
        {
            RenderTexture texCopy = CreateTexture(resolution, 1);
            Copy(ref texCopy, texture, resolution);

            tcShader.SetTexture(8, "Texture_A", texCopy);
            tcShader.SetTexture(8, "Texture_Output", texture);

            tcShader.SetVector("Resolution", new Vector3(resolution.x, resolution.y, resolution.z));
            tcShader.SetFloat("SmoothingRadius", smoothingRadius);

            ComputeHelper.DispatchKernel(tcShader, "GaussianBlur_3D_F1", resolution, tbShaderThreadSize);
        }
    }

    /// <summary>Apply a box blur effect to a 3D render texture</summary>
    public void BoxBlur (ref RenderTexture texture, int3 resolution, int smoothingRadius = 2, int iterations = 1)
    {
        for (int i = 0; i < iterations; i++)
        {
            RenderTexture texCopy = CreateTexture(resolution, 1);
            Copy(ref texCopy, texture, resolution);

            tcShader.SetTexture(9, "Texture_A", texCopy);
            tcShader.SetTexture(9, "Texture_Output", texture);

            tcShader.SetVector("Resolution", new Vector3(resolution.x, resolution.y, resolution.z));
            tcShader.SetFloat("SmoothingRadius", smoothingRadius);

            ComputeHelper.DispatchKernel(tcShader, "BoxBlur_3D_F1", resolution, tbShaderThreadSize);
        }
    }
#endregion

#region Set Noise Textures

    /// <summary>Creates a perlin noise texture and appends it to a 3D render texture</summary>
    public void SetPerlin (ref RenderTexture texture, int3 resolution, int cellSize, int rngSeed)
    {
        // -- PERLIN_3D -> PerlinNoise --

        UpdateScriptTextures(resolution, cellSize);

        int NumPasses = (int)Mathf.Log(cellSize, 2);

        ngShader.SetInt("RngSeed", rngSeed);
        ngShader.SetInt("NumPasses", NumPasses);
        ngShader.SetInt("MaxNoiseCellSize", cellSize);

        int cellSizeIterator = cellSize*2;
        for (int pass = 0; pass < NumPasses; pass++)
        {
            cellSizeIterator /= 2;
            ngShader.SetInt("NoiseCellSize", cellSizeIterator);
            ngShader.SetInt("PassCount", pass);

            ComputeHelper.DispatchKernel(ngShader, "GenerateVectorMap", resolution / cellSizeIterator, ngShaderThreadSize);

            ngShader.SetTexture(1, "PerlinNoise", texture);
            ComputeHelper.DispatchKernel(ngShader, "Perlin", resolution, ngShaderThreadSize);
        }
    }
    /// <summary>Creates a voronoi noise texture and appends it to a 3D render texture</summary>
    public void SetVoronoi (ref RenderTexture texture, int3 resolution, int cellSize, int rngSeed)
    {
        // -- VORONOI_3D -> VoronoiNoise --

        UpdateScriptTextures(resolution, cellSize);

        ngShader.SetInt("RngSeed", rngSeed);
        ngShader.SetInt("MaxNoiseCellSize", cellSize);

        ComputeHelper.DispatchKernel(ngShader, "GeneratePointsMap", resolution / cellSize, ngShaderThreadSize);
        ngShader.SetTexture(3, "VoronoiNoise", texture);
        ComputeHelper.DispatchKernel(ngShader, "Voronoi", resolution, ngShaderThreadSize);
    }
#endregion

#region Class

    /// <summary>Updates INTERNAL script textures</summary>
    /// <remarks>Should only be called from WITHIN the internal class, or at OBJECT INITIALISATION</remarks>
    public void UpdateScriptTextures (int3 newResolution, int newCellSize)
    {
        bool3 resolutionHasChanged = newResolution != LastResolution;
        bool cellSizeHasChanged = newCellSize != LastCellSize;
        bool settingsHasChanged = resolutionHasChanged.x || resolutionHasChanged.y || resolutionHasChanged.z || cellSizeHasChanged;

        if (!settingsHasChanged) { return; }

        T_VectorMap = CreateTexture(newResolution / newCellSize, 3);

        T_PointsMap = CreateTexture(newResolution / newCellSize, 3);

        LastResolution = newResolution;
        LastCellSize = newCellSize;
    }
#endregion
}