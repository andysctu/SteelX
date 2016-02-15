using UnityEngine;
using System.Collections;

public class Lobby : MonoBehaviour {

	public GameObject Mech;

	// Use this for initialization
	void Start () {
		Mech.SetActive(true);
	}

	void OnGUI(){
		Debug.Log("Drawing");
		GUI.BeginGroup(new Rect(Screen.width/2-150,Screen.height/2-150,300,300));
		GUI.Box(new Rect(0,0,300,300),"This is the title of a box");
		GUI.Button(new Rect(0,25,100,20),"I am a button");
		GUI.Label (new Rect (0, 50, 100, 20), "I'm a Label!");
//		toggleTxt = GUI.Toggle(Rect(0, 75, 200, 30), toggleTxt, "I am a Toggle button");
//		toolbarInt = GUI.Toolbar (Rect (0, 110, 250, 25), toolbarInt, toolbarStrings);
//		selGridInt = GUI.SelectionGrid (Rect (0, 160, 200, 40), selGridInt, selStrings, 2);
//		hSliderValue = GUI.HorizontalSlider (Rect (0, 210, 100, 30), hSliderValue, 0.0, 1.0);
//		hSbarValue = GUI.HorizontalScrollbar (Rect (0, 230, 100, 30), hSbarValue, 1.0, 0.0, 10.0);
		GUI.EndGroup ();
	}
}
