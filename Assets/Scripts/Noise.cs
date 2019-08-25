using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    
    public enum NormalizeMode
    {
        Local, Global
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode) {
        // scale can never be zero or negative
        if (scale <= 0) {
            scale = 0.0001f;
        }

        System.Random prng = new System.Random(seed);
        Vector2[] octavesOffset = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float frecuencyMod = 1;
        float amplitudeMod = 1;

        for (int i = 0; i < octaves; i++)
        {
            // set random limit to get interesting values
            int limit = 1000;
            float offsetX = prng.Next(-limit,limit) + offset.x;
            float offsetY = prng.Next(-limit,limit) - offset.y;//y goes the other way around
            octavesOffset[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitudeMod;
            // with each octave ammplitude decreases due to persistance having to be between 0 and 1
            amplitudeMod *= persistance;
        }

        float[,] noiseMap = new float[mapWidth, mapHeight];
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                // reset the values for each position
                frecuencyMod = 1;
                amplitudeMod = 1;
                float noiseHeight = 0;

                // process octaves
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth + octavesOffset[i].x) / scale * frecuencyMod;
                    float sampleY = (y - halfHeight + octavesOffset[i].y) / scale * frecuencyMod;

                    // perlin noise is between 0 and 1 -> turn to -1 to 1 to have more interesting noise
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitudeMod;

                    // with each octave ammplitude decreases due to persistance having to be between 0 and 1
                    amplitudeMod *= persistance;
                    // with each octave frecuency increases due to lacunarity having to be greater than 1
                    frecuencyMod *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight) {
                    maxLocalNoiseHeight = noiseHeight;
                } else if (noiseHeight < minLocalNoiseHeight) {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                if(normalizeMode == NormalizeMode.Local){
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                } else {
                    float normalzedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.45f);
                    noiseMap[x, y] = Mathf.Clamp(normalzedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }

}
