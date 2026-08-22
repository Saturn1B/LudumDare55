using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingRotation : MonoBehaviour
{
    void Update()
    {
        transform.eulerAngles -= Vector3.forward / 2;
    }
}
