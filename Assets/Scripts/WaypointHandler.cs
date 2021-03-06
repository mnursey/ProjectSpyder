﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointHandlerMode { OFFLINE, ONLINE }

public class WaypointHandler : MonoBehaviour
{
    [SerializeField]
    private Queue<Vector3> waypoints = new Queue<Vector3>();
    private Vector3 validationPoint;

    private Vector3 prevAddedWaypoint;
    private Vector3 prevReachedWaypoint;
    private Vector3 prevDisp;

    private LineRenderer waypointLine;

    public float waypointRadius = 5f;
    public float minWaypointAngleDelta = 0;

    // Detect target arrival within radius for first and last targets in a waypoint sequence
    public bool useRadiusArrivalDetection = true;

    public WaypointHandlerMode mode;

    // References to the possible controller scripts
    public IUnit entityController;

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
        waypointLine.positionCount = 1;
        waypointLine.widthCurve = new AnimationCurve(new Keyframe(0, waypointLineWidth));
        waypointLine.enabled = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        entityController = GetComponent<IUnit>();
    }


    void Update()
    {
        if(waypoints.Count >= 1)
        {
            waypointLine.SetPosition(0, transform.position);

            if (CheckArrivedAtTarget())
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
            return distanceToTarget.magnitude < Mathf.Min(distanceFromPrev.magnitude, waypointRadius);
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
        // Can we do prevReachedWaypoint = waypoints.Dequeue() ?
        waypoints.Dequeue();
        prevReachedWaypoint = entityController.GetMoveTarget();
        RemovePointFromLineRenderer();

        // Update controller target if there are points remaining
        if (waypoints.Count >= 1){
            if(mode == WaypointHandlerMode.OFFLINE)
            {
                entityController.SetMoveTarget(waypoints.Peek());
            }

            if (mode == WaypointHandlerMode.ONLINE)
            {
                ClientGameRunner.Instance.IssueComand(entityController, waypoints.Peek(), true);
            }
        }
        else
        {
            // Clear target and hide line if no points remaining
            waypointLine.enabled = false;
            waypointLine.positionCount = 0;

            if (mode == WaypointHandlerMode.OFFLINE)
            {
                entityController.ClearMoveTarget();
            }

            if (mode == WaypointHandlerMode.ONLINE)
            {
                ClientGameRunner.Instance.IssueComand(entityController, new Vector3(), false);
            }
        }
    }


    // Add a waypoint
    public void AddWaypoint(Vector3 waypoint, bool forcePlacement)
    {
        if(waypoints.Count == 0){
            prevReachedWaypoint = entityController.GetPos();
            AddPointToLineRenderer(prevReachedWaypoint);
            prevAddedWaypoint = waypoint;

            validationPoint = waypoint + (waypoint - prevReachedWaypoint).normalized*0.1f;

            if (mode == WaypointHandlerMode.OFFLINE)
            {
                entityController.SetMoveTarget(waypoint);
            }

            if (mode == WaypointHandlerMode.ONLINE)
            {
                ClientGameRunner.Instance.IssueComand(entityController, waypoint, true);
            }

            waypoints.Enqueue(waypoint);

        }else{
            Vector3 currentDisp = waypoint - prevAddedWaypoint;
            float angle = (prevDisp != Vector3.zero) ? Vector3.Angle(prevDisp, currentDisp) : Mathf.Infinity;
            if(forcePlacement || waypoints.Count < 2 || angle >= minWaypointAngleDelta || (waypoint - prevAddedWaypoint).magnitude > 40){
                prevAddedWaypoint = waypoint;
                prevDisp = currentDisp;
                waypoints.Enqueue(waypoint);

            }else{
                Vector3[] waypointArray = waypoints.ToArray();
                waypointArray[waypointArray.Length-1] = waypoint;
                waypoints = new Queue<Vector3>(waypointArray);

                RemoveLastPointFromLineRenderer();
                
                if(waypoints.Count == 1){
                    if (mode == WaypointHandlerMode.OFFLINE)
                    {
                        entityController.SetMoveTarget(waypoint);
                    }

                    if (mode == WaypointHandlerMode.ONLINE)
                    {
                        ClientGameRunner.Instance.IssueComand(entityController, waypoint, true);
                    }
                }
            }
        }

        AddPointToLineRenderer(waypoint);
    }

    public void ClearWaypoints(){
        waypoints.Clear();

        if (mode == WaypointHandlerMode.OFFLINE)
        {
            entityController.ClearMoveTarget();
        }

        if (mode == WaypointHandlerMode.ONLINE)
        {
            ClientGameRunner.Instance.IssueComand(entityController, new Vector3(), false);
        }

        waypointLine.enabled = false;
        waypointLine.positionCount = 0;
    }


    // Adds a point to the end of the line
    void AddPointToLineRenderer(Vector3 point)
    {
        waypointLine.enabled = true;
        waypointLine.positionCount = Mathf.Max(waypointLine.positionCount + 1, 2);
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
