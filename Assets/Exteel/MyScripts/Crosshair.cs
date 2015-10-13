using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour {

	public Camera playerSight;
	public Sprite CrosshairNormal;
	public Sprite CrosshairTarget;

	private Image currentCrosshair;
	private LayerMask layerMask = 1 << 8;

	// Use this for initialization
	void Start () {
		if (playerSight == null) {
			playerSight = Camera.main;
		}
		currentCrosshair = GetComponent<Image> ();
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine (playerSight.transform.position, playerSight.transform.forward * 3);
		RaycastHit hit;
		if (Physics.Raycast(playerSight.transform.position, playerSight.transform.forward, out hit, Mathf.Infinity, layerMask)){
			currentCrosshair.sprite = CrosshairTarget;
		} else {
			currentCrosshair.sprite = CrosshairNormal;
		}
	}
}
