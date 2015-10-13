using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Utility
{
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
		public float rotationSpeed = 10;
		public float dampingTime = 0.2f;
		public bool autoZeroVerticalOnMobile = true;
		public bool autoZeroHorizontalOnMobile = false;
		public bool relative = true;

		public float mouseYSensitivity = 5f;
		
		private Vector3 m_TargetAngles;
		private Vector3 m_FollowAngles;
		private Vector3 m_FollowVelocity;
		private Quaternion m_OriginalRotation;

		private float mouseY = 0f;
		private float behind;

		private void Start()
		{
			m_OriginalRotation = transform.localRotation;
			behind = Vector3.Distance(transform.position, transform.parent.position);
		}
		
		
		private void Update()
		{
			// we make initial calculations from the original local rotation
			transform.localRotation = m_OriginalRotation;
			
			// read input from mouse or mobile controls
			float inputH;
			float inputV;
			if (relative) {
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
				
				#if MOBILE_INPUT
				// on mobile, sometimes we want input mapped directly to tilt value,
				// so it springs back automatically when the look input is released.
				if (autoZeroHorizontalOnMobile) {
					m_TargetAngles.y = Mathf.Lerp (-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH * .5f + .5f);
				} else {
					m_TargetAngles.y += inputH * rotationSpeed;
				}
				if (autoZeroVerticalOnMobile) {
					m_TargetAngles.x = Mathf.Lerp (-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV * .5f + .5f);
				} else {
					m_TargetAngles.x += inputV * rotationSpeed;
				}
				#else
				// with mouse input, we have direct control with no springback required.
				m_TargetAngles.y += inputH * rotationSpeed;
				m_TargetAngles.x += inputV * rotationSpeed;
				#endif
				
				// clamp vertical, let 360 horizontal
				m_TargetAngles.x = Mathf.Clamp (m_TargetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);

//				while (m_TargetAngles.y > 360) m_TargetAngles.y -= 360;
//				while (m_TargetAngles.y < -360) m_TargetAngles.y += 360;
			} else {
				inputH = Input.mousePosition.x;
				inputV = Input.mousePosition.y;
				
				// set values to allowed range
				m_TargetAngles.y = Mathf.Lerp (-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH / Screen.width);
				m_TargetAngles.x = Mathf.Lerp (-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV / Screen.height);
			}
			
			// smoothly interpolate current values to target angles
			//m_FollowAngles = Vector3.SmoothDamp(m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);
			
			// update the actual gameobject's rotation
			//transform.localRotation = m_OriginalRotation*Quaternion.Euler(-m_FollowAngles.x, m_FollowAngles.y, 0);


//			float xOffset = (float) (distanceBehind * Math.Sin (m_FollowAngles.y) / Math.Sin (90 - m_FollowAngles.y / 2));
//			transform.localPosition.Set(transform.position.x + xOffset, transform.position.y, transform.position.z);
//
//			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
//			RaycastHit hit;
//			Vector3 lookTarget = Vector3.;
//			if (Physics.Raycast (ray, out hit)) {
//				lookTarget = hit.point;
//			}
//
//			transform.LookAt (lookTarget);
			//transform.rotation = m_OriginalRotation * Quaternion.Euler (m_TargetAngles.x, 0, 0);
			//Debug.DrawLine
			transform.parent.rotation = m_OriginalRotation * Quaternion.Euler (0, m_TargetAngles.y, 0);
			transform.localRotation = m_OriginalRotation * Quaternion.Euler (-m_TargetAngles.x, 0, 0);

			float outerRotate = ( - inputV) * rotationSpeed;
			transform.RotateAround(transform.parent.position + transform.parent.up * 3, transform.parent.right, outerRotate);

//			float clampedRotation = transform.rotation.x;
////			if (clampedRotation > 0.36) {
////				clampedRotation -= 360;
////			}
//			
//			if (clampedRotation > 0.36f) {
//				clampedRotation = 0.36f;
//			}
//			
//			if (clampedRotation < -0.4193f) {
//				clampedRotation = -0.4193f;
//			}
//
//			Debug.Log ("clamped: " + clampedRotation);
//			Debug.Log ("transform.rotation.x: " + transform.rotation.x) ;

			// TODO clamp
//			Vector3 euler = transform.eulerAngles;
//			Debug.Log ("euler.x: " + euler.x);
//	
//			float eulerAngle = euler.x;
//			if (eulerAngle > 180){
//				eulerAngle -= 360;
//			}
//			
//			if (eulerAngle > 60) {
//				eulerAngle = 60f;
//			}
//			
//			if (eulerAngle < -60) {
//				eulerAngle = -60f;
//			}
//
//			euler.x = eulerAngle;
//			transform.eulerAngles = euler;




//			transform.Rotate(eulerAngle, transform.rotation.y, transform.rotation.z);

//			Vector3 direction = new Vector3 (0, 0, transform.position.magnitude-transform.parent.position.magnitude);
////			Quaternion rotation = Quaternion.Euler (inputH, inputV, 0);
////			Vector3 desiredPosition = transform.parent.position + rotation * direction;
//
//			transform.position = new Vector2 (inputV, inputV);

			//Vector3 vertLine = new Vector3 (0, transform.parent.GetComponent<CharacterController> ().center.y, 0);
			//Vector3 horizLine = new Vector3(transform.parent.GetComponent<CharacterController> ().center.x, (float)(transform.parent.GetComponent<CharacterController> ().height * 0.75), 0);
			//Debug.DrawLine (vertLine, vertLine + Vector3.up * 5);
			//Debug.DrawLine (horizLine, horizLine + Vector3.left * 5);

			//transform.rotation = Quaternion.AngleAxis (-m_FollowAngles.y, vertLine);

//			CharacterController charCtrler = transform.parent.GetComponent<CharacterController> ();
//			transform.parent.LookAt (Camera.main.ScreenToWorldPoint (Input.mousePosition));
		}
	}
}
