using UnityEngine;
using System.Collections;

public class Lobby : MonoBehaviour {

	public GameObject Mech;

	// Use this for initialization
	void Start () {
		Debug.Log("H");
		Mech.SetActive(true);
	}

	void Awake() {
		Debug.Log("Hi");
		Mech.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
