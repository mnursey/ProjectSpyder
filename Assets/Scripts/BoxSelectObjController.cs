using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class BoxSelectObjController : MonoBehaviour
{
    public Vector3 boxSelectBegin = new Vector3();
    public Vector3 boxSelectEnd = new Vector3();
    public Vector3 boxCenterLocation = new Vector3();
    public Vector3 selectionBoundsCenter = new Vector3();
    public float selectionScaleX = 0.0f;
    public float selectionScaleZ = 0.0f;
    public float boxSelectHeight = 5.0f;

    private MeshFilter selectionMeshFilter;
    private MeshRenderer selectionMeshRenderer;
    private Material meshMaterial;


    // Start is called before the first frame update
    void Awake()
    {
        // Init some stuff
        selectionMeshFilter = GetComponent<MeshFilter>();
        selectionMeshFilter.mesh = Resources.Load<Mesh>(@"BoxSelectMeshRounded");

        // Add a material to our renderer
        selectionMeshRenderer = GetComponent<MeshRenderer>();
        meshMaterial = Instantiate(Resources.Load<Material>(@"BoxSelectMat"));
        var materials = selectionMeshRenderer.sharedMaterials.ToList();
        materials.Clear(); 
        materials.Add(meshMaterial);
        selectionMeshRenderer.materials = materials.ToArray();
        selectionMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        selectionMeshRenderer.enabled = false;

        // Make the selection box tall
        transform.localScale = new Vector3(0.0f, boxSelectHeight, 0.0f);
    }


    void Update()
    {
        if (selectionMeshRenderer.enabled)
        {
            Vector3 selectionBounds = boxSelectEnd - boxSelectBegin;
            selectionBoundsCenter = selectionBounds / 2;
            Vector3 selectionCenterOffset = new Vector3(selectionBoundsCenter.x, 0.0f, selectionBoundsCenter.z);
            boxCenterLocation = boxSelectBegin + selectionCenterOffset;
            transform.position = boxCenterLocation;

            selectionScaleX = selectionBounds.x;
            selectionScaleZ = selectionBounds.z;
            transform.localScale = new Vector3(selectionScaleX, transform.localScale.y, selectionScaleZ);
        }
    }


    public void SetVisibility(bool isVisible)
    {
        selectionMeshRenderer.enabled = isVisible;
    }

    public void SetBoxBegin(Vector3 begin)
    {
        boxSelectBegin = begin;
        boxSelectEnd = boxSelectBegin + new Vector3(1.0f, 0.0f, 1.0f);
    }

    public void SetBoxEnd(Vector3 end)
    {
        boxSelectEnd = end;
    }
}
