using System.Collections;
using UnityEngine;
using snum = System.Numerics;
using System;
using UnityEditor.UIElements;

public static class Noise
{
    
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float noiseScale, float fractalScale, 
        bool generateNoise, int seed, int octaves, float persistance, float lacunarity, Vector2 offsetNoise, float noisiness, 
        bool generateFractal, Vector2 offsetFractal, float fractalness, int maxIter, bool useDistanceEstimator, bool smoothFract)
    {
        Debug.Log("Generating heightmaps...");

        float[,] finalMap = new float[mapWidth, mapHeight];
        float[,] fractMap = new float[mapWidth, mapHeight];
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (noiseScale <= 0)
        {
            noiseScale = 0.0001f;
        }
        if (fractalScale <= 0)
        {
            fractalScale = 0.0001f;
        }

        // NOISE MAP GENERATION
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        if (generateNoise)
        {
            Debug.Log("Generating noise map...");

            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offsetNoise.x;
                float offsetY = prng.Next(-100000, 100000) + offsetNoise.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x-halfWidth) / noiseScale * frequency + octaveOffsets[i].x;
                        float sampleY = (y-halfHeight) / noiseScale * frequency + octaveOffsets[i].y;
                        float outputValue = 0.0f;
                        outputValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += outputValue * amplitude;
                        //noiseMap[x, y] = outputValue;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]) * noisiness;
                }
            }
        }

        // FRACTAL MAP GENERATION
        float maxFractHeight = float.MinValue;
        float minFractHeight = float.MaxValue;
        if (generateFractal)
        {
            Debug.Log("Generating fractal map...");

            // fractal vars
            double thresh = 4d;
            double thresh_half = thresh / 2;
            double log_zn = 0d;
            double nu = 0d;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    // scale (from center)
                    float sampleX = ((x - halfWidth) / fractalScale) + offsetFractal.x;
                    float sampleY = ((y - halfHeight) / fractalScale) + offsetFractal.y;

                    double fractHeight = 0;
                    // convert x and y to complex plane coords
                    float c_real = sampleX + (offsetFractal.x / fractalScale);
                    float c_imag = sampleY + (offsetFractal.y / fractalScale);

                    if (useDistanceEstimator)
                    {
                        //function set_distance(cx, cy) as float
                        //dim c, z, z_new, dz, dz_new as complex
                        snum.Complex c = new snum.Complex(c_real, c_imag);
                        snum.Complex z = new snum.Complex(0, 0);
                        snum.Complex dz = new snum.Complex(1, 0);
                        snum.Complex z_new, dz_new;
                        int cnt = 1;

                        do
                        {
                            z_new = z * z + c; // iterate the quadratic equation
                            dz_new = 2 * z * dz + 1; // iterate the derivative
                            z = z_new; // roll values
                            dz = dz_new; // roll values
                            if (snum.Complex.Abs(z) > (thresh / 2)) {
                                break;
                            } 
                            //cnt = cnt + 1 ’ increment counter
                        } while (cnt > maxIter);

                        //return modulus(z) * log(modulus(z)) / modulus(dz) ’ return the distance
                        z_new = snum.Complex.Abs(z) * snum.Complex.Log(snum.Complex.Abs(z)) / snum.Complex.Abs(dz);

                        // let color_table_index = 0 - k * log(distance_estimator(z, max_iteraions))
                        // k = color_table_size / log(maximum_zoom_magnification)

                    }
                    else // standard escape time heightmap
                    {
                        snum.Complex cz = new snum.Complex(0, 0);
                        snum.Complex cc = new snum.Complex(c_real, c_imag);
                        double iter = 0d;
                        while (iter < maxIter)
                        {
                            iter += 1;
                            cz = (cz * cz) + cc; // iterate the quadratic
                            if (Math.Abs(Math.Sqrt(Math.Pow(cz.Real, 2) + Math.Pow(cz.Imaginary, 2))) > thresh)
                            {
                                fractHeight = (iter * 2d) / maxIter;
                                break;
                            }
                        }
                        if (smoothFract) // calculate decimal portion
                        {
                            if (iter < maxIter)
                            {
                                log_zn = Math.Log(cz.Real * cz.Real + cz.Imaginary * cz.Imaginary) / thresh_half;
                                nu = Math.Log(log_zn / Math.Log(thresh_half)) / Math.Log(thresh_half);
                                fractHeight = iter + 1d - nu;
                            }
                        }

                        if (fractHeight > maxFractHeight)
                        {
                            maxFractHeight = (float)fractHeight;
                        }
                        else if (fractHeight < minFractHeight)
                        {
                            minFractHeight = (float)fractHeight;
                        }
                        fractMap[x, y] = (float)fractHeight;
                     }
                }
            }
            for (int yi = 0; yi < mapHeight; yi++)
            {
                for (int xi = 0; xi < mapWidth; xi++)
                {
                    fractMap[xi, yi] = Mathf.InverseLerp(minFractHeight, maxFractHeight, fractMap[xi, yi]) * fractalness;
                }
            }

        }

        // COMBINE NOISE AND FRACTAL MAPS
        if (generateNoise && generateFractal)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    finalMap[x, y] = fractMap[x, y] * noiseMap[x, y];
                }
            }
        }
        else if (generateNoise) { finalMap = noiseMap; }
        else if (generateFractal) { finalMap = fractMap; }
        return finalMap;
    }
}