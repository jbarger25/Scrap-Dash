using UnityEngine;

namespace PGC_Terrain
{
    public class Noise
    {
        public static float[,] GeneratePerlinNoise(int width, int height, float scale, float frequency, float amplitude)
        {
            float[,] perlinNoise = new float[width, height];

            float seed = Random.value * perlinNoise.Length;

            float maxValue = float.MinValue;
            float minValue = float.MaxValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = 0;
                    float sampleX = x / scale * frequency + seed;
                    float sampleY = y / scale * frequency + seed;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Between -1 and 1
                    value += perlinValue * amplitude;

                    if (height > maxValue)
                    {
                        maxValue = value;
                    }
                    else if (height < minValue)
                    {
                        minValue = value;
                    }

                    perlinNoise[x, y] = value;
                }
            }

            return perlinNoise;
        }

        public static float[,] GenerateMultiplePerlinNoise(int width, int height, float scale, int numberOfOctaves, float persistence, float lacunarity)
        {
            float[,] perlinNoise = new float[width, height];

            float[] seeds = new float[numberOfOctaves];
            for (int i = 0; i < numberOfOctaves; i++)
            {
                seeds[i] = Random.value * perlinNoise.Length;
            }

            float maxValue = float.MinValue;
            float minValue = float.MaxValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float value = 0;

                    for (int i = 0; i < numberOfOctaves; i++)
                    {

                        float xSample = x / scale * frequency + seeds[i];
                        float ySample = y / scale * frequency + seeds[i];

                        float perlinValue = Mathf.PerlinNoise(xSample, ySample) * 2 - 1; // Between -1 and 1
                        value += perlinValue * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    if (value > maxValue)
                    {
                        maxValue = value;
                    }
                    else if (value < minValue)
                    {
                        minValue = value;
                    }

                    perlinNoise[x, y] = value;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    perlinNoise[x, y] = Mathf.InverseLerp(minValue, maxValue, perlinNoise[x, y]);
                }
            }

            return perlinNoise;
        }

        public static float[,] GenerateVoronoiMountain(int width, int height, float mountainRadius, float mountainHeight, float mountainCurvature)
        {
            float[,] voronoiMountain = new float[width, height];

            float xPos = Random.value * width;
            float yPos = Random.value * height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float radialDistanceFromCenter = Vector2.Distance(new Vector2(xPos, yPos), new Vector2(x, y));

                    float normalizedDistanceFromCenter = Mathf.Clamp01((mountainRadius - radialDistanceFromCenter) / mountainRadius);

                    float value = normalizedDistanceFromCenter / (mountainCurvature * (1 - normalizedDistanceFromCenter) + 1) * mountainHeight;

                    voronoiMountain[x, y] = value;
                }
            }

            return voronoiMountain;
        }

        public static float[,] GenerateMidpointDisplacement(int width, int height, float startingSpread, float spreadReductionConstant, float[,] input)
        {
            float[,] mpdNoise = new float[width, height]; 

            if (input == null)
            {
                mpdNoise = new float[width, height]; //Unity forces this to be 2^n + 1 already so its fine to leave for MPD... this caused bugs...
                // Initialize 4 corners
                mpdNoise[0, 0] = Random.value;
                mpdNoise[0, height - 1] = Random.value;
                mpdNoise[width - 1, 0] = Random.value;
                mpdNoise[width - 1, height - 1] = Random.value;
            }
            else
            {
                mpdNoise = input;
            }

            //Start MPD Recursive
            MPDRecursive(mpdNoise, 0, 0, width - 1, startingSpread, spreadReductionConstant);

            float[,] returnNoise = new float[width, height];

            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    returnNoise[x, y] = mpdNoise[x, y];
                }
            }

            return mpdNoise;
        }

        private static void MPDRecursive(float[,] mpdNoise, int xOff, int yOff, int squareSize, float currentSpread, float spreadReductionConstant)
        {
            if(squareSize < 2)
            {
                return;
            }

            int xMiddle = xOff + squareSize / 2;
            int yMiddle = yOff + squareSize / 2;

            //Fill Centers of Edges
            mpdNoise[xMiddle, yOff] = (mpdNoise[xOff, yOff] + mpdNoise[xOff + squareSize, yOff]) / 2;
            mpdNoise[xOff, yMiddle] = (mpdNoise[xOff, yOff] + mpdNoise[xOff, yOff + squareSize]) / 2;
            mpdNoise[xOff + squareSize, yMiddle] = (mpdNoise[xOff + squareSize, yOff] + mpdNoise[xOff + squareSize, yOff + squareSize]) / 2;
            mpdNoise[xMiddle, yOff + squareSize] = (mpdNoise[xOff, yOff + squareSize] + mpdNoise[xOff + squareSize, yOff + squareSize]) / 2;

            //Fill Center
            mpdNoise[xMiddle, yMiddle] = (mpdNoise[xMiddle, yOff] + mpdNoise[xOff, yMiddle] + mpdNoise[xOff + squareSize, yMiddle] + mpdNoise[xMiddle, yOff + squareSize]) / 4;

            //Jitter Center
            mpdNoise[xMiddle, yMiddle] += currentSpread * (Random.value * 2 - 1);

            //Do 4 Corners
            int newSquareSize = squareSize / 2;
            MPDRecursive(mpdNoise, xOff, yOff, newSquareSize, currentSpread * spreadReductionConstant, spreadReductionConstant);
            MPDRecursive(mpdNoise, xOff + newSquareSize, yOff, newSquareSize, currentSpread * spreadReductionConstant, spreadReductionConstant);
            MPDRecursive(mpdNoise, xOff, yOff + newSquareSize, newSquareSize, currentSpread * spreadReductionConstant, spreadReductionConstant);
            MPDRecursive(mpdNoise, xOff + newSquareSize, yOff + newSquareSize, newSquareSize, currentSpread * spreadReductionConstant, spreadReductionConstant);
        }
    }
}
