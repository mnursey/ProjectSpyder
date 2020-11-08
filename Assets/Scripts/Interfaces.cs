using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable{
	void TakeHit(int hit);
	Vector3 GetPosition();
}