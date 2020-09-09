using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public bool linkScaling;
    public float noiseScale;
    public float fractalScale;

    public bool applyNoise = true;
    public int seed;
    [Range(1,8)]
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public Vector2 offsetNoise;
    [Range(0, 1)]
    public float noisiness;

    public bool applyFractal = true;
    public Vector2 offsetFractal;
    [Range(0, 1)]
    public float fractalness;
    public int maxIter;
    public bool useDistanceEstimator = false;
    public bool smoothFract;

    public bool autoUpdate;
    public float maxHeight;
    public float minHeight;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        maxHeight = 0;
        minHeight = 999999;

        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, fractalScale, applyNoise, seed, octaves, persistance, lacunarity, offsetNoise, noisiness, applyFractal, offsetFractal, fractalness, maxIter, useDistanceEstimator, smoothFract);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                // read min and max height values
                if (currentHeight > maxHeight) { maxHeight = currentHeight; }
                if (currentHeight < minHeight) { minHeight = currentHeight; }

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
    }

    void OnValidate()
    {
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { octaves = 0; }
        if (maxIter < 20) { maxIter = 20; }

    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    [Range(0.0000f,1.0000f)]
    public float height;
    public Color color;
}
