using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAI : MonoBehaviour, IWaypointFollower
{
	Rigidbody rb;

	[Header("Car Inputs")]
    public float steeringInput = 0.0f;
    public float accelerationInput = 0.0f;
    public float brakingInput = 0.0f;

    [Header("AI Parameters")]
    public float nodeRadius = 1;
    public float turnDamping = 1;
    public float accelPrudence = 1;
    public float maxSpeed = 5;

    public bool manualControl;

    public Vector3 movementTarget;
    public bool hasTarget = false;

    void Start(){
    	if(!rb){
    		rb = GetComponent<Rigidbody>();
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

    public Vector3 GetTarget(){
    	return movementTarget;
    }

    public void UpdateInputs(){
    	if(manualControl){
    		GetKeyboardInputs();
    		return;
    	}

    	if(hasTarget){
    		Vector2 planePos = transform.position.DiscardY();
    		Vector2 planeVel = rb.velocity.DiscardY();
    		Vector2 nodePos = movementTarget.DiscardY();

    		//Negative angle means turn right, positive means turn left
    		//float nextNodeAngle = Mathf.Deg2Rad * Vector2.SignedAngle(transform.forward.DiscardY(), nodePos - planePos);
    		float nextNodeAngle = Mathf.Deg2Rad * Vector2.SignedAngle(rb.velocity.DiscardY().normalized + transform.forward.DiscardY(), nodePos - planePos);
    		float alignment = Mathf.Cos(nextNodeAngle);
    		int turnDirection =  -(int)Mathf.Sign(nextNodeAngle);

    		steeringInput = Mathf.Pow(Mathf.Min(1-alignment, 1), turnDamping) * turnDirection;

    		if(false && Vector2.Dot(nodePos - planePos, planeVel) < 0){
    			brakingInput = 1;
    			accelerationInput = 0;
    		}else{
    			brakingInput = 0;
    			accelerationInput = Mathf.Pow(Mathf.Max(alignment, 0), accelPrudence);
    		}
	 
    	}else{
    		BrakeIfMoving();
    	}
    }

    void GetKeyboardInputs(){
		steeringInput = Input.GetAxis("Horizontal");
		accelerationInput = Mathf.Max(Input.GetAxis("Vertical"), 0);
		brakingInput = Mathf.Max(-Input.GetAxis("Vertical"), 0);
	}

    void BrakeIfMoving(){
    	float speed = rb.velocity.DiscardY().magnitude;
    	ClearInputs();
    	if(speed > 0.01f){
    		if(Vector2.Dot(transform.forward, rb.velocity) > 0){
    			brakingInput = 1 - 1/(speed + 1);
    		}else{
    			accelerationInput = 1 - 1/(speed + 1);
    		}
    	}
	}	

    void ClearInputs(){
    	steeringInput = 0;
    	accelerationInput = 0;
    	brakingInput = 0;
    }



}
