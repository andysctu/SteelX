using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NameTags : MonoBehaviour {

	GameManager gm;
	GameObject drone;
	Camera cam;
	// Use this for initialization
	void Start () 
    {
//        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		drone = GameObject.Find ("Drone");
		if (drone == null) {
			Debug.Log ("Drone is null");
		}
//		foreach (KeyValuePair<GameObject, Data> entry in gm.playerInfo)
//		{
////			aList.Add (GetComponent<GUIText> ().text = entry.Key.name);
//		}
		cam = GameObject.Find("Camera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update ()
    {
//        List<GameObject> aList = new List<GameObject> ();


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

