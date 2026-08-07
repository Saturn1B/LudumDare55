using System.Collections.Generic;
using UnityEngine;

public class Prism : MonoBehaviour, IReflectiveSurface
{
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float spawnOffset = 0.05f;

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
            Vector3 exitPoint = GetExitPoint(castDir);

            // derive the real travel direction from the geometry itself,
            // instead of reusing castDir (which may not exactly match)
            Vector3 travelDir = (exitPoint - transform.position).normalized;
            Vector3 spawnPos = exitPoint + travelDir * spawnOffset;

            if (lasers[i] == null)
            {
                lasers[i] = Instantiate(laserPrefab, spawnPos, Quaternion.LookRotation(travelDir), this.transform);
            }
            else
            {
                lasers[i].transform.position = spawnPos;
                lasers[i].transform.rotation = Quaternion.LookRotation(travelDir);
            }
        }
    }

    // finds where the ray exits the prism collider on the far side of its center
    private Vector3 GetExitPoint(Vector3 direction)
    {
        float castDistance = 100f;
        Vector3 farPoint = transform.position + direction * castDistance;
        Ray ray = new Ray(farPoint, -direction);

        if (prismCollider.Raycast(ray, out RaycastHit exitHit, castDistance * 2f))
        {
            return exitHit.point;
        }

        // fallback, shouldn't normally trigger for a convex collider
        return transform.position;
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
}