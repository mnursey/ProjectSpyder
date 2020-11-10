using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType{
	Ballistic,
	Collision
}

public struct Attack
{
	public int damage;
	public Vector3 point;
	public float force;
	public float explosionRadius;
	public DamageType type;
	
    public Attack(int d, DamageType t = DamageType.Ballistic, Vector3 p = new Vector3(), float f = 0, float er = 0){
    	damage = d;
    	point = p;
    	force = f;
    	explosionRadius = er;
    	type = t;
    }
}
