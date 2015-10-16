using UnityEngine;
using System.Collections;

public class MechMotor : MonoBehaviour {

	public static MechMotor Instance;
	public float MoveSpeed = 10f;
	public Vector3 MoveVector { get; set; }
	// Use this for initialization
	void Awake () {
		Instance = this;
	}
	
	// Update is called once per frame
	public void UpdateMotor () {
//		SnapMechToCamera ();
		ProcessMotion ();
	}

	void ProcessMotion(){
		//MoveVector = transform.forward*MoveVector.z + transform.right*MoveVector.x;

		//Debug.DrawLine (transform.position, transform.forward * 5);
		MoveVector = transform.TransformDirection(MoveVector);

		if (MoveVector.magnitude > 1) {
			MoveVector = Vector3.Normalize (MoveVector);
		}

		MoveVector *= MoveSpeed;

		MoveVector *= Time.deltaTime;

		MechController.CharacterController.Move (MoveVector);

	}

	void SnapMechToCamera(){
//		if (MoveVector.x != 0 || MoveVector.z != 0) {
//			transform.rotation = Quaternion.Euler (transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, transform.eulerAngles.z);
//		}

		//transform.rotation = Quaternion.Euler (Input.GetAxis("Mouse X")
	}
}
