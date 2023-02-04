using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colorMap, int width, int height)
    {
        var tex = new Texture2D(width,height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixels(colorMap);
        tex.Apply();
        return tex;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);

        var colorMap = new Color[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.white, Color.black, heightMap[x, y]);
            }
        }

        return TextureFromColourMap(colorMap,width,height);
    }
}
