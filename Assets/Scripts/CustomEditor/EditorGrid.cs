using UnityEngine;
using System.Collections.Generic;
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EditorGrid : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int gridResolution = 100;
    [SerializeField] private float boldOffset = 2f;
    [SerializeField] private int boldLineEvery = 5;
    [SerializeField] private Color thinLineColor = new Color(0.6f, 0.6f, 0.6f, 0.3f);
    [SerializeField] private Color boldLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    private void OnEnable()
    {
        GenerateGrid();
        transform.position = new Vector3(-0.5f, -0.5f, -0.5f);
    }
    private void Update()
    {
        // No snapping logic needed here
    }
    private void GenerateGrid()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralGrid";
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var indices = new List<int>();
        float extent = gridResolution * cellSize;
        for (int i = -gridResolution; i <= gridResolution; i++)
        {
            float lineCoord = i * cellSize;
            // Calculate world coordinates for each axis, including bold offset
            float worldX = lineCoord + boldOffset;
            float worldZ = lineCoord + boldOffset;
            int xIndex = Mathf.FloorToInt(worldX / cellSize);
            int zIndex = Mathf.FloorToInt(worldZ / cellSize);
            bool isBoldX = xIndex % boldLineEvery == 0;
            bool isBoldZ = zIndex % boldLineEvery == 0;
            Color colorX = isBoldX ? boldLineColor : thinLineColor;
            Color colorZ = isBoldZ ? boldLineColor : thinLineColor;
            // Horizontal (Z)
            vertices.Add(new Vector3(-extent, 0, lineCoord));
            vertices.Add(new Vector3(+extent, 0, lineCoord));
            colors.Add(colorZ);
            colors.Add(colorZ);
            indices.Add(vertices.Count - 2);
            indices.Add(vertices.Count - 1);
            // Vertical (X)
            vertices.Add(new Vector3(lineCoord, 0, -extent));
            vertices.Add(new Vector3(lineCoord, 0, +extent));
            colors.Add(colorX);
            colors.Add(colorX);
            indices.Add(vertices.Count - 2);
            indices.Add(vertices.Count - 1);
        }
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}