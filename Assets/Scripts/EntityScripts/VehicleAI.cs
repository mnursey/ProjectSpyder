using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AttackStyle{
	Ram,
	FixedGun,
	TurretGun
}


public class VehicleAI : MonoBehaviour, IUnit
{
	Rigidbody rb;

	[Header("Car Inputs")]
    public float steeringInput = 0.0f;
    public float accelerationInput = 0.0f;
    public float brakingInput = 0.0f;

    [Header("Gun Inputs")]
    public float targetTurretAngle;

    [Header("AI Parameters")]
    public float nodeRadius = 1;
    public float turnDamping = 1;
    public float accelPrudence = 1;
    //public float maxSpeed = 5;

	[Header("Important Stuff")]
    public bool manualControl;
    public AttackStyle atkStyle;

    private IUnit attackTarget = null;
    private Vector3 movementTarget;
    private bool hasMovementTarget = false;

    void Start(){
    	attackTarget = null;
    	if(!rb){
    		rb = GetComponent<Rigidbody>();
    	}
    }

    public void SetMoveTarget(Vector3 target)
    {
        movementTarget = target;
        hasMovementTarget = true;
    }

    public void ClearMoveTarget()
    {
        hasMovementTarget = false;
    }

    public Vector3 GetMoveTarget(){
    	return movementTarget;
    }

    public void SetAttackTarget(IUnit target){
        attackTarget = target;
    }

    public void ClearAttackTarget(){
        attackTarget = null;
    }

    public IUnit GetAttackTarget(){
        return attackTarget;
    }

    public void ReceiveAttack(Attack atk){
        GetComponent<VehicleController>().HP -= atk.damage;
    }

    public bool IsDestroyed(){
        return GetComponent<VehicleController>().HP <= 0;
    }

    public bool IsFriendly(){
        return true;
    }

    public Vector3 GetPos(){
    	return transform.position;
    }

    public void UpdateInputs(){
    	if(manualControl){
    		GetKeyboardInputs();
    		return;
    	}

    	ClearInputs();

    	Vector2 effectiveMovementTarget = Vector2.positiveInfinity;
    	if(hasMovementTarget){
    		effectiveMovementTarget = movementTarget.DiscardY();
    	}
    		
    	if(attackTarget != null){
    		Vector2 targetPos = attackTarget.GetPos().DiscardY();

    		switch(atkStyle){
    	
    			case AttackStyle.Ram:
    				if(!hasMovementTarget){
    					effectiveMovementTarget = targetPos;
    				}
    			break;

    			case AttackStyle.FixedGun:
    				if(!hasMovementTarget){
    					float targetAngle = Mathf.Deg2Rad * Vector2.SignedAngle(transform.forward.DiscardY(), targetPos - transform.position.DiscardY());
    					steeringInput = Mathf.Pow(Mathf.Min(1-Mathf.Cos(targetAngle), 1), 0.2f) * (-Mathf.Sign(targetAngle));
    				}
    			break;

    			case AttackStyle.TurretGun:
    				targetTurretAngle = Vector2.Angle(transform.forward.DiscardY(), targetPos - transform.position.DiscardY());
    			break;
    		}

    	}else{
    		targetTurretAngle = 0;
    	}

    	if(effectiveMovementTarget.x < 10000000000){
    		DriveToward(effectiveMovementTarget);
    	}else{
    		BrakeIfMoving();
    	}
    }

    void DriveToward(Vector2 nodePos){
    	Vector2 planePos = transform.position.DiscardY();
		Vector2 planeVel = rb.velocity.DiscardY();

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
