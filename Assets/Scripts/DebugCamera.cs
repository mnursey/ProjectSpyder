using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCamera : MonoBehaviour
{
    public Transform focus;
    public Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        transform.position = focus.transform.position + offset;
    }
}
