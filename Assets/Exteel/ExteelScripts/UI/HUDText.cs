using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDText : MonoBehaviour {

	Vector3 originalWorldPos;
	Vector3 currentScreenPos;
	Camera cam;
	private float height = 0f;

	public void Set(Camera c, Vector3 orig) {
		cam = c;
		originalWorldPos = orig;
		transform.position = cam.WorldToScreenPoint(originalWorldPos);
	}

	// Update is called once per frame
	void Update () {
		if (cam == null) return;
		transform.position = cam.WorldToScreenPoint(originalWorldPos) + new Vector3(0, height, 0);
		height += 0.25f;
	}
}
