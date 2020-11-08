using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxSelectObjController : MonoBehaviour
{
    public Vector3 boxSelectBegin;
    public Vector3 boxSelectEnd;
    public Vector3 boxCenterLocation;
    public Vector3 selectionBoundsCenter;
    public float selectionScaleX = 0.0f;
    public float selectionScaleZ = 0.0f;
    public float boxSelectHeight = 5.0f;

    private MeshFilter selectionMeshFilter;
    private MeshRenderer selectionMeshRenderer;
    private Material meshMaterial;
    //private Outline selectionMeshOutline;


    // Start is called before the first frame update
    void Awake()
    {
        // Init some stuff
        selectionMeshFilter = GetComponent<MeshFilter>();
        boxSelectBegin = new Vector3();
        boxSelectEnd = new Vector3();
        boxCenterLocation = new Vector3();

        // Add a material to our renderer
        selectionMeshRenderer = GetComponent<MeshRenderer>();
        meshMaterial = Instantiate(Resources.Load<Material>(@"SelectionBox_Mat"));
        var materials = selectionMeshRenderer.sharedMaterials.ToList();
        //materials[0] = meshMaterial;
        materials.Clear(); 
        materials.Add(meshMaterial);
        selectionMeshRenderer.materials = materials.ToArray();
        selectionMeshRenderer.enabled = false;

        // Make the selection box super tall
        transform.localScale = new Vector3(0.0f, boxSelectHeight, 0.0f);

        /*  // Create the outline component and set its settings
        selectionMeshOutline = GetComponent<Outline>();
        selectionMeshOutline.OutlineWidth = 5.0f;
        selectionMeshOutline.OutlineMode = Outline.Mode.OutlineVisible;
        selectionMeshOutline.enabled = true;*/
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


    public void SetMesh(Mesh mesh)
    {
        if(mesh != null)
        {
            if(selectionMeshFilter != null)
            {
                selectionMeshFilter.mesh = mesh;
            }
            else
            {
                Debug.Log("SelectionObjectController - selectionMeshFilter was null - unable to assign mesh");
            }
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
