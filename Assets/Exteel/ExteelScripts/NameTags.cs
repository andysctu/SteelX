using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NameTags : MonoBehaviour {

	GameManager gm;
	GameObject drone;
	Camera cam;

	void Start () 
    {
		drone = GameObject.Find ("Drone");
		if (drone == null) {
			Debug.Log ("Drone is null");
		}
		cam = GameObject.Find("Camera").GetComponent<Camera>();
	}

	void OnGUI() {
		if (drone == null) {
			drone = GameObject.Find ("Drone");
		} else {
			Vector3 pos = cam.WorldToScreenPoint (drone.transform.position + new Vector3(0, 10));
			GUI.Label (new Rect (pos.x, Screen.height - pos.y, 100, 100), "Drone");
		}
	}
 
}

