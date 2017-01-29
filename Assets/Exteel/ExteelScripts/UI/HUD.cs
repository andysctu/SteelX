using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

	[SerializeField] GameObject Placeholder;
	[SerializeField] Sprite Hit, Kill, Defense;

	private GameObject canvas;

	void Start() {
		canvas = GameObject.Find("Canvas");
	}

	public void ShowHit(Camera cam, Vector3 p) {
		Debug.Log("Showing hit");
		GameObject i = Instantiate(Placeholder, cam.WorldToScreenPoint(p), Quaternion.identity) as GameObject;
		i.GetComponent<HUDText>().Set(cam, p);
		i.transform.SetParent(canvas.transform);
		Image im = i.GetComponent<Image>();
		im.sprite = Hit;
		im.preserveAspect = true;
		im.SetNativeSize();
		i.transform.localScale = new Vector3(1,1,1);
		Destroy(i, 0.5f);
	}

	public void ShowKill(Vector3 p) {
	}

	public void ShowDefense(Vector3 p) {
	}
}
