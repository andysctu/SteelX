using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NameTags : MonoBehaviour {

	GameManager gm;
	GameObject drone;
	Camera cam;

	void Start () 
    {	
		// Try to find the drone
		drone = GameObject.Find ("Drone");
		if (drone == null) {
			// if we can't find it, it probably did not load yet, we will find it later
			Debug.Log ("Drone is null");
		}
		cam = GameObject.Find("Camera").GetComponent<Camera>();
	}

	// OnGUI runs every frame, like Update, but just for GUI stuff like labels and scoreboard, etc
	void OnGUI() {
		// If we didn't find the drone yet, try to find it
		if (drone == null) {
			drone = GameObject.Find ("Drone");
		} else {
			// Once we've found it, transform the drone's 3D position to a 2D position corresponding to its position on your 2D screen
			Vector3 pos = cam.WorldToScreenPoint (drone.transform.position + new Vector3(0, 10));

			// Draw its name there
			GUI.Label (new Rect (pos.x, Screen.height - pos.y, 100, 100), "Drone");
		}
	}
 
}

