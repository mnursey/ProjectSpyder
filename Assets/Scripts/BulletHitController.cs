using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitController : MonoBehaviour
{
    public float lifespan = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifespan);
    }
}
