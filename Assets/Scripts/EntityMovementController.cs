using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class EntityMovementController : MonoBehaviour
{
    public Rigidbody rBody;

    [Header("Movement Settings")]
    [Tooltip("How fast to go!")]
    public float movementSpeedMultiplier = 3.5f;

    [Tooltip("How quick to turn!")]
    public float rotationSpeedMultiplier = 6.0f;

    [Tooltip("How high to fly!")]
    public float targetDistanceFromGround = 1.0f;

    private Vector3 movementTarget;
    private bool hasTarget = false;

    // Start is called before the first frame update
    void Start()
    {
        // Set up the rigid body component
        rBody = GetComponent<Rigidbody>();
        rBody.useGravity = false;
        rBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rBody.drag = 1.0f;
        rBody.angularDrag = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(hasTarget)
        {
            // If we have some distance still to go
            MoveTowardPoint(movementTarget);
            LookAtTarget(movementTarget);
        }
        else
        {
            rBody.velocity = Vector3.zero;
        }
    }

    public void SetTarget(Vector3 target)
    {
        movementTarget = target;
        hasTarget = true;
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

    // Apply some force to a rigidbody on this object in the direction of its target
    void MoveTowardPoint(Vector3 target)
    {
        if(rBody != null)
        {
            // Calculate and apply velocity
            Vector3 targetVelocity = (target - transform.position).normalized;
            targetVelocity *= movementSpeedMultiplier;
            targetVelocity.y = 0.0f;
            rBody.velocity = targetVelocity;

            // Position entity at constant distance above ground
            RaycastHit hitResult;
            if(Physics.Raycast(transform.position, Vector3.down, out hitResult))
            {
                float targetPositionY = hitResult.point.y + targetDistanceFromGround;
                float positionYAdjustment = targetPositionY - transform.position.y;
                transform.Translate(new Vector3(0.0f, positionYAdjustment, 0.0f));
            }
        }
    }

    // Update rotation to face target point
    void LookAtTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        directionToTarget.y = 0.0f;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeedMultiplier);
    }
}
