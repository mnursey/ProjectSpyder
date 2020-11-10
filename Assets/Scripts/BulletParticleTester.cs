using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletParticleTester : MonoBehaviour
{
    Vector3 begin;
    Vector3 end;
    bool beginSet;
    public GameObject pPrefab1;
    public GameObject pPrefab2;
    public List<GameObject> bulletTrailPrefabs;
    int currentTrailPrefabIndex = 0;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hitResult;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitResult))
            {
                if(!beginSet)
                {
                    begin = hitResult.point + Vector3.up * 1;
                    beginSet = true;
                }
                else
                {
                    end = hitResult.point + Vector3.up * 1;
                    beginSet = false;

                    Quaternion rotation = Quaternion.LookRotation(Vector3.RotateTowards(Vector3.forward, (end - begin), Mathf.PI * 2, 1));
                    var thing = Instantiate(pPrefab1, begin, rotation);
                    BulletTrailController c = thing.gameObject.GetComponent<BulletTrailController>();
                    if(c != null)
                    {
                        c.Init(begin, end);
                    }
                }
            }
        }
    }
}
