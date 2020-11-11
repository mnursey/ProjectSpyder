﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera c;

    void Awake()
    {
        c = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (c != null)
            transform.LookAt(c.transform);
        else
            gameObject.SetActive(false);
    }
}
