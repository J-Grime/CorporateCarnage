using Assets.Scripts.Building;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BuildingZone : MonoBehaviour
{
    [SerializeField] private Texture2D _buildingZoneMap;
    [SerializeField] private bool _regen = false;
    [SerializeField] private TeamColor _teamColor;

    private Renderer _renderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private int _width;
    private int _height;
    private BuildingTile[,] _tileMatrix;

    internal int Width =>
        _width;

    internal int Height
        => _height;

    internal TeamColor TeamColor
        => _teamColor;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();

        this.Resize();
    }

    private void Resize()
    {
        var mesh = this.GenerateZoneMap();
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    private void Update()
    {
        if (_regen)
        {
            _regen = false;
            Resize();
        }
    }

    public Vector3 GetNearestEdge(Vector3 point)
    {
        var localX = point.x - transform.position.x;
        var localZ = point.z - transform.position.z;

        localX = Mathf.RoundToInt(localX);
        localZ = Mathf.RoundToInt(localZ);

        return new Vector3(localX + transform.position.x,
                           transform.position.y,
                           localZ + transform.position.z);
    }

    private bool LoadTileMatrix()
    {
        _tileMatrix = null;
        _height = 0;
        _width = 0;

        if (_buildingZoneMap == null || _buildingZoneMap.width == 0 || _buildingZoneMap.height == 0)
        {
            return false;
        }

        _width = _buildingZoneMap.width;
        _height = _buildingZoneMap.height;

        _tileMatrix = new BuildingTile[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var pixel = _buildingZoneMap.GetPixel(x, y);

                _tileMatrix[x, y] = pixel.a > 0.5f ? new BuildingTile(x, y) : null;
            }
        }

        return true;
    }

    private Mesh GenerateZoneMap()
    {
        if (!this.LoadTileMatrix()) return null;

        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();

        var minHalfWidth = -_width / 2;
        var minHalfHeight = -_height / 2;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_tileMatrix[x, y] == null)
                {
                    continue;
                }

                var index = ((y * _width) + x) * 6;

                vertices.AddRange(new Vector3[]{
                    new Vector3(minHalfWidth + x, 0, minHalfHeight + y),
                    new Vector3(minHalfWidth + x, 0, minHalfHeight + y + 1),
                    new Vector3(minHalfWidth + x + 1, 0, minHalfHeight + y + 1),

                    new Vector3(minHalfWidth + x, 0, minHalfHeight + y),
                    new Vector3(minHalfWidth + x + 1, 0, minHalfHeight + y + 1),
                    new Vector3(minHalfWidth + x + 1, 0, minHalfHeight + y)
                });

                uvs.AddRange(new Vector2[]
                {
                    new Vector2(0,0),
                    new Vector2(0,1),
                    new Vector2(1,1),

                    new Vector2(0,0),
                    new Vector2(1,1),
                    new Vector2(1,0),
                });
            }
        }

        var triangles = new int[vertices.Count];
        var normals = new Vector3[vertices.Count];

        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
            normals[i] = Vector3.up;
        }

        newMesh.vertices = vertices.ToArray();
        newMesh.uv = uvs.ToArray();
        newMesh.normals = normals;

        newMesh.triangles = triangles;

        return newMesh;
    }

    internal bool CanPlaceStructure(Structure structure, Vector3 worldPosition, int rotationFrame, out Vector3 placePosition, out BuildingTile desiredTile)
    {
        placePosition = worldPosition;

        var relativeX = worldPosition.x - (transform.position.x - _width / 2);
        var relativeZ = worldPosition.z - (transform.position.z - _height / 2);

        switch (structure.StructurePlacement)
        {
            case StructurePlacement.OnEdge:
                
                if (rotationFrame % 2 == 0)
                {
                    placePosition.x = (transform.position.x - (_width / 2)) + Mathf.FloorToInt(relativeX) + 0.5f;
                    placePosition.z = (transform.position.z - (_height / 2)) + Mathf.RoundToInt(relativeZ);
                }
                else
                {
                    placePosition.x = (transform.position.x - (_width / 2)) + Mathf.RoundToInt(relativeX);
                    placePosition.z = (transform.position.z - (_height / 2)) + Mathf.FloorToInt(relativeZ) + 0.5f;
                }

                desiredTile = null;

                return true;
            case StructurePlacement.OverTile:

                placePosition.x = (transform.position.x - (_width / 2)) + Mathf.FloorToInt(relativeX) + 0.5f;
                placePosition.z = (transform.position.z - (_height / 2)) + Mathf.FloorToInt(relativeZ) + 0.5f;

                desiredTile = null;

                return true;
            default:
                desiredTile = null;
                return false;
        }

        desiredTile = null;
        return false;
    }

    internal BuildingTile GetTile(int xID, int yID)
        => _tileMatrix[xID, yID];
}
