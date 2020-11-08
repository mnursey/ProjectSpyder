using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAI : MonoBehaviour
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

    public List<Vector2> nodes = new List<Vector2>();

    void Start(){
    	if(!rb){
    		rb = GetComponent<Rigidbody>();
    	}
    }

    public void UpdateInputs(){
    	if(manualControl){
    		GetKeyboardInputs();
    		return;
    	}
    	if(nodes.Count == 0){
    		BrakeIfMoving();
    	}else{
    		Vector2 planePos = transform.position.DiscardY();
    		Vector2 nodePos = nodes[0];

    		if((planePos - nodePos).magnitude < nodeRadius){
    			nodes.RemoveAt(0);
    			ClearInputs();

    		}else{
	    		//Negative angle means turn right, positive means turn left
	    		float nextNodeAngle = Mathf.Deg2Rad * Vector2.SignedAngle(transform.forward.DiscardY(), nodePos - planePos);
	    		float alignment = Mathf.Cos(nextNodeAngle);
	    		int turnDirection =  -(int)Mathf.Sign(nextNodeAngle);

	    		steeringInput = Mathf.Pow(Mathf.Min(1-alignment, 1), turnDamping) * turnDirection;

	    		//if(rb.velocity.mag)
	    		accelerationInput = Mathf.Pow(Mathf.Max(alignment, 0), accelPrudence);
	    	}




    	}
    }

    void GetKeyboardInputs(){
		steeringInput = Input.GetAxis("Horizontal");
		accelerationInput = Mathf.Max(Input.GetAxis("Vertical"), 0);
		brakingInput = Mathf.Max(-Input.GetAxis("Vertical"), 0);
	}

    void BrakeIfMoving(){
    	float speed = rb.velocity.magnitude;
    	if(speed > 0.01f){
    		if(Vector2.Dot(transform.forward, rb.velocity) > 0){
    			brakingInput = 1 - 1/(speed + 1);
    		}else{
    			accelerationInput = 1 - 1/(speed + 1);
    		}
    	}else{
    		ClearInputs();
    	}
    }

    void ClearInputs(){
    	steeringInput = 0;
    	accelerationInput = 0;
    	brakingInput = 0;
    }



}
