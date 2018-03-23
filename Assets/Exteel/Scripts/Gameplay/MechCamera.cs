using UnityEngine;
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
	public float dampingTime = 0.2f;
	
	private Vector3 m_TargetAngles;
	private Vector3 m_FollowAngles;
	private Vector3 m_FollowVelocity;
	private Quaternion m_OriginalRotation;	

	private CharacterController parentCtrl;

	private void Start()
	{
		parentCtrl = transform.parent.GetComponent<CharacterController>();
		m_OriginalRotation = transform.localRotation;
	}
	
	
	private void Update()
	{
		// we make initial calculations from the original local rotation
		transform.localRotation = m_OriginalRotation;
		
		// read input from mouse or mobile controls
		float inputH;
		float inputV;
	
		inputH = CrossPlatformInputManager.GetAxis ("Mouse X");
		inputV = CrossPlatformInputManager.GetAxis ("Mouse Y");
		
		// wrap values to avoid springing quickly the wrong way from positive to negative
		if (m_TargetAngles.y > 180) {
			m_TargetAngles.y -= 360;
			m_FollowAngles.y -= 360;
		}
		if (m_TargetAngles.x > 180) {
			m_TargetAngles.x -= 360;
			m_FollowAngles.x -= 360;
		}
		if (m_TargetAngles.y < -180) {
			m_TargetAngles.y += 360;
			m_FollowAngles.y += 360;
		}
		if (m_TargetAngles.x < -180) {
			m_TargetAngles.x += 360;
			m_FollowAngles.x += 360;
		}

		// with mouse input, we have direct control with no springback required.
		m_TargetAngles.y += inputH * rotationSpeed;
		m_TargetAngles.x += inputV * rotationSpeed;
		
		// clamp vertical, let 360 horizontal
		// m_TargetAngles.x = Mathf.Clamp (m_TargetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);
	
		
		// smoothly interpolate current values to target angles
//		m_FollowAngles = Vector3.SmoothDamp(m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);
		m_FollowAngles = m_TargetAngles;


		float outerRotate = ( - inputV) * rotationSpeed;
		transform.RotateAround(transform.parent.position + transform.parent.up * 5, transform.parent.right, outerRotate);
		//transform.RotateAround(transform.parent.position , transform.parent.right, outerRotate);

		transform.parent.rotation = m_OriginalRotation * Quaternion.Euler (0, m_FollowAngles.y, 0);
		transform.localRotation = m_OriginalRotation * Quaternion.Euler (-m_FollowAngles.x, 0, 0);

		Vector3 rot = transform.parent.eulerAngles;
		rot.z = 0;
		transform.parent.eulerAngles = rot;
	}

	public float GetFollowAngle_x(){
		return m_FollowAngles.x;
	}
}
