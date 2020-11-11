using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public interface IUnit{
    void SetMoveTarget(Vector3 target);
    void ClearMoveTarget();
    Vector3 GetMoveTarget();

    void SetAttackTarget(IUnit target);
    void ClearAttackTarget();
    IUnit GetAttackTarget();

    GameObject GetGameObject();

    void ReceiveAttack(Attack atk);
    bool IsDestroyed();
    bool IsFriendly();

    Vector3 GetPos();
}


public class PlayerControls : MonoBehaviour
{
    // Box selection vars
    public Mesh boxSelectMesh;
    private GameObject boxSelectObject;
    private BoxSelectObjController boxSelectController;
    bool boxSelectActive = false;

    // Other stuff
    private List<GameObject> selectedObjects;
    private Vector3 lastWaypointSet;

    [Tooltip("The minimum allowable distance between consecutive waypoints")]
    public float minWaypointDistanceBetween = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        selectedObjects = new List<GameObject>();
        boxSelectObject = new GameObject();
        boxSelectObject.AddComponent<MeshFilter>();
        boxSelectObject.AddComponent<MeshRenderer>();
        boxSelectController = boxSelectObject.AddComponent<BoxSelectObjController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Select Primary"))
        {
            if (!Input.GetButton("Multi-Select"))
            {
                DeselectAll();
            }
            BoxSelectBegin();
        }
        
        if (Input.GetButton("Select Primary"))
        {
            if(boxSelectActive){
                BoxSelectUpdate();
            }
        }

        if (Input.GetButtonUp("Select Primary"))
        {
            // Selects all friendly units
            if(boxSelectActive){
                BoxSelectEnd();
            }
        }

        if(Input.GetButton("Multi-Select"))
        {
            // Assigns attack target
            if(Input.GetButtonDown("Select Secondary"))
            {
                bool hitEnempy = SetAttackTarget();
            }
        } else
        {
            if (Input.GetButtonDown("Select Secondary"))
            {

                SetWaypoint(true);
            }
            else if (Input.GetButton("Select Secondary"))
            {
                SetWaypoint(false);
            }
        }

        if(Input.GetKeyDown("space"))
        {
            ClearSelectedEntityWaypoints();
        }

    }

    void SetWaypoint(bool buttonDown)
    {
        RaycastHit hitResult;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitResult))
        {
            float dist = Vector3.Distance(lastWaypointSet, hitResult.point);
            //float angle = Vector3.Angle(lastWaypointSet, hitResult.point);
            if(dist > minWaypointDistanceBetween || buttonDown)
            {
                AddWaypointToSelectedEntities(hitResult.point, buttonDown);
                lastWaypointSet = hitResult.point;
            }
        }
    }

    // Update our box select controller to begin a selection
    void BoxSelectBegin()
    {
        boxSelectActive = true;
        boxSelectController.SetVisibility(true);
        RaycastHit hitResult;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitResult))
        {
            boxSelectController.SetBoxBegin(hitResult.point);
        }
    }

    // Update the box select controller to reflect current mouse position (while holding mouse down)
    void BoxSelectUpdate()
    {
        RaycastHit hitResult;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitResult))
        {
            boxSelectController.SetBoxEnd(hitResult.point);
        }
    }

    // Handle the box trace when we complete our selection. She may not look like much, but she's got it where it counts, kid.
    void BoxSelectEnd()
    {
        boxSelectActive = false;
        boxSelectController.SetVisibility(false);
        Vector3 boxCastCenter = boxSelectController.boxCenterLocation;
        Vector3 boxHalfExtents = boxSelectController.selectionBoundsCenter;
        boxCastCenter.y += 10.0f;
        boxHalfExtents.x = Mathf.Abs(boxHalfExtents.x);
        boxHalfExtents.z = Mathf.Abs(boxHalfExtents.z);

        RaycastHit[] hitResults = Physics.BoxCastAll(boxCastCenter, boxHalfExtents, Vector3.down);

        foreach (RaycastHit hit in hitResults)
        {
            if (hit.collider.gameObject.GetComponent<WaypointHandler>() != null)
            {
                AddSelectedObject(hit.collider.gameObject);
            }
        }
    }

    // Handle the ray casting portion of selecting entities using individual (one-by-one) selection
    bool SetAttackTarget()
    {
        bool hitEnemy = false;
        RaycastHit hitResult;
        GameObject selectedObject;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitResult))
        {
            selectedObject = hitResult.collider.gameObject;
            WaypointHandler WH = selectedObject.GetComponent<WaypointHandler>();
            if (WH)    // Check if hit result is an entity object. DO THIS BETTER.
            {
                if(selectedObjects.Count > 0){
                    SetSelectedEntityAttackTargets((IUnit)WH.entityController);
                    hitEnemy = true;
                    Debug.Log("Hit with raycast enemy");
                }
            }
        }

        return hitEnemy;
    }

    // Adds a single object to our selection list
    void AddSelectedObject(GameObject selectedObject)
    {
        if (!selectedObjects.Contains(selectedObject) && ClientGameRunner.Instance.IsOurUnit(selectedObject))
        {
            //Debug.Log("Selecting object");
            selectedObjects.Add(selectedObject);
            UpdateOutlineOnObject(selectedObject, true);
        }
    }

    // Deselect all selected objects and remove disable their outline components
    void DeselectAll()
    {
        foreach (GameObject obj in selectedObjects)
        {
            if(obj != null)
            {
                UpdateOutlineOnObject(obj, false);
            }
        }
        selectedObjects.Clear();
    }

    // Updates the state of the outline component on an object (if it has one, which selectable objects should)
    void UpdateOutlineOnObject(GameObject obj, bool isObjectSelected)
    {
        if(obj == null)
        {
            DeselectAll();
        }
        var outlineComponent = obj.GetComponent<Outline>();
        if(outlineComponent != null)
        {
            if(isObjectSelected)
            {
                outlineComponent.OutlineColor = Color.yellow;
            } else
            {
                outlineComponent.OutlineColor = Color.white;
            }
        }
    }

    // Adds waypoints to all the selected entities based on the input point
    void AddWaypointToSelectedEntities(Vector3 waypoint, bool forcePlacement)
    {
        Vector3 groupMidPoint = new Vector3();

        // Get the point in the middle of all the entities
        foreach (GameObject entity in selectedObjects)
        {
            groupMidPoint += entity.transform.position;
        }
        groupMidPoint /= selectedObjects.Count;

        // Get the translation from each entity to the mid (avg) point
        foreach (GameObject entity in selectedObjects)
        {
            var waypointHandler = entity.GetComponent<WaypointHandler>();
            if (waypointHandler != null)
            {
                Vector3 offsetFromMidpoint = entity.transform.position - groupMidPoint;
                waypointHandler.AddWaypoint(waypoint + offsetFromMidpoint, forcePlacement);
            }
            else
            {
                Debug.Log("PlayerControls.cs - Unable to add waypoint because selected entities did not have waypoint controller!");
            }
        }
    }

    void ClearSelectedEntityWaypoints(){
        foreach (GameObject entity in selectedObjects){
            WaypointHandler WH = entity.GetComponent<WaypointHandler>();
            if (WH){
                WH.ClearWaypoints();
            }
        }
    }

    void SetSelectedEntityAttackTargets(IUnit target){
        foreach (GameObject entity in selectedObjects){
            Debug.Log("Setting attack target...");
            WaypointHandler WH = entity.GetComponent<WaypointHandler>();
            if (WH && !ClientGameRunner.Instance.IsOurUnit(target.GetGameObject())){
                if (WH.mode == WaypointHandlerMode.ONLINE)
                {
                    Debug.Log("Issued attack cmd");
                    ClientGameRunner.Instance.IssueComand(entity.GetComponent<IUnit>(), target);
                }
                else
                {
                    WH.entityController.SetAttackTarget(target);
                }
            }
        }
    }

    void ClearSelectedEntityAttackTargets(){
        foreach (GameObject entity in selectedObjects){
            WaypointHandler WH = entity.GetComponent<WaypointHandler>();
            if (WH){
                if (WH.mode == WaypointHandlerMode.ONLINE)
                {
                    ClientGameRunner.Instance.IssueComand(entity.GetComponent<IUnit>(), null);
                }
                else
                {
                    WH.entityController.ClearAttackTarget();
                }
            }
        }
    }

    /*
    bool SelectedEntitiesHaveWaypoint(){
        foreach (GameObject entity in selectedObjects)
        {
            var waypointHandler = entity.GetComponent<WaypointHandler>();
            if (waypointHandler != null)
            {
                if(entity.hasTarget) return true;
            }
        }

        return false;
    }
    */
}
