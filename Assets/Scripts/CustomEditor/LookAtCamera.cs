using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;

    void Update()
    {
        transform.LookAt(cameraTarget);
    }
}
