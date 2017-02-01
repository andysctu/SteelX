using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

	[SerializeField] GameObject Placeholder;
	[SerializeField] Sprite Hit, Kill, Defense;

	private GameObject canvas;
	private bool showCursor;

	void Start() {
		canvas = GameObject.Find("Canvas");
		showCursor = false;
	}

	public void ShowText(Camera cam, Vector3 p, string Text) {
		GameObject i = Instantiate(Placeholder, cam.WorldToScreenPoint(p), Quaternion.identity) as GameObject;
		i.GetComponent<HUDText>().Set(cam, p);
		i.transform.SetParent(canvas.transform);
		Image im = i.GetComponent<Image>();

		switch (Text) {
		case "Hit": im.sprite = Hit; break;
		case "Kill": im.sprite = Kill; break;
		case "Defense": im.sprite = Defense; break;
		}

		im.preserveAspect = true;
		im.SetNativeSize();
		i.transform.localScale = new Vector3(1,1,1);
		Destroy(i, 0.5f);
	}

	public void ShowCursor() { showCursor = true; }

	void OnGUI() {
		Cursor.visible = showCursor;
		Cursor.lockState = CursorLockMode.Confined;
	}
}
