using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class EndlessTerrain : MonoBehaviour
{
    private const float MAXViewDist = 450;
    public Transform viewer;

    private static Vector2 _viewerPosition;
    private int _chunkSize;
    private int _chunksVisibleInViewDist;

    private readonly Dictionary<Vector2, TerrainChunk> _terrChunkDict = new Dictionary<Vector2, TerrainChunk>();
    private readonly List<TerrainChunk> _visibleChunksLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        _chunkSize = MapGenerator.mapChunkSize - 1;
        _chunksVisibleInViewDist = Mathf.RoundToInt(MAXViewDist / _chunkSize);
    }

    private void Update()
    {
        _viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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

    public class TerrainChunk
    {
        public GameObject meshObject;
        public Vector2 pos;
        private Bounds _bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            pos = coord * size;
            _bounds = new Bounds(pos, Vector2.one*size);
            var pos3 = new Vector3(pos.x, 0, pos.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = pos3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            var viewerDist = Mathf.Sqrt(_bounds.SqrDistance(_viewerPosition));
            var visible = viewerDist <= MAXViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}