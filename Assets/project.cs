using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class project : MonoBehaviour
{
    public Transform ut;
    public Transform camera;
    public Transform center;
    Vector3 n;
    Vector3 u;

    // Update is called once per frame
    void Update()
    {
        n = camera.position + center.position;
        u = ut.position;
        transform.position = Vector3.ProjectOnPlane(u, n);
    }
}
