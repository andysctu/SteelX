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
		if (Physics.Raycast(playerSight.transform.position, playerSight.transform.forward, out hit, Mathf.Infinity, layerMask)){
			currentCrosshair.sprite = CrosshairTarget;
		} else {
			currentCrosshair.sprite = CrosshairNormal;
		}
	}
}
