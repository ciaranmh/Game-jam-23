using System;
using System.Collections.Generic;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }

    public DrawMode drawMode;

    public int mapChunkSize = 241;

    [Range(0,6)]
    public int levelOfDetail;
    
    [Tooltip("The scale of the perlin noise")]
    public float noiseScale;
    [Tooltip("The number of octaves the noise has")]
    public int octaves;
    [Range(0.0001f, 1)] public float persistence;
    public float lacunarity;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    [Tooltip("The seed used to generate the map")]
    public int seed;

    public bool useRandomSeed;
    public bool drawGizmos;

    [Tooltip("Whether or not the map is re-generated after each change")]
    public bool autoUpdate;

    public List<TerrainTypes> regions;

    [Range(0, 100)] public int randomFillPercent = 45;

    private int[,] _map;

    private void Start()
    {
        GenerateMapData();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow)) offset.x += 1;
        if (Input.GetKey(KeyCode.LeftArrow)) offset.x -= 1;
        if (Input.GetKey(KeyCode.DownArrow)) offset.y += 1;
        if (Input.GetKey(KeyCode.UpArrow)) offset.y -= 1;
    }

    public MapData GenerateMapData()
    {
        if (useRandomSeed)
            seed = Time.renderedFrameCount;

        var noiseMap =
            Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        var colorMap = new Color[mapChunkSize * mapChunkSize];
        for (var y = 0; y < mapChunkSize; y++)
        {
            for (var x = 0; x < mapChunkSize; x++)
            {
                var curMapHeight = noiseMap[x, y];
                foreach (var region in regions)
                {
                    if (!(curMapHeight <= region.height)) continue;
                    colorMap[y * mapChunkSize + x] = region.colour;
                    break;
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    public void DrawMapInEditor()
    {
        var mapData = GenerateMapData();
        
        var display = FindObjectOfType<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(
                    MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                    TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (_map == null || !drawGizmos) return;
        for (var x = 0; x < mapChunkSize; x++)
        {
            for (var y = 0; y < mapChunkSize; y++)
            {
                Gizmos.color = (_map[x, y] == 1) ? Color.black : Color.white;
                var pos = new Vector3(-mapChunkSize / 2 + x + .5f, -mapChunkSize / 2 + y + .5f, 0);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }

    private void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 1)
            octaves = 1;
        if (noiseScale < 0.0001)
            noiseScale = 0.0001f;
        if (meshHeightMultiplier <= 0)
            meshHeightMultiplier = 1;
    }
}

[Serializable]
public struct TerrainTypes
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}