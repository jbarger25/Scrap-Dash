using System;
using System.Linq;
using UnityEngine;

namespace PGC_Terrain
{
    public class PGCTerrain : MonoBehaviour
    {
        public float perlinScale = 1;
        public float perlinFrequency = 1;
        public float perlinAmplitude = 1;

        public void ApplyPerlinNoise(Terrain terrain, bool additive = false)
        {
            float[,] perlinNoise = Noise.GeneratePerlinNoise(terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution, perlinScale, perlinFrequency, perlinAmplitude);

            ApplyTerrainHeightmap(terrain, perlinNoise, additive);
        }

        public int perlinNumberOfOctaves = 4;
        public float perlinPersistence;
        public float perlinLacunarity;

        public void ApplyMultiplePerlinNoise(Terrain terrain, bool additive = false)
        {
            float[,] perlinNoise = Noise.GenerateMultiplePerlinNoise(terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution, perlinScale, perlinNumberOfOctaves, perlinPersistence, perlinLacunarity);

            ApplyTerrainHeightmap(terrain, perlinNoise, additive);
        }

        public float voronoiMountainRadius;
        public float voronoiMountainHeight;
        public float voronoiMountainCurvature;

        public void ApplyVornoiMountain(Terrain terrain, bool additive = false)
        {
            float[,] voronoiMountain = Noise.GenerateVoronoiMountain(terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution, voronoiMountainRadius, voronoiMountainHeight, voronoiMountainCurvature);

            ApplyTerrainHeightmap(terrain, voronoiMountain, additive);
        }

        public float mpdStartingSpread;
        public float mpdSpreadReductionConstant;

        public void ApplyMPDNoise(Terrain terrain, bool additive = false)
        {
            float[,] mpdNoise = Noise.GenerateMidpointDisplacement(terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution, mpdStartingSpread, mpdSpreadReductionConstant, additive ? terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution) : null);

            ApplyTerrainHeightmap(terrain, mpdNoise, false);
        }

        private void ApplyTerrainHeightmap(Terrain terrain, float[,] heightMap, bool additive = false)
        {
            float[,] currentTerrainHeightmap;
            if (additive)
            {
                currentTerrainHeightmap = GetCurrentHeightmap(terrain);

                for (int x = 0; x < Mathf.Sqrt(heightMap.Length); x++)
                {
                    for (int y = 0; y < Mathf.Sqrt(heightMap.Length); y++)
                    {
                        currentTerrainHeightmap[x, y] += heightMap[x, y];
                    }
                }

                NormalizeFloatArray(currentTerrainHeightmap, 0, 1);
            }
            else
            {
                currentTerrainHeightmap = heightMap;
            }

            terrain.terrainData.SetHeights(0, 0, currentTerrainHeightmap);
        }

        private const int DIRT = 0;
        private const int GRASS = 1;
        private const int ROCK = 2;
        private const int SNOW = 3;

        public float minHeightDirt;
        public float maxHeightDirt;
        public float minSlopeDirt;
        public float maxSlopeDirt;

        public float minHeightGrass;
        public float maxHeightGrass;
        public float minSlopeGrass;
        public float maxSlopeGrass;

        public float minHeightRock;
        public float maxHeightRock;
        public float minSlopeRock;
        public float maxSlopeRock;

        public float minHeightSnow;
        public float maxHeightSnow;
        public float minSlopeSnow;
        public float maxSlopeSnow;

        public void ApplyTerrainTextures(Terrain terrain)
        {
            int alphamapWidth = terrain.terrainData.alphamapWidth;
            int alphamapHeight = terrain.terrainData.alphamapHeight;
            int heightmapWidth = terrain.terrainData.heightmapResolution;
            int heightmapHeight = terrain.terrainData.heightmapResolution;

            // Output splatmap data
            float[,,] splatmap = new float[alphamapWidth, alphamapHeight, 2];

            for (int x = 0; x < alphamapWidth; x++)
            {
                for (int y = 0; y < alphamapHeight; y++)
                {
                    // x and y sample between 0 and 1
                    // They are swapped for some reason...
                    float ySample = x/((float)alphamapWidth);
                    float xSample = y/((float)alphamapHeight);

                    float height = terrain.terrainData.GetHeight((int)(xSample * heightmapWidth), (int)(ySample * heightmapHeight)) / terrain.terrainData.size.y;
                    float slope = terrain.terrainData.GetSteepness(xSample, ySample);

                    float[] weights = new float[2];

                    float dirtHeightInput = maxHeightDirt >= height && minHeightDirt <= height ? (Mathf.Sin(Mathf.PI * height / (maxHeightDirt - minHeightDirt)) + 1) / 2 : 0;
                    float grassHeightInput = maxHeightGrass >= height && minHeightGrass <= height ? (Mathf.Sin(Mathf.PI * height / (maxHeightGrass - minHeightGrass)) + 1) / 2 : 0;
                    float rockHeightInput = maxHeightRock >= height && minHeightRock <= height ? (Mathf.Sin(Mathf.PI * height / (maxHeightRock - minHeightRock)) + 1) / 2 : 0;
                    float snowHeightInput = maxHeightSnow >= height && minHeightSnow <= height ? (Mathf.Sin(Mathf.PI * height / (maxHeightSnow - minHeightSnow)) + 1) / 2 : 0;

                    float dirtSlopeInput = maxSlopeDirt >= slope && minSlopeDirt <= slope ? (Mathf.Sin(Mathf.PI * slope / (maxSlopeDirt - minSlopeDirt)) + 1) / 2 : 0;
                    float grassSlopeInput = maxSlopeGrass >= slope && minSlopeGrass <= slope ? (Mathf.Sin(Mathf.PI * slope / (maxSlopeGrass - minSlopeGrass)) + 1) / 2 : 0;
                    float rockSlopeInput = maxSlopeRock >= slope && minSlopeRock <= slope ? (Mathf.Sin(Mathf.PI * slope / (maxSlopeRock - minSlopeRock)) + 1) / 2 : 0;
                    float snowSlopeInput = maxSlopeSnow >= slope && minSlopeSnow <= slope ? (Mathf.Sin(Mathf.PI * slope / (maxSlopeSnow - minSlopeSnow)) + 1) / 2 : 0;


                    weights[DIRT] = dirtHeightInput;// + dirtSlopeInput;
                    weights[GRASS] = grassHeightInput;// + grassSlopeInput;
                    //weights[ROCK] = rockHeightInput + rockSlopeInput;
                    //weights[SNOW] = snowHeightInput + snowSlopeInput;

                    float weightsTotal = weights.Sum();

                    // Make sum of all weights 1 and set output splatmap
                    for (int i = 0; i < 2; i++)
                    {
                        weights[i] /= weightsTotal;
                        splatmap[x, y, i] = weights[i];
                    }
                }
            }

            // Set texture splatmap
            terrain.terrainData.SetAlphamaps(0, 0, splatmap);
        }

        //Min Max normalization
        private float[,] NormalizeFloatArray(float[,] floatArray, float min, float max)
        {
            for (int i = 0; i < floatArray.GetLength(0); i++)
            {
                for (int j = 0; j < floatArray.GetLength(1); j++)
                {
                    floatArray[i, j] = (floatArray[i, j] - min) / (max - min);
                }
            }

            return floatArray;
        }

        private float[,] GetEmptyHeightmap(Terrain terrain)
        {
            return new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];
        }

        private float[,] GetCurrentHeightmap(Terrain terrain)
        {
            return terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
        }

        private Tuple<float,float> GetMinMax(float [,] floatArray)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            
            for(int i = 0; i < floatArray.GetLength(0); i++)
            {
                for (int j = 0; j < floatArray.GetLength(1); j++)
                {
                    if (floatArray[i, j] > max)
                    {
                        max = floatArray[i,j];
                    }
                    if (floatArray[i, j] < min)
                    {
                        min = floatArray[i, j];
                    }
                }
            }

            return new Tuple<float, float>(min, max);
        }
    }
}
