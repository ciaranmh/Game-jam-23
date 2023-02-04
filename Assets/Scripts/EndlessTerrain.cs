using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

public class EndlessTerrain : MonoBehaviour
{
    private const float MaxViewDist = 450;
    private Transform viewer;
    private static MapGenerator _mapGen;
    private float _offsetMult;
    public Material terrainMat;

    private static Vector2 _viewerPosition;
    private int _chunkSize;
    private int _chunksVisibleInViewDist;
    private const int MaxConcurrentGenerations = 2;

    private readonly Dictionary<Vector2, TerrainChunk> _terrChunkDict = new();
    private readonly List<TerrainChunk> _visibleChunksLastUpdate = new();
    private readonly ConcurrentDictionary<Vector2, (MapData md, MeshData m)> _chunkCompleted = new();
    private readonly HashSet<Vector2> _awaitingGeneration = new();

    private void Start()
    {
        _mapGen = GetComponent<MapGenerator>();
        _chunkSize = _mapGen.mapChunkSize - 1;
        _chunksVisibleInViewDist = Mathf.RoundToInt(MaxViewDist / _chunkSize);
        _offsetMult = 1 / _mapGen.noiseScale;
        viewer = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        _viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        // UpdateVisibleChunks();
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        foreach (var preChunk in _chunkCompleted.ToList())
        {
            Debug.Log(preChunk.Value.md.colorMap);
            _terrChunkDict[preChunk.Key] =
                new TerrainChunk(preChunk.Key, _chunkSize, transform,
                    TextureGenerator.TextureFromColourMap(preChunk.Value.md.colorMap, _mapGen.mapChunkSize,
                        _mapGen.mapChunkSize),
                    preChunk.Value.m.CreateMesh());
            _chunkCompleted.Remove(preChunk.Key, out var _);
        }

        foreach (var chunk in _visibleChunksLastUpdate)
        {
            chunk.SetVisible(false);
        }

        _visibleChunksLastUpdate.Clear();

        var currentChunkX = Mathf.RoundToInt(_viewerPosition.x / _chunkSize);
        var currentChunkY = Mathf.RoundToInt(_viewerPosition.y / _chunkSize);

        for (var yOffset = -_chunksVisibleInViewDist; yOffset <= _chunksVisibleInViewDist; yOffset++)
        {
            for (var xOffset = -_chunksVisibleInViewDist; xOffset <= _chunksVisibleInViewDist; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkX + xOffset, currentChunkY + yOffset);

                if (_terrChunkDict.ContainsKey(viewedChunkCoord))
                {
                    _terrChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                    if (_terrChunkDict[viewedChunkCoord].IsVisible())
                    {
                        _visibleChunksLastUpdate.Add(_terrChunkDict[viewedChunkCoord]);
                    }
                }
                else if (!_awaitingGeneration.Contains(viewedChunkCoord) &&
                         _awaitingGeneration.Count < MaxConcurrentGenerations &&
                         !_chunkCompleted.ContainsKey(viewedChunkCoord))
                {
                    _awaitingGeneration.Add(viewedChunkCoord);
                    Task.Run(() =>
                    {
                        try
                        {
                            var md = _mapGen.GenerateMapData(
                                new Vector2(viewedChunkCoord.x * _chunkSize * _offsetMult,
                                    -viewedChunkCoord.y * _chunkSize * _offsetMult));
                            Debug.Log(md.colorMap);
                            _chunkCompleted[viewedChunkCoord] = (md,
                                MeshGenerator.GenerateTerrainMesh(md.heightMap, _mapGen.meshHeightMultiplier,
                                    _mapGen.meshHeightCurve, _mapGen.levelOfDetail));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.StackTrace);
                        }

                        _awaitingGeneration.Remove(viewedChunkCoord);
                    }).ConfigureAwait(false);
                }
            }
        }
    }

    private class TerrainChunk
    {
        internal readonly GameObject _meshObject;
        private readonly Vector2 _pos;
        private Bounds _bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent, MapData md)
        {
            var et = GameObject.FindWithTag("MapGen").GetComponent<EndlessTerrain>();

            _pos = coord * size;
            _bounds = new Bounds(_pos, Vector2.one * size);
            var pos3 = new Vector3(_pos.x, 0, _pos.y);

            var p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = coord.ToString();

            var t = TextureGenerator.TextureFromColourMap(md.colorMap, _mapGen.mapChunkSize, _mapGen.mapChunkSize);
            var m = MeshGenerator.GenerateTerrainMesh(md.heightMap,
                _mapGen.meshHeightMultiplier, _mapGen.meshHeightCurve, _mapGen.levelOfDetail).CreateMesh();
            var r = p.GetComponent<Renderer>();
            r.material = et.terrainMat;
            p.GetComponent<MeshFilter>().mesh = m;
            p.GetComponent<MeshCollider>().sharedMesh = m;
            r.material.mainTexture = t;


            p.tag = "Terrain";
            _meshObject = p;
            _meshObject.transform.position = pos3;
            _meshObject.transform.localScale = (Vector3.one);
            _meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public TerrainChunk(Vector2 coord, int size, Transform parent, Texture t, Mesh m)
        {
            var et = GameObject.FindWithTag("MapGen").GetComponent<EndlessTerrain>();

            _pos = coord * size;
            _bounds = new Bounds(_pos, Vector2.one * size);
            var pos3 = new Vector3(_pos.x, 0, _pos.y);

            var p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = coord.ToString();

            var r = p.GetComponent<Renderer>();
            r.material = et.terrainMat;
            p.GetComponent<MeshFilter>().mesh = m;
            p.GetComponent<MeshCollider>().sharedMesh = m;
            r.material.mainTexture = t;


            p.tag = "Terrain";
            _meshObject = p;
            _meshObject.transform.position = pos3;
            _meshObject.transform.localScale = (Vector3.one);
            _meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            var viewerDist = Mathf.Sqrt(_bounds.SqrDistance(_viewerPosition));
            var visible = viewerDist <= MaxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }
    }
}