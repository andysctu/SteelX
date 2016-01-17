using UnityEngine;
using System.Collections;

public class GameManagerReferences : MonoBehaviour {
	public GameObject RespawnButton;	

	private bool showboard = false;

	void Update() {
		showboard = Input.GetKey(KeyCode.Tab);
	}

	void OnGUI() {
		if (showboard) {
//			GUI.color = Color.blue;

			GUILayout.BeginArea(new Rect(Screen.width/4, Screen.height/4, Screen.width/2, Screen.height/2), "scoreboard");

			GUILayout.EndArea();
		}
	}
}
