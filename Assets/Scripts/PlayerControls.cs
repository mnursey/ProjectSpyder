using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField]
    private LayerMask selectableObjectsLayer = LayerMask.GetMask("Everything");

    private List<GameObject> selectedObjects;

    // Start is called before the first frame update
    void Start()
    {
        selectedObjects = new List<GameObject>();        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hitResult;
            Debug.Log("Casting ray");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitResult, selectableObjects))
            {
                Debug.Log("hit something");
                selectedObjects.Add(hitResult.collider.gameObject);
                Debug.Log(hitResult.collider.gameObject);
            }
        }
    }
}
