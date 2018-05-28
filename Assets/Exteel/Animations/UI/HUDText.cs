using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDText : MonoBehaviour {

    private Image img;
	private Vector3 originalWorldPos;
    private Vector3 currentScreenPos;
    private Camera cam;
	private float height = 0f;

    private void Start() {
        img = GetComponent<Image>();
    }
    
    public void Display(Camera cam) {
        this.cam = cam;
        transform.position = cam.WorldToScreenPoint(transform.root.position + new Vector3(0, 5, 0));
        transform.localScale = new Vector3(1,1,1);
        img.color = new Color(img.color.r, img.color.g, img.color.b, 255);
        img.enabled = true;
        enabled = true;
    }

	// Update is called once per frame
	void Update () {
		if (cam == null) return;

		//if(cam.WorldToScreenPoint (originalWorldPos).z<0)
		//	gameObject.SetActive (false);

        transform.localScale *= 0.98f;
        if(transform.localScale.x < 0.8f) { img.enabled = false; enabled = false; }
        img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a - 2);
        transform.position = cam.WorldToScreenPoint(transform.root.position + new Vector3(0,5,0));

		/*transform.position = cam.WorldToScreenPoint(originalWorldPos) + new Vector3(0, height, 0);
		height += 0.25f;*/
	}
}
