using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

class VehicleController : MonoBehaviour {

	public Rigidbody rb;
	public Vector3 centerOfMass = Vector3.zero;

	public VehicleAI inputs;

	public Axle[] axles = new Axle[2];

	public Axle frontAxle{
		get{ return axles[Axle.FRONT_AXLE_INDEX]; }
		set{ axles[Axle.FRONT_AXLE_INDEX] = value; }
	}

	public Axle rearAxle{
		get{ return axles[Axle.REAR_AXLE_INDEX]; }
		set{ axles[Axle.REAR_AXLE_INDEX] = value; }
	}

	[Header("Car Performance")]

	public float enginePower;
	public float steeringPower;
	public float brakingPower;
	public bool tankSteering;

	[Header("Other")]

	public float resetHeight;

	Vector3 previousFramePosition;

	//Added props

	public int HP;
	public bool piloted = false; //Whether the vehicle currently has a driver
	public bool overturned = false; //Whether vehicle needs to be flipped back over

	float stoppedThreshold = 0.1f;


	void Start(){
    	if(!rb) rb = GetComponent<Rigidbody>();
    	if(!inputs) inputs = GetComponent<VehicleAI>();

    	rb.centerOfMass = new Vector3(0, rb.centerOfMass.y, (frontAxle.offset.x + rearAxle.offset.x)/2);
    }

	void Update(){
		inputs.UpdateInputs();
	}

	public void Reset(){
		rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, 0.0f);
        transform.position += Vector3.up*resetHeight;
	}


	void FixedUpdate(){
		bool anyWheelGrounded = axles[Axle.REAR_AXLE_INDEX].leftWheel.isGrounded || axles[Axle.REAR_AXLE_INDEX].rightWheel.isGrounded || axles[Axle.FRONT_AXLE_INDEX].leftWheel.isGrounded || axles[Axle.FRONT_AXLE_INDEX].rightWheel.isGrounded;
		overturned = (!anyWheelGrounded && rb.velocity.magnitude < stoppedThreshold);

		if(overturned) Reset();


		SuspensionForce();
        EngineForce();
        SteeringForce();
        BrakingForce();
	}

	void SuspensionForce()
    {
        SuspensionForceAtWheel(axles[Axle.FRONT_AXLE_INDEX], axles[Axle.FRONT_AXLE_INDEX].leftWheel);
        SuspensionForceAtWheel(axles[Axle.FRONT_AXLE_INDEX], axles[Axle.FRONT_AXLE_INDEX].rightWheel);

        SuspensionForceAtWheel(axles[Axle.REAR_AXLE_INDEX], axles[Axle.REAR_AXLE_INDEX].leftWheel);
        SuspensionForceAtWheel(axles[Axle.REAR_AXLE_INDEX], axles[Axle.REAR_AXLE_INDEX].rightWheel);
    }

    void SuspensionForceAtWheel(Axle axle, WheelData wheelData)
    {
        Vector3 compressedWheelPos = WheelPosition(axle, wheelData, false);

        RaycastHit hit;

        int layerMask = ~LayerMask.GetMask("Entities", "Water");
        bool hitSomething = Physics.Raycast(compressedWheelPos, -transform.up, out hit, axle.suspensionHeight, layerMask, QueryTriggerInteraction.Ignore);

        Debug.DrawLine(compressedWheelPos, compressedWheelPos - (transform.up * axle.suspensionHeight), Color.green);

        wheelData.isGrounded = hitSomething;


        if (hitSomething)
        {
            // calculate suspension force

            float suspensionLength = hit.distance;
            float suspensionForceMag = 0.0f;

            wheelData.compression = 1.0f - Mathf.Clamp01(suspensionLength / axle.suspensionHeight);

            // Hooke's Law (springs)

            float springForce = wheelData.compression * -axle.suspensionStiffness;
            suspensionForceMag += springForce;

            // Damping force (try to rest velocity to 0)

            float suspensionCompressionVelocity = (wheelData.compression - wheelData.compressionPrev) / Time.deltaTime;
            wheelData.compressionPrev = wheelData.compression;

            float damperFoce = -suspensionCompressionVelocity * axle.suspensionDampining;
            suspensionForceMag += damperFoce;

            // Only consider component of force that is along contact normal

            float denom = Vector3.Dot(hit.normal, transform.up);
            suspensionForceMag *= denom;

            // Apply suspension force
            Vector3 suspensionForce = suspensionForceMag * -transform.up;
            rb.AddForceAtPosition(suspensionForce, hit.point);


            // calculate friction

            Vector3 wheelVelocity = rb.GetPointVelocity(hit.point);

            Vector3 contactUp = hit.normal;
            Vector3 contactLeft = -transform.right;
            Vector3 contactForward = transform.forward;

            // Calculate sliding velocity (without normal force)
            Vector3 lVel = Vector3.Dot(wheelVelocity, contactLeft) * contactLeft;
            Vector3 fVel = Vector3.Dot(wheelVelocity, contactForward) * contactForward;
            Vector3 slideVelocity = (lVel + fVel) * 0.5f;

            // Calculate current sliding force
            // (4 because we have 4 wheels)
            // TODO use num wheel variable
            Vector3 slidingForce = (slideVelocity * rb.mass / Time.deltaTime) / 4;

            float laterialFriciton = Mathf.Clamp01(axle.laterialFriction);

            Vector3 frictionForce = -slidingForce * laterialFriciton;

            Vector3 longitudinalForce = Vector3.Dot(frictionForce, contactForward) * contactForward;

            // TODO
            // Apply rolling-friction only if player doesn't press the accelerator
            float rollingK = 1.0f - Mathf.Clamp01(axle.rollingFriction);
            longitudinalForce *= rollingK;

            frictionForce -= longitudinalForce;

            rb.AddForceAtPosition(frictionForce, FixToCarCentre(hit.point));

        } else
        {
            // relax suspension
            wheelData.compressionPrev = wheelData.compression;
            wheelData.compression = Mathf.Clamp01(wheelData.compression - axle.suspensionRelaxSpeed * Time.deltaTime);

        }
    }

    void BrakingForce()
    {
    	float effectiveBrakePower = (Vector2.Dot(rb.velocity, transform.forward) > 0) ? brakingPower : brakingPower/2;

        foreach (Axle axle in axles)
        {
            // Refactor this and remove the divide by 4. First count number of braking wheels on all axis
            if (axle.leftWheel.isGrounded)
            {
                rb.AddForce(-transform.forward * effectiveBrakePower * Time.deltaTime * inputs.brakingInput / (2*axles.Length), ForceMode.Acceleration);
            }

            if (axle.rightWheel.isGrounded)
            {
                rb.AddForce(-transform.forward * effectiveBrakePower * Time.deltaTime * inputs.brakingInput / (2*axles.Length), ForceMode.Acceleration);
            }
        }
    }


    void EngineForce()
    {
        foreach(Axle axle in axles)
        {
            if(axle.isPowered)
            {
                // Refactor this and remove the divide by 2. First count number of powered wheels on all axis
                if(axle.leftWheel.isGrounded)
                {
                    rb.AddForce(transform.forward * enginePower * Time.deltaTime * inputs.accelerationInput / 2,  ForceMode.Acceleration);
                }

                if (axle.rightWheel.isGrounded)
                {
                    rb.AddForce(transform.forward * enginePower * Time.deltaTime * inputs.accelerationInput / 2,  ForceMode.Acceleration);
                }
            }
        }
    }

    void SteeringForce()
    {
        Axle axle = frontAxle;

        if (axle.leftWheel.isGrounded)
        {
            rb.AddForceAtPosition(transform.right * steeringPower * Time.deltaTime * inputs.steeringInput / 2, FixToCarCentre(WheelPosition(axle, axle.leftWheel)), ForceMode.Acceleration);
        }

        if (axle.rightWheel.isGrounded)
        {
            rb.AddForceAtPosition(transform.right * steeringPower * Time.deltaTime * inputs.steeringInput / 2, FixToCarCentre(WheelPosition(axle, axle.rightWheel)), ForceMode.Acceleration);
        }

        if(tankSteering){
        	axle = rearAxle;

        	if (axle.leftWheel.isGrounded)
	        {
	            rb.AddForceAtPosition(-transform.right * steeringPower * Time.deltaTime * inputs.steeringInput / 2, FixToCarCentre(WheelPosition(axle, axle.leftWheel)), ForceMode.Acceleration);
	        }

	        if (axle.rightWheel.isGrounded)
	        {
	            rb.AddForceAtPosition(-transform.right * steeringPower * Time.deltaTime * inputs.steeringInput / 2, FixToCarCentre(WheelPosition(axle, axle.rightWheel)), ForceMode.Acceleration);
	        }
        }
    }


    Vector3 WheelPosition(Axle axle, WheelData wheel, bool accountForSuspension = true)
    {
        bool isLeftWheel = IsLeftWheel(axle, wheel);

        // Start with Local Space
        Vector3 wheelPos = Vector3.zero;

        // Apply axle offset and axle width
        wheelPos.Set(wheelPos.x + (axle.width / 2 * (isLeftWheel ? -1f : 1f)), wheelPos.y + axle.offset.y, wheelPos.z + axle.offset.x);

        // Apply suspension
        if(accountForSuspension) 
            wheelPos.Set(wheelPos.x, wheelPos.y - (axle.suspensionHeight * (1f - wheel.compression)), wheelPos.z);

        return transform.TransformPoint(wheelPos);
    }

    static bool IsLeftWheel(Axle axle, WheelData wheel)
    {
        // Determin if left or right wheel
        bool leftWheel = true;

        if (axle.leftWheel == wheel)
        {
            leftWheel = true;
        }
        else
        {
            if (axle.rightWheel == wheel)
            {
                leftWheel = false;

            }
            else
            {
                Debug.LogError("Wheel not appart of axle");
            }
        }

        return leftWheel;
    }

    Vector3 FixToCarCentre(Vector3 point)
    {
        point = transform.InverseTransformPoint(point);
        point.Set(point.x, rb.centerOfMass.y, point.z);
        point = transform.TransformPoint(point);
        return point;
    }

}





public class WheelData
{
    public bool isGrounded = false;

    public RaycastHit hitPoint = new RaycastHit();

    // compression 0 -> fully extended, 1 -> fully compressed
    public float compression = 0.0f;

    public float compressionPrev = 0.0f;
}

[Serializable]
public class Axle
{
    public float width = 0.4f;

    public Vector2 offset = Vector2.zero;

    public float wheelRadius = 0.3f;

    public float laterialFriction = 0.5f;

    public float rollingFriction = 0.1f;

    public float suspensionStiffness = 8500.0f;

    public float suspensionDampining = 3000.0f;

    public float suspensionHeight = 0.55f;

    public float suspensionRelaxSpeed = 1.0f;

    public WheelData leftWheel = new WheelData();

    public WheelData rightWheel = new WheelData();

    public bool isPowered = false;

    public static int FRONT_AXLE_INDEX = 0;

    public static int REAR_AXLE_INDEX = 1;
}