using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointHandler : MonoBehaviour
{
    [SerializeField]
    private Queue<Vector3> waypoints;

    [SerializeField]
    private LineRenderer waypointLine;

    // References to the possible controller scripts
    public EntityMovementController nonVehicleController;
    public VehicleAI vehicleController;
    private bool isVehicle = false; // True if we should use vehicle controller instead of movement controller

    [Header("Waypoint render settings")]
    [Tooltip("How high the waypoint line should sit above the ground")]
    public float waypointVerticalOffset = 1.0f;

    [Tooltip("How tall should the little spike be at the final waypoint (or THE waypoint, if there's only one)")]
    public float waypointDestinationMarkerHeight = 4.5f;

    [Tooltip("How thick is the line!?!????")]
    public float waypointLineWidth = 0.15f;


    private void Awake()
    {
        // Set up the waypoint line renderer
        waypointLine = gameObject.AddComponent<LineRenderer>();
        waypointLine.material = Instantiate(Resources.Load<Material>(@"SelectionBox_Mat"));
        waypointLine.positionCount = 2;
        waypointLine.widthCurve = new AnimationCurve(new Keyframe(0, waypointLineWidth));
        waypointLine.enabled = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        nonVehicleController = GetComponent<EntityMovementController>();
        vehicleController = GetComponent<VehicleAI>();
        if(vehicleController != null)
        {
            isVehicle = true;
        }

        waypoints = new Queue<Vector3>();
    }


    void Update()
    {
        if(waypoints.Count >= 1)
        {
            if(CheckArrivedAtTarget())
            {
                Arrived();
            }
        }
    }


    // Return true of this object is within a certain distance of its target
    bool CheckArrivedAtTarget(float tolerance = 1.5f)
    {
        if(waypoints.Count >= 1)
        {
            Vector3 distanceToTarget = waypoints.Peek() - transform.position;
            if (distanceToTarget.magnitude <= tolerance)
            {
                return true;
            }
            return false;
        }
        return true;
    }


    // Handle arriving at the first target
    public void Arrived()
    {
        waypoints.Dequeue();
        RemovePointFromLineRenderer();
        if (waypoints.Count >= 1)
        {
            UpdateTargetOnMovementControllers(waypoints.Peek());
        }
        else
        {
            waypointLine.enabled = false;
        }
    }


    // Add a waypoint
    public void AddWaypoint(Vector3 waypoint)
    {
        if(waypoints.Count == 0)
        {
            UpdateTargetOnMovementControllers(waypoint);
        }

        // Add the waypoint to the queue so the entity can follow it
        waypoints.Enqueue(waypoint);

        // Handle the visuals (line drawing)
        AddPointToLineRenderer(waypoint);
    }


    void UpdateTargetOnMovementControllers(Vector3 newTarget)
    {
        if (isVehicle)
        {
            //vehicleController.SetTarget(waypoint);
        }
        else
        {
            if (nonVehicleController != null)
            {
                nonVehicleController.SetTarget(newTarget);
            }
        }
    }


    // Adds a point to the end of the line
    void AddPointToLineRenderer(Vector3 point)
    {
        waypointLine.enabled = true;
        waypointLine.positionCount += 1;
        int linePosCount = waypointLine.positionCount;
        waypointLine.SetPosition(linePosCount - 2, point + new Vector3(0.0f, waypointVerticalOffset, 0.0f));
        waypointLine.SetPosition(linePosCount - 1, point + new Vector3(0.0f, waypointDestinationMarkerHeight, 0.0f));
    }


    // Removes the first point in the waypoint line renderer line and shifts all subsequent points backwards. Not fast, but fast enough.
    void RemovePointFromLineRenderer()
    {
        int linePosCount = waypointLine.positionCount;
        if (linePosCount > 2)
        {
            for (int i = 1; i < linePosCount; i++)
            {
                waypointLine.SetPosition(i - 1, waypointLine.GetPosition(i));
            }
            waypointLine.positionCount--;
        }
    }
}
