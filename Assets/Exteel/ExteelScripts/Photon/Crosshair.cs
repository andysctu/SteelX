using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour {
	
	public Sprite CrosshairNormal;
	public Sprite CrosshairTarget;

	private Camera playerSight;
	private Image currentCrosshair;
	private LayerMask layerMask = (1 << 8);
	private RaycastHit hit;

	public const float CAM_DISTANCE_TO_MECH = 19f;
	// Use this for initialization
	void Start () {
		playerSight = GetComponent<Camera>();
		currentCrosshair = GetComponentInChildren<Image>();
	}

	public void NoCrosshair() {
		currentCrosshair = null;
	}
	
	// Update is called once per frame
	void Update () {
//		Debug.Log("ps: " + playerSight.transform.position.x + ", " + playerSight.transform.position.y + ", " + playerSight.transform.position.z);
		Debug.DrawLine(playerSight.transform.TransformPoint(0, 0, CAM_DISTANCE_TO_MECH), playerSight.transform.TransformPoint(0, 0, CAM_DISTANCE_TO_MECH) + playerSight.transform.forward * 20);
		if (Physics.Raycast(playerSight.transform.TransformPoint(0, 0, CAM_DISTANCE_TO_MECH), playerSight.transform.forward, out hit, Mathf.Infinity, layerMask)){
			currentCrosshair.sprite = CrosshairTarget;
		} else {
			currentCrosshair.sprite = CrosshairNormal;
		}
	}
}
