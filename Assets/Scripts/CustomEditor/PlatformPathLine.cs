using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPathLine : MonoBehaviour
{
    [SerializeField] private Transform pointA, pointB;
    [SerializeField] private LineRenderer lineRenderer;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		if (lineRenderer.GetPosition(0) != pointA.position)
			lineRenderer.SetPosition(0, pointA.position);
		if (lineRenderer.GetPosition(1) != pointB.position)
			lineRenderer.SetPosition(1, pointB.position);
	}
}
