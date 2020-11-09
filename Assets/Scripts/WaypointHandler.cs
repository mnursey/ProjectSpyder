using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointHandler : MonoBehaviour
{
    [SerializeField]
    private Queue<Vector3> waypoints = new Queue<Vector3>();
    private Vector3 prevAddedWaypoint;
    private Vector3 prevReachedWaypoint;
    private Vector3 prevDisp;

    [SerializeField]
    private LineRenderer waypointLine;

    public float waypointRadius = 1.5f;
    public float minWaypointAngleDelta = 0;

    // References to the possible controller scripts
    public IWaypointFollower entityController;

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
        entityController = GetComponent<IWaypointFollower>();
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
    bool CheckArrivedAtTarget()
    {
        if(waypoints.Count >= 1 && prevReachedWaypoint.x < 1000000000)
        {
            Vector3 distanceToTarget = waypoints.Peek() - transform.position;
            Vector3 distanceFromPrev = prevReachedWaypoint - transform.position;
            return distanceToTarget.magnitude < distanceFromPrev.magnitude;
        }
        else if(waypoints.Count >= 1)
        {
            Vector3 distanceToTarget = waypoints.Peek() - transform.position;
            if (distanceToTarget.magnitude <= waypointRadius)
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
        prevReachedWaypoint = entityController.GetTarget();
        RemovePointFromLineRenderer();

        if (waypoints.Count >= 1){
            entityController.SetTarget(waypoints.Peek());
            
        }
        else
        {
            waypointLine.enabled = false;
            entityController.ClearTarget();
        }
    }


    // Add a waypoint
    public void AddWaypoint(Vector3 waypoint)
    {
        

        if(waypoints.Count == 0){
            prevReachedWaypoint = Vector3.positiveInfinity;
            prevAddedWaypoint = waypoint;
            entityController.SetTarget(waypoint);
            waypoints.Enqueue(waypoint);

        }else{
            Vector3 currentDisp = waypoint - prevAddedWaypoint;
            float angle = (prevDisp != Vector3.zero) ? Vector3.Angle(prevDisp, currentDisp) : Mathf.Infinity;
            if(waypoints.Count < 2 || angle >= minWaypointAngleDelta || (waypoint - prevAddedWaypoint).magnitude > 10){
                prevAddedWaypoint = waypoint;
                prevDisp = currentDisp;
                waypoints.Enqueue(waypoint);

            }else{
                Vector3[] waypointArray = waypoints.ToArray();
                waypointArray[waypointArray.Length-1] = waypoint;
                waypoints = new Queue<Vector3>(waypointArray);

                RemoveLastPointFromLineRenderer();
                
                if(waypoints.Count == 1){
                    entityController.SetTarget(waypoint);
                }
            }
        }

        AddPointToLineRenderer(waypoint);


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

        waypointLine.Simplify(0.1f);
    }

    void RemoveLastPointFromLineRenderer()
    {
        int linePosCount = waypointLine.positionCount;
        if (linePosCount > 2)
        {
            waypointLine.SetPosition(linePosCount - 2, waypointLine.GetPosition(linePosCount - 3));
            waypointLine.SetPosition(linePosCount - 1, waypointLine.GetPosition(linePosCount - 3) + Vector3.up*waypointDestinationMarkerHeight);
        }

        waypointLine.Simplify(0.1f);
    }
}
