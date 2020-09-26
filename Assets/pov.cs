using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pov : MonoBehaviour
{
    void UpdateVector(Vector3 rotation) {
        transform.position = Quaternion.Euler(rotation) * transform.position;
    }
}
