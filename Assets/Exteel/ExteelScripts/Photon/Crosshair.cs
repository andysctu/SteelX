using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour {

	public float CrosshairRadius = 15f;
	public float MaxDistance = 100f;
	public Sprite CrosshairNormal;
	public Sprite CrosshairTarget;

	private Camera camera;
	private Image currentCrosshair;
	public LayerMask layerMask = 8;

	private Transform target;
	public const float CAM_DISTANCE_TO_MECH = 20f;
	// Use this for initialization
	void Start () {
		camera = transform.GetComponent<Camera>();
		currentCrosshair = transform.GetComponentInChildren<Image>();
	}

	public void NoCrosshair() {
		currentCrosshair = null;
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		//Debug.DrawLine (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH) + camera.transform.forward * 20);

		if (Physics.SphereCast (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadius, camera.transform.forward, out hit, MaxDistance, layerMask)) {
			currentCrosshair.sprite = CrosshairTarget;
			target = hit.transform;

			//play Lock sound
		} else {
			currentCrosshair.sprite = CrosshairNormal;
			target = null;
		}
	}

	public Transform getCurrentTarget(){
		return target;
	}
}
