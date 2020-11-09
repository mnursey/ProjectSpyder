using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Attack
{
	public int damage;
	public float force;
	public float explosionForce;
	public float explosionRadius;
	
    public Attack(int d, float f = 0, float ef = 0, float er = 0){
    	damage = d;
    	force = f;
    	explosionForce = ef;
    	explosionRadius = er;
    }
}
