using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EditorGrid : MonoBehaviour
{
    [SerializeField] private float gridSize = 100f;
    [SerializeField] private Camera targetCamera;

    private void OnEnable()
    {
        GenerateQuad();

        // Si pas de caméra assignée, utiliser la caméra principale
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        // Suivre la caméra pour créer l'effet de grille infinie
        if (targetCamera != null)
        {
            Vector3 camPos = targetCamera.transform.position;
            // Garder seulement les coordonnées X et Z, maintenir Y à 0
            transform.position = new Vector3(camPos.x, 0, camPos.z);
        }
    }

    private void GenerateQuad()
    {
        Mesh mesh = new Mesh();
        mesh.name = "GridQuad";

        // Créer un simple quad qui couvre une grande zone
        float size = gridSize;
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-size, 0, -size),
            new Vector3(size, 0, -size),
            new Vector3(size, 0, size),
            new Vector3(-size, 0, size)
        };

        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        int[] triangles = new int[6]
        {
            0, 1, 2,
            0, 2, 3
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}