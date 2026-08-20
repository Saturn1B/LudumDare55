using System.Collections.Generic;
using UnityEngine;

public class Prism : MonoBehaviour, IReflectiveSurface
{
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float spawnOffset = 0.01f;

    private static readonly float[] angles = { 0f, 90f, -90f };

    private Collider prismCollider;
    private Dictionary<LaserEmitter, GameObject[]> reflections = new Dictionary<LaserEmitter, GameObject[]>();

    private void Awake()
    {
        prismCollider = GetComponent<Collider>();
    }

    public void GenerateReflection(LaserEmitter source, Vector3 hitPosition, Vector3 hitNormal, Vector3 incomingDirection, Vector3 reflectedDirection)
    {
        if (!reflections.TryGetValue(source, out GameObject[] lasers) || lasers == null)
        {
            lasers = new GameObject[angles.Length];
            reflections[source] = lasers;
        }

        Vector3 baseDir = (transform.position - hitPosition).normalized;

        for (int i = 0; i < angles.Length; i++)
        {
            Vector3 castDir = Quaternion.AngleAxis(angles[i], transform.up) * baseDir;
            RefractedRayData exitPoint = GetExitPoint(castDir);

			// derive the real travel direction from the geometry itself,
			// instead of reusing castDir (which may not exactly match)
			Debug.DrawRay(hitPosition, hitNormal, Color.green, 1);

            Vector3 spawnPos = exitPoint.spawnPos + exitPoint.spawnDir * spawnOffset;

            if (lasers[i] == null)
            {
                lasers[i] = Instantiate(laserPrefab, spawnPos, Quaternion.LookRotation(exitPoint.spawnDir), this.transform);
            }
            else
            {
                lasers[i].transform.position = spawnPos;
                lasers[i].transform.rotation = Quaternion.LookRotation(exitPoint.spawnDir);
            }
        }
    }

    // finds where the ray exits the prism collider on the far side of its center
    private RefractedRayData GetExitPoint(Vector3 direction)
    {
        float castDistance = 100f;
        Vector3 farPoint = transform.position + direction * castDistance;
		Ray ray = new Ray(farPoint, -direction);

        RefractedRayData refractedRay = new RefractedRayData();

		if (prismCollider.Raycast(ray, out RaycastHit exitHit, castDistance * 2f))
		{
			refractedRay.spawnPos = exitHit.point;
            refractedRay.spawnDir = exitHit.normal;

            Debug.DrawRay(exitHit.point, Vector3.up, Color.green, 1);
            Debug.DrawRay(exitHit.point, exitHit.normal, Color.green, 1);
            return refractedRay;
        }

        // fallback, shouldn't normally trigger for a convex collider
        return refractedRay;
    }

    public void RemoveReflection(LaserEmitter source)
    {
        if (reflections.TryGetValue(source, out GameObject[] lasers))
        {
            foreach (var laser in lasers)
            {
                if (laser != null) Destroy(laser);
            }
            reflections.Remove(source);
        }
    }

    class RefractedRayData
	{
        public Vector3 spawnPos;
        public Vector3 spawnDir;
	}
}