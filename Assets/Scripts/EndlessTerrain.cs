using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

public class EndlessTerrain : MonoBehaviour
{
    private const float MaxViewDist = 450;
    public Transform viewer;
    private static MapGenerator _mapGen;
    public float _offsetMult;
    private float lasfOffsetMult;

    private static Vector2 _viewerPosition;
    private int _chunkSize;
    private int _chunksVisibleInViewDist;

    private readonly Dictionary<Vector2, TerrainChunk> _terrChunkDict = new();
    private readonly List<TerrainChunk> _visibleChunksLastUpdate = new();

    private void Start()
    {
        _mapGen = GetComponent<MapGenerator>();
        _chunkSize = _mapGen.mapChunkSize - 1;
        _chunksVisibleInViewDist = 1; //Mathf.RoundToInt(MaxViewDist / _chunkSize);
        // _offsetMult = 1 / _mapGen.noiseScale;
    }

    private void Update()
    {
        _viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        // UpdateVisibleChunks();
        UpdateVisibleChunksSpecial();
    }

    private void UpdateVisibleChunks()
    {
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
                else
                    _terrChunkDict[viewedChunkCoord] = new TerrainChunk(viewedChunkCoord, _chunkSize, transform);
            }
        }
    }

    private void UpdateVisibleChunksSpecial()
    {
        if (_offsetMult == lasfOffsetMult) return;

        lasfOffsetMult = _offsetMult;

        var currentChunkX = Mathf.RoundToInt(_viewerPosition.x / _chunkSize);
        var currentChunkY = Mathf.RoundToInt(_viewerPosition.y / _chunkSize);

        for (var yOffset = -_chunksVisibleInViewDist; yOffset <= _chunksVisibleInViewDist; yOffset++)
        {
            for (var xOffset = -_chunksVisibleInViewDist; xOffset <= _chunksVisibleInViewDist; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkX + xOffset, currentChunkY + yOffset);

                if (_terrChunkDict.ContainsKey(viewedChunkCoord))
                {
                    Destroy(_terrChunkDict[viewedChunkCoord]._meshObject);
                    _terrChunkDict[viewedChunkCoord] = new TerrainChunk(viewedChunkCoord, _chunkSize, transform);
                    _terrChunkDict[viewedChunkCoord].SetVisible(true);
                }
                else
                {
                    _terrChunkDict[viewedChunkCoord] = new TerrainChunk(viewedChunkCoord, _chunkSize, transform);
                    _terrChunkDict[viewedChunkCoord].SetVisible(true);
                }
            }
        }
    }

    private class TerrainChunk
    {
        internal readonly GameObject _meshObject;
        private readonly Vector2 _pos;
        private Bounds _bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            var et = GameObject.FindWithTag("MapGen").GetComponent<EndlessTerrain>();
            
            _pos = coord * size;
            _bounds = new Bounds(_pos, Vector2.one * size);
            var pos3 = new Vector3(_pos.x, 0, _pos.y);

            var p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _mapGen.offset = _pos * et._offsetMult;

            if (_mapGen.drawMode == MapGenerator.DrawMode.Mesh){
                var md = _mapGen.GenerateMapData();
                p.GetComponent<MeshFilter>().mesh = MeshGenerator.GenerateTerrainMesh(md.heightMap,
                    _mapGen.meshHeightMultiplier, _mapGen.meshHeightCurve, _mapGen.levelOfDetail).CreateMesh();
                p.GetComponent<MeshRenderer>().material.mainTexture =
                    TextureGenerator.TextureFromColourMap(md.colorMap, _mapGen.mapChunkSize, _mapGen.mapChunkSize);
            }

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