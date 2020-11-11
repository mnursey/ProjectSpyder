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
	public VehicleController controller;

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

    public float gunCooldown;

    public IUnit attackTarget = null;
    private Vector3 movementTarget;
    private bool hasMovementTarget = false;

    void Start(){
    	attackTarget = null;
    	if(!rb) rb = GetComponent<Rigidbody>();
    	if(!controller) controller = GetComponent<VehicleController>();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
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


        if(controller == null)
        {
            Debug.LogError("Controller is null");
        }

        controller.ReceiveAttack(atk);
    }

    public bool IsDestroyed(){
        return controller.HP <= 0;
    }

    public bool IsFriendly(){
        return ClientGameRunner.Instance.IsOurUnit(gameObject);
    }

    public Vector3 GetPos(){
    	return rb.position + rb.centerOfMass;
    }

    public void UpdateInputs(){
    	ClearInputs();
    	gunCooldown = Mathf.Max(0, gunCooldown - Time.deltaTime);

    	Vector2 effectiveMovementTarget = Vector2.positiveInfinity;
    	if(hasMovementTarget){
    		effectiveMovementTarget = movementTarget.DiscardY();
    	}

        if (attackTarget != null && attackTarget.IsDestroyed())
        {
            attackTarget = null;
        }

        if (attackTarget != null){
    		Vector2 targetPos = attackTarget.GetPos().DiscardY();

    		switch(atkStyle){
    	
    			case AttackStyle.Ram:
    				if(!hasMovementTarget){
    					effectiveMovementTarget = targetPos;
    				}
    			break;

    			case AttackStyle.FixedGun:
    				if(!hasMovementTarget){
    					float targetAngle = Mathf.Deg2Rad * Vector2.SignedAngle(transform.forward.DiscardY(), targetPos - rb.position.DiscardY());
    					steeringInput = Mathf.Pow(Mathf.Min(1-Mathf.Cos(targetAngle), 1), 0.2f) * (-Mathf.Sign(targetAngle));
    					ShootIfAble();
    				}
    			break;

    			case AttackStyle.TurretGun:
    				targetTurretAngle = Vector2.SignedAngle(transform.forward.DiscardY(), targetPos - rb.position.DiscardY());
    				ShootIfAble();
    			break;
    		}

    	}else{
    		targetTurretAngle = 0;
    	}

    	if(manualControl){
    		GetKeyboardInputs();
    		return;
    	}

    	if(effectiveMovementTarget.x < 10000000000){
    		DriveToward(effectiveMovementTarget);
    	}else{
    		BrakeIfMoving();
    	}
    }

    void DriveToward(Vector2 nodePos){
    	Vector2 planePos = rb.position.DiscardY();
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

    void BrakeIfMoving(){
    	float speed = rb.velocity.DiscardY().magnitude;
    	if(speed > 0.01f){
    		if(Vector2.Dot(transform.forward, rb.velocity) > 0){
    			brakingInput = 1 - 1/(speed + 1);
    		}else{
    			accelerationInput = 1 - 1/(speed + 1);
    		}
    	}
	}	

	void ShootIfAble(){
        // Return if client
        if (ClientGameRunner.Instance != null)
            return;

		if(gunCooldown > 0) return;
		Vector3 targetDisp = attackTarget.GetPos() - rb.position;
		float targetRange = targetDisp.magnitude;
		if(targetRange > controller.gunRange) return;

        if (atkStyle == AttackStyle.FixedGun){
			if(Vector2.Angle(transform.forward.DiscardY(), (attackTarget.GetPos() - rb.position).DiscardY()) > 1) return;
		}else if(atkStyle == AttackStyle.TurretGun){
			//Debug.Log(-controller.turret.localEulerAngles.y - targetTurretAngle);
			if(Mathf.Abs(-controller.turret.localEulerAngles.y - targetTurretAngle)%360 > 1) return;
		}else{
            return;
		}

		Vector3 raycastOrigin = rb.position;
		if(Physics.Raycast(raycastOrigin, attackTarget.GetPos() - raycastOrigin, targetRange, LayerMask.GetMask("Terrain"))){
			//Debug.Log(gameObject.name + "blocked by terrain.");
			//GameObject.Find("Test Ball").transform.position = attackTarget.GetPos();
			return;
		} 

		Vector3 hitPoint = Vector3.positiveInfinity;
		RaycastHit[] hits = Physics.RaycastAll(raycastOrigin, attackTarget.GetPos() - raycastOrigin, Mathf.Infinity, LayerMask.GetMask("Entity"));
		foreach(RaycastHit hit in hits){
			if(hit.collider.attachedRigidbody != rb && hit.distance < (hitPoint - rb.position).magnitude){
				hitPoint = hit.point;
			}
			
		}
		if(hitPoint.x > 1000000000){
			hitPoint = attackTarget.GetPos() - targetDisp.normalized;
		}

        //GameObject.Find("Test Ball").transform.position = hitPoint;

        Debug.DrawLine(attackTarget.GetGameObject().transform.position, transform.position);

		attackTarget.ReceiveAttack(new Attack(controller.gunDamage, 
								   DamageType.Ballistic, 
								   hitPoint,
								   controller.gunForce,
								   controller.gunExplosionRadius));

		gunCooldown = 1/controller.fireRate;
        ServerGameRunner.Instance.em.GetEntity(this.gameObject).shot = true;
	}

	void GetKeyboardInputs(){
		steeringInput = Input.GetAxis("Horizontal");
		accelerationInput = Mathf.Max(Input.GetAxis("Vertical"), 0);
		brakingInput = Mathf.Max(-Input.GetAxis("Vertical"), 0);
	}

    void ClearInputs(){
    	steeringInput = 0;
    	accelerationInput = 0;
    	brakingInput = 0;
    }



}
