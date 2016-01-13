using UnityEngine;
using System.Collections;

public class ShootAnimation : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			Debug.Log("H");
			transform.Rotate(new Vector3(90,90,90));
		}
	}
}
