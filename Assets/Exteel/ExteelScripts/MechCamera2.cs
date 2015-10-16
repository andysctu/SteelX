using UnityEngine;
using System.Collections;

public class MechCamera2 : MonoBehaviour {

	public static MechCamera2 Instance;
	public Transform TargetLookAt;
	public float CameraDistance = 5f;
	public float CameraMinDistance = 3f;
	public float CameraMaxDistance = 10f;
	public float CameraSmooth = 0.05f;

	public float mouseXSensitivity = 5f;
	public float mouseYSensitivity = 5f;
	public float mouseWheelSensitivity = 5f;

	public float XSmooth = 0.05f;
	public float YSmooth = 0.1f;

	public float minVertRotation = -40f;
	public float maxVertRotation = 80f;

	private float mouseX = 0f;
	private float mouseY = 0f;

	private float velX = 0f;
	private float velY = 0f;
	private float velZ = 0f;
	private Vector3 pos = Vector3.zero;

	private float velDistance = 0f;
	private float initialCameraDistance = 0f;
	private Vector3 desiredCameraPosition = Vector3.zero;
	private float desiredCameraDistance = 0f;
	
	// Use this for initialization
	void Awake () {
		Instance = this;
	}

	void Start(){
		CameraDistance = Mathf.Clamp (CameraDistance, CameraMinDistance, CameraMaxDistance);
		initialCameraDistance = CameraDistance;
		Reset ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate(){
		if (TargetLookAt == null) {
			return;
		}

		HandlePlayerInput ();
		CalculateDesiredPosition ();
		UpdatePosition ();
	}

	void HandlePlayerInput(){
		var marginOfError = 0.01f;

		mouseX += Input.GetAxis("Mouse X") * mouseXSensitivity;
		mouseY -= Input.GetAxis("Mouse Y") * mouseYSensitivity;


		// Limit moouseY
		mouseY = Helper.ClampAngle (mouseY, minVertRotation, maxVertRotation);

		if (Input.GetAxis ("Mouse ScrollWheel") > marginOfError || Input.GetAxis ("Mouse ScrollWheel") < -marginOfError) {
			desiredCameraDistance = Mathf.Clamp (CameraDistance - Input.GetAxis ("Mouse ScrollWheel") * mouseWheelSensitivity, CameraMinDistance, CameraMaxDistance);
		}
	}

	void Reset(){
		mouseX = 0;
		mouseY = 10;
		CameraDistance = initialCameraDistance;
		desiredCameraDistance = initialCameraDistance; 
	}

	void CalculateDesiredPosition(){
		CameraDistance = Mathf.SmoothDamp (CameraDistance, desiredCameraDistance, ref velDistance, CameraSmooth);
		desiredCameraPosition = CalculatePosition (mouseY, mouseX, CameraDistance);
	}

	void UpdatePosition(){
		var posX = Mathf.SmoothDamp (pos.x, desiredCameraPosition.x, ref velX, XSmooth);
		var posY = Mathf.SmoothDamp (pos.y, desiredCameraPosition.y, ref velX, YSmooth);
		var posZ = Mathf.SmoothDamp (pos.z, desiredCameraPosition.z, ref velX, XSmooth);
		pos = new  Vector3 (posX, posY, posZ);

		transform.position = pos;
		transform.LookAt (TargetLookAt);
	}

	Vector3 CalculatePosition(float rotationX, float rotationY, float distance) {
		Vector3 direction = new Vector3 (0, 0, -distance);
		Quaternion rotation = Quaternion.Euler (rotationX, rotationY, 0);
		return TargetLookAt.position + rotation * direction;
	}

	public static void UseMainCamera(){
		GameObject tempCamera;
		GameObject targetLookAt;
		MechCamera2 myCamera; 

		if (Camera.main != null) {
			tempCamera = Camera.main.gameObject;
		} else {
			tempCamera = new GameObject ("Main Camera");
			tempCamera.AddComponent<Camera>();
			tempCamera.tag = "Main Camera";
		}
		 
		tempCamera.AddComponent <MechCamera2>();
		myCamera = tempCamera.GetComponent ("MechCamera2") as MechCamera2;

		targetLookAt = GameObject.Find ("targetLookAt") as GameObject;

		if (targetLookAt == null) {
			targetLookAt = new GameObject ("targetLookAt");
			targetLookAt.transform.position = new Vector3(0,5,0);
		}

		myCamera.TargetLookAt = targetLookAt.transform; 
	}
}
