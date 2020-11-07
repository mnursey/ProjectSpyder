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

    public bool manualControl;

    List<Vector2> nodes = new List<Vector2>();

    public void UpdateInputs(){
    	if(manualControl){
    		GetKeyboardInputs();
    		return;
    	}
    	if(nodes.Count == 0){
    		BrakeIfMoving();
    	}else{
    		Vector2 planePos = new Vector2(transform.position.x, transform.position.z);
    		Vector2 nodePos = nodes[0];

    		//nextNodeAngle = Vector2.SignedAngle((Vector2)transform.forward, nodePos - planePos);


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
