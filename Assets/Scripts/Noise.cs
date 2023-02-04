using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves,
        float persistence,
        float lacunarity, Vector2 offset)
    {
        var noiseMap = new float[mapWidth, mapHeight];
        
        var octaveOffsets = new Vector2[octaves];
        Random.InitState(seed);
        for (var i = 0; i < octaves; i++)
        {
            var offsetX = Random.Range(-100000, 100000) + offset.x;
            var offsetY = Random.Range(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0) scale = 0.00001f;

        var maxNoise = float.MinValue;
        var minNoise = float.MaxValue;

        var halfWidth = mapWidth / 2f;
        var halfHeight = mapHeight / 2f;

        for (var y = 0; y < mapHeight; y++)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f;

                for (var i = 0; i < octaves; i++)
                {
                    var sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x * frequency;
                    var sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y * frequency;

                    var perlinVal = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinVal * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoise) maxNoise = noiseHeight;
                else if (noiseHeight < minNoise) minNoise = noiseHeight;
                noiseMap[x, y] = Mathf.InverseLerp(-1.1f, 1.3f, noiseHeight);
            }
        }

        return noiseMap;
    }
}