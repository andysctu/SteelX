using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSHScollider : MonoBehaviour {
	private bool isLeft;
	public Vector3 localp, rot;
	public Vector3 orgRot;
	void Start(){
		/*isLeft = (transform.parent.parent.name [transform.parent.parent.name.Length - 1] == 'L');
		transform.localPosition = new Vector3 (0.1, 0, 0);
		transform.localRotation = Quaternion.identity;*/
		//orgRot = transform.rotation.eulerAngles;
	}
	void LateUpdate () {
		//transform.rotation = Quaternion.Euler (orgRot.x, transform.rotation.eulerAngles.y, orgRot.z);
		/*Debug.Log(((isLeft)? "Left " : "Right ") + "angle :" + Vector3.Angle(transform.parent.parent.right))
		transform.localPosition = localp;
		transform.rotation = Quaternion.Euler (rot.x, rot.y, rot.z);*/
	}
}
