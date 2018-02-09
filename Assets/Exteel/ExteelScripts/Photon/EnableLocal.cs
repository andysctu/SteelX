using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EnableLocal : MonoBehaviour {

	// Use this for initialization
	void Start () {

		if (!GetComponent<PhotonView> ().isMine)
			return;
		// Enable mech controller
		GetComponent<MechController>().enabled = true;

		// Enable camera/radar
		foreach (Camera c in GetComponentsInChildren<Camera>()){
				c.enabled = true;
		}

		GetComponentInChildren<MechCamera>().enabled = true;
		GetComponentInChildren<AudioListener> ().enabled = true;
//		GetComponent<NameTags>().enabled = true;
//		GetComponentInChildren<AudioListener>().enabled = true;

		// Enable crosshair
		GetComponentInChildren<Crosshair>().enabled = true;

		
		GameObject crossHairImage = transform.Find("Camera/Canvas/CrosshairImage").gameObject;
		crossHairImage.SetActive(true);
		GameObject HeatBar = transform.Find("Camera/Canvas/HeatBar").gameObject;
		HeatBar.SetActive (true);
		// Disable your own cube
//		transform.Find("Cube").gameObject.SetActive(false);
	}
}
