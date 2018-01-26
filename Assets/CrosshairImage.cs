using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairImage : MonoBehaviour {

	private float radius;

	[SerializeField]
	private GameObject[] crosshairs;

	[SerializeField]
	private RectTransform[] crosshairs0;
	[SerializeField]
	private RectTransform[] crosshairs1;

	public bool noCrosshair = false;

	public void SetRadius(float setRadius){
		radius = setRadius * 10f;

		crosshairs0 [0].offsetMin = new Vector2 (-radius, radius);
		crosshairs0 [0].offsetMax = new Vector2 (-radius, radius);

		crosshairs0 [1].offsetMin = new Vector2 (radius, radius);
		crosshairs0 [1].offsetMax = new Vector2 (radius, radius);

		crosshairs0 [2].offsetMin = new Vector2 (-radius, -radius);
		crosshairs0 [2].offsetMax = new Vector2 (-radius, -radius);

		crosshairs0 [3].offsetMin = new Vector2 (radius, -radius);
		crosshairs0 [3].offsetMax = new Vector2 (radius, -radius);

		crosshairs1 [0].offsetMin = new Vector2 (-radius, radius);
		crosshairs1 [0].offsetMax = new Vector2 (-radius, radius);

		crosshairs1 [1].offsetMin = new Vector2 (radius, radius);
		crosshairs1 [1].offsetMax = new Vector2 (radius, radius);

		crosshairs1 [2].offsetMin = new Vector2 (-radius, -radius);
		crosshairs1 [2].offsetMax = new Vector2 (-radius, -radius);

		crosshairs1 [3].offsetMin = new Vector2 (radius, -radius);
		crosshairs1 [3].offsetMax = new Vector2 (radius, -radius);

	}

	public void SetCurrentImage(int CurImage){
		if(CurImage == 1){
			crosshairs [0].SetActive (false);
			crosshairs [1].SetActive (true);
		}else if(CurImage == 0){
			crosshairs [0].SetActive (true);
			crosshairs [1].SetActive (false);
		}
	}
}
