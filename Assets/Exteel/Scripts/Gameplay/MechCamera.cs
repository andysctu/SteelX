using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class MechCamera : MonoBehaviour
{
	[SerializeField]private GameObject currentMech;
	[SerializeField]private Animator animator;
	private Transform fakeTransform, player;
	private MechController mctrl;
	private Vector3 m_TargetAngles;
	private Vector3 m_FollowAngles;
	private Vector3 m_FollowVelocity;
	private Quaternion m_OriginalRotation, tempCurrentMechRot;
	private float inputH,inputV;
	private float idealLocalAngle = 0,orbitAngle = 0;//this is cam local angle
	private CharacterController parentCtrl;
	private float playerlerpspeed = 50f, orbitlerpspeed = 50f;
	private float upamount = 1;

	public Vector2 rotationRange = new Vector3(70, 70);
	public float rotationSpeed = 5;
	public float dampingTime = 0.2f;
	public bool lockPlayerRot = false;
	public float orbitRadius = 19, lerpFakePosSpeed = 12;
	public float angleOffset = 33;

	void Start()
	{
		parentCtrl = transform.parent.GetComponent<CharacterController>();
		m_OriginalRotation = transform.localRotation;
		orbitAngle = Vector3.SignedAngle (transform.parent.forward + transform.parent.up, transform.position - transform.parent.position - Vector3.up * 5, -transform.parent.right);
		mctrl = transform.root.GetComponent<MechController> ();
		player = transform.root;

		GameObject g = new GameObject ();
		g.name = "PlayerCam";
		g.transform.position = transform.parent.position;
		fakeTransform = g.transform;
		transform.SetParent (fakeTransform);
		transform.localPosition = Vector3.zero;
	}

	void Update(){
		inputH = CrossPlatformInputManager.GetAxis ("Mouse X");
		inputV = CrossPlatformInputManager.GetAxis ("Mouse Y");

		float scrollwheel;
		if ((scrollwheel = Input.GetAxis ("Mouse ScrollWheel")) > 0) {
			orbitRadius = Mathf.Clamp (--orbitRadius, 16, 23);
		}else if(scrollwheel < 0){
			orbitRadius = Mathf.Clamp (++orbitRadius, 16, 23);
		}

	}

	void FixedUpdate()
	{
		fakeTransform.position = Vector3.Lerp (fakeTransform.position, player.position + new Vector3(0,upamount,0), Time.fixedDeltaTime * lerpFakePosSpeed);

		transform.localRotation = m_OriginalRotation;

		if(lockPlayerRot){
			currentMech.transform.rotation = tempCurrentMechRot;
		}else{
			if(currentMech.transform.localRotation != Quaternion.identity){
				currentMech.transform.localRotation = Quaternion.Lerp (currentMech.transform.localRotation, Quaternion.identity, Time.fixedDeltaTime * playerlerpspeed);
			}
		}

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

		if(player!=null)
			player.rotation = Quaternion.Lerp (transform.parent.rotation, m_OriginalRotation * Quaternion.Euler (0, m_FollowAngles.y, 0), Time.fixedDeltaTime *playerlerpspeed);
		
		//lerp cam rotation
		//orbitAngle = Mathf.Lerp (orbitAngle, Mathf.Clamp (orbitAngle + inputV * rotationSpeed, 70, 220), Time.deltaTime * orbitlerpspeed);
		orbitAngle = Mathf.Clamp (orbitAngle + inputV * rotationSpeed, 70, 200);


		idealLocalAngle = -1.0322f * (orbitAngle - 119.64f);
		transform.localRotation = Quaternion.Euler(idealLocalAngle+angleOffset,0,0);
		transform.localPosition = new Vector3 (transform.localPosition.x, orbitRadius * Mathf.Sin (orbitAngle * Mathf.Deg2Rad), orbitRadius * Mathf.Cos (orbitAngle * Mathf.Deg2Rad));
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

	public void SetLerpFakePosSpeed(float n){
		lerpFakePosSpeed = n;
	}

	public void SetOrbitRadius(float r){
		orbitRadius = r;
	}
}
