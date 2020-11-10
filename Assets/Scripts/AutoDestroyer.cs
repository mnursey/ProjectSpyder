using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyer : MonoBehaviour
{
    public float lifespan = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        Object.Destroy(gameObject, lifespan);
    }
}
