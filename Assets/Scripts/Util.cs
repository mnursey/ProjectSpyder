using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
	//Extensions
	public static Vector2 DiscardY(this Vector3 v){
		return new Vector2(v.x, v.z);
	}
}
