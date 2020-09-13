using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabs : MonoBehaviour
{
    public GameObject pre;
    
    public int xRange;
    public int yRange;
    public Vector2 zRange;

    // Start is called before the first frame update
    void Start()
    {
        for (int y = -yRange; y < yRange; y+=3) {
            for (int x = -xRange; x < xRange; x+=3) {
                float z = Random.Range(zRange.y, zRange.x);
                // instantiate pre in x y z
                (Instantiate (pre, new Vector3(x, y, transform.position.z + z), Quaternion.Euler(-90, 0, 0)) as GameObject).transform.parent = transform;
            }
        }
        
    }

}
