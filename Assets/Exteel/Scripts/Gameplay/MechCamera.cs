﻿using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class MechCamera : MonoBehaviour
{
	// A mouselook behaviour with constraints which operate relative to
	// this gameobject's initial rotation.
	// Only rotates around local X and Y.
	// Works in local coordinates, so if this object is parented
	// to another moving gameobject, its local constraints will
	// operate correctly
	// (Think: looking out the side window of a car, or a gun turret
	// on a moving spaceship with a limited angular range)
	// to have no constraints on an axis, set the rotationRange to 360 or greater.
	public Vector2 rotationRange = new Vector3(70, 70);
	public float rotationSpeed = 5;
	public float dampingTime = 0.2f, lerpYSpeed_g = 15f, lerpYSpeed_a = 8, lerpYSpeed, lerpcoeff = 10;
	public bool lockPlayerRot = false, lockCamRot = false;

	private Vector3 m_TargetAngles;
	private Vector3 m_FollowAngles;
	private Vector3 m_FollowVelocity;
	private Quaternion m_OriginalRotation, tempCurrentMechRot, tempCamRot;	
	[SerializeField]private GameObject currentMech;
	private MechController mctrl;
	float inputH;
	float inputV;
	float orbitAngle = 0;
	float idealLocalAngle = 0;//this is cam local angle
	private CharacterController parentCtrl;
	public float orbitRadius = 19;
	public float angleOffset = 33;

	private float playerlerpspeed = 50f, orbitlerpspeed = 50f;

	float curYpos;

	private void Start()
	{
		parentCtrl = transform.parent.GetComponent<CharacterController>();
		m_OriginalRotation = transform.localRotation;
		orbitAngle = Vector3.SignedAngle (transform.parent.forward + transform.parent.up, transform.position - transform.parent.position - Vector3.up * 5, -transform.parent.right);
		curYpos = transform.position.y;
		mctrl = transform.root.GetComponent<MechController> ();
	}

	void Update(){
		inputH = CrossPlatformInputManager.GetAxis ("Mouse X");
		inputV = CrossPlatformInputManager.GetAxis ("Mouse Y");
	}

	void FixedUpdate()
	{
		// we make initial calculations from the original local rotation
		transform.localRotation = m_OriginalRotation;

		if(lockPlayerRot){
			currentMech.transform.rotation = tempCurrentMechRot;
		}else{
			if(currentMech.transform.localRotation != Quaternion.identity){
				currentMech.transform.localRotation = Quaternion.Lerp (currentMech.transform.localRotation, Quaternion.identity, Time.fixedDeltaTime * playerlerpspeed);
			}
		}
		// read input from mouse or mobile controls
		/*inputH = CrossPlatformInputManager.GetAxis ("Mouse X");
		inputV = CrossPlatformInputManager.GetAxis ("Mouse Y");*/

		// with mouse input, we have direct control with no springback required.
		m_TargetAngles.y += inputH * rotationSpeed;

		// wrap values to avoid springing quickly the wrong way from positive to negative
		if (m_TargetAngles.y > 180) {
			m_TargetAngles.y = (m_TargetAngles.y%360) - 360;
		}
		if (m_TargetAngles.y < -180) {
			m_TargetAngles.y = (m_TargetAngles.y%360) + 360;
		}
			
		m_FollowAngles = m_TargetAngles;

		//lerp parent rotation
		transform.parent.rotation = Quaternion.Lerp (transform.parent.rotation, m_OriginalRotation * Quaternion.Euler (0, m_FollowAngles.y, 0), Time.fixedDeltaTime *playerlerpspeed);
		//transform.parent.rotation = m_OriginalRotation * Quaternion.Euler (0, m_FollowAngles.y, 0);

		//lerp cam rotation
		//orbitAngle = Mathf.Lerp (orbitAngle, Mathf.Clamp (orbitAngle + inputV * rotationSpeed, 10, 220), Time.deltaTime * orbitlerpspeed);
		orbitAngle = Mathf.Clamp (orbitAngle + inputV * rotationSpeed, 70, 200);


		idealLocalAngle = -1.0322f * (orbitAngle - 119.64f);
		transform.localRotation = Quaternion.Euler(idealLocalAngle+angleOffset,0,0);
		transform.localPosition = new Vector3 (transform.localPosition.x, orbitRadius * Mathf.Sin (orbitAngle * Mathf.Deg2Rad), orbitRadius * Mathf.Cos (orbitAngle * Mathf.Deg2Rad));

		if (!mctrl.grounded) {
			lerpYSpeed = lerpYSpeed_a;
			curYpos = Mathf.Lerp (curYpos, transform.position.y, Time.fixedDeltaTime * lerpYSpeed);
			transform.position = new Vector3 (transform.position.x, curYpos, transform.position.z);

		}else{
			lerpYSpeed = Mathf.Lerp (lerpYSpeed, lerpYSpeed_g, Time.fixedDeltaTime * lerpcoeff);
			curYpos = Mathf.Lerp (curYpos, transform.position.y, Time.fixedDeltaTime * lerpYSpeed);
			transform.position = new Vector3 (transform.position.x, curYpos, transform.position.z);
		}
	}

	public float GetCamAngle(){
		return ((transform.rotation.eulerAngles.x > 180) ? -(transform.rotation.eulerAngles.x - 360) : -transform.rotation.eulerAngles.x);
	}

	public void LockMechRotation(bool b){//cam not included
		if(b){
			if (lockPlayerRot)return;//already locked
			tempCurrentMechRot = currentMech.transform.rotation;
		}
		lockPlayerRot = b;
	}

	public void LockCamRotation(bool b){
		lockCamRot = b;
		tempCamRot = transform.rotation;
	}
}
