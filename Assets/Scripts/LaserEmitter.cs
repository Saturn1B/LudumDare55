using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [SerializeField] private Transform laser;
    [SerializeField] private LayerMask layer;

    private void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, 100, ~layer))
		{
            laser.localScale = new Vector3(.02f, .02f, hit.distance);
            laser.localPosition = new Vector3(0, 0, hit.distance / 2);
        }
		else
		{
            laser.localScale = new Vector3(.02f, .02f, 100);
            laser.localPosition = new Vector3(0, 0, 100 / 2);
        }
    }
}
