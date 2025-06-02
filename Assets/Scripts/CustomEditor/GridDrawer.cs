using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridDrawer : MonoBehaviour
{
	[SerializeField] private Color baseLineColor = new Color(0.7f, 0.7f, 0.7f, 0.4f);
	[SerializeField] private Color boldLineColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
	[SerializeField] private float gridSpacing = 1f;
	[SerializeField] private int boldLineEvery = 5;
	[SerializeField] private float gridOffset = 0.5f;

	private Material lineMaterial;

	void CreateLineMaterial()
	{
		if(lineMaterial == null)
		{
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			lineMaterial = new Material(shader)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			lineMaterial.SetInt("_ZWrite", 0);
		}
	}

	private void OnPostRender()
	{
		if (!Camera.current) return;

		CreateLineMaterial();
		lineMaterial.SetPass(0);

		GL.Begin(GL.LINES);

		Camera cam = Camera.current;
		Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
		Vector3[] corners = new Vector3[4];

		for (int i = 0; i < 4; i++)
		{
			Ray ray = cam.ViewportPointToRay(new Vector3(i % 2, i / 2, 0));
			if (groundPlane.Raycast(ray, out float enter))
				corners[i] = ray.GetPoint(enter);
		}

		float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x) - 10f;
		float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x) + 10f;
		float minZ = Mathf.Min(corners[0].z, corners[1].z, corners[2].z, corners[3].z) - 10f;
		float maxZ = Mathf.Max(corners[0].z, corners[1].z, corners[2].z, corners[3].z) + 10f;

		minX = Mathf.Floor((minX - gridOffset) / gridSpacing) * gridSpacing + gridOffset;
		maxX = Mathf.Ceil((maxX - gridOffset) / gridSpacing) * gridSpacing + gridOffset;
		minZ = Mathf.Floor((minZ - gridOffset) / gridSpacing) * gridSpacing + gridOffset;
		maxZ = Mathf.Ceil((maxZ - gridOffset) / gridSpacing) * gridSpacing + gridOffset;

		for (float x = minX; x <= maxX; x += gridSpacing)
		{
			GL.Color(Mathf.RoundToInt(x / gridSpacing) % boldLineEvery == 0 ? boldLineColor : baseLineColor);
			GL.Vertex3(x, 0, minZ);
			GL.Vertex3(x, 0, maxZ);
		}

		for (float z = minZ; z <= maxZ; z += gridSpacing)
		{
			GL.Color(Mathf.RoundToInt(z / gridSpacing) % boldLineEvery == 0 ? boldLineColor : baseLineColor);
			GL.Vertex3(minX, 0, z);
			GL.Vertex3(maxX, 0, z);
		}

		GL.End();
	}
}
