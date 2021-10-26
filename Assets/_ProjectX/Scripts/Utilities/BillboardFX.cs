using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardFX : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        transform.LookAt(camTransform);
        transform.Rotate(new Vector3(0, 180, 0));
    }
}