using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSHScollider : MonoBehaviour {

	public Vector3 dir;
	Vector3 orgRot;
	void Start(){
		orgRot = transform.rotation.eulerAngles;
	}
	void LateUpdate () {
		transform.rotation = Quaternion.Euler (orgRot.x, transform.rotation.eulerAngles.y, orgRot.z);
	}
}
