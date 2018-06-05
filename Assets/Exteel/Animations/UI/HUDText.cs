using UnityEngine;
using UnityEngine.UI;

public class HUDText : MonoBehaviour {
    private Image img;
    private Camera cam;
    private float alpha_decrease_speed = 1, alpha_threshold = 0.75f, scale_decrease_speed = 1f;
    private Vector3 Mech_Mid_Point = new Vector3(0, 5, 0);

    private void Start() {
        img = GetComponent<Image>();
    }
    
    public void Display(Camera cam) {
        this.cam = cam;
        transform.position = cam.WorldToScreenPoint(transform.root.position + Mech_Mid_Point);
        transform.localScale = new Vector3(1.2f, 1.2f, 1);

        img.SetNativeSize();

        img.color = new Color(img.color.r, img.color.g, img.color.b, 1);


        img.enabled = (transform.position.z > 0);
        enabled = (transform.position.z > 0);
    }

	void Update () {
		if (cam == null) return;

        img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a - Time.deltaTime * alpha_decrease_speed);

        transform.position = cam.WorldToScreenPoint(transform.root.position + Mech_Mid_Point);
        transform.localScale -= transform.localScale * Time.deltaTime * scale_decrease_speed;

       if(transform.position.z < 0) {
            img.enabled = false;
            enabled = false;
        }

        if (img.color.a < alpha_threshold) {
            img.enabled = false;
            enabled = false;
        }
    }
}
