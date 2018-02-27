using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairImage : MonoBehaviour {

	private float radiusL;
	private float radiusR;

	[SerializeField]
	private GameObject[] crosshairs;// 4 : RCL's circle, 5 : middlecross , 6:RCL middlecross

	[SerializeField]
	private RectTransform[] crosshairsL0;
	[SerializeField]
	private RectTransform[] crosshairsL1;
	[SerializeField]
	private RectTransform[] crosshairsR0;
	[SerializeField]
	private RectTransform[] crosshairsR1;


	public bool noCrosshairL = false;
	public bool noCrosshairR = false;

	public void SetRadius(float setRadiusL, float setRadiusR){
		radiusL = setRadiusL * 25f;
		radiusR = setRadiusR * 25f;


		SetR1offset (radiusR);
		SetL1offset (radiusL);

	}
	void SetL1offset(float radiusL){
		crosshairsL0 [0].offsetMin = new Vector2 (-radiusL, radiusL);
		crosshairsL0 [0].offsetMax = new Vector2 (-radiusL, radiusL);

		crosshairsL0 [1].offsetMin = new Vector2 (radiusL, radiusL);
		crosshairsL0 [1].offsetMax = new Vector2 (radiusL, radiusL);

		crosshairsL0 [2].offsetMin = new Vector2 (-radiusL, -radiusL);
		crosshairsL0 [2].offsetMax = new Vector2 (-radiusL, -radiusL);

		crosshairsL0 [3].offsetMin = new Vector2 (radiusL, -radiusL);
		crosshairsL0 [3].offsetMax = new Vector2 (radiusL, -radiusL);

		crosshairsL1 [0].offsetMin = new Vector2 (-radiusL, radiusL);
		crosshairsL1 [0].offsetMax = new Vector2 (-radiusL, radiusL);

		crosshairsL1 [1].offsetMin = new Vector2 (radiusL, radiusL);
		crosshairsL1 [1].offsetMax = new Vector2 (radiusL, radiusL);

		crosshairsL1 [2].offsetMin = new Vector2 (-radiusL, -radiusL);
		crosshairsL1 [2].offsetMax = new Vector2 (-radiusL, -radiusL);

		crosshairsL1 [3].offsetMin = new Vector2 (radiusL, -radiusL);
		crosshairsL1 [3].offsetMax = new Vector2 (radiusL, -radiusL);
	}
	void SetR1offset(float radiusR){
		//R
		crosshairsR0 [0].offsetMin = new Vector2 (-radiusR, radiusR);
		crosshairsR0 [0].offsetMax = new Vector2 (-radiusR, radiusR);

		crosshairsR0 [1].offsetMin = new Vector2 (radiusR, radiusR);
		crosshairsR0 [1].offsetMax = new Vector2 (radiusR, radiusR);

		crosshairsR0 [2].offsetMin = new Vector2 (-radiusR, -radiusR);
		crosshairsR0 [2].offsetMax = new Vector2 (-radiusR, -radiusR);

		crosshairsR0 [3].offsetMin = new Vector2 (radiusR, -radiusR);
		crosshairsR0 [3].offsetMax = new Vector2 (radiusR, -radiusR);

		crosshairsR1 [0].offsetMin = new Vector2 (-radiusR, radiusR);
		crosshairsR1 [0].offsetMax = new Vector2 (-radiusR, radiusR);

		crosshairsR1 [1].offsetMin = new Vector2 (radiusR, radiusR);
		crosshairsR1 [1].offsetMax = new Vector2 (radiusR, radiusR);

		crosshairsR1 [2].offsetMin = new Vector2 (-radiusR, -radiusR);
		crosshairsR1 [2].offsetMax = new Vector2 (-radiusR, -radiusR);

		crosshairsR1 [3].offsetMin = new Vector2 (radiusR, -radiusR);
		crosshairsR1 [3].offsetMax = new Vector2 (radiusR, -radiusR);
	}

	public void SetCurrentLImage(int CurImage){
		if(CurImage == 1){
			crosshairs [0].SetActive (false);
			crosshairs [1].SetActive (true);
		}else if(CurImage == 0){
			crosshairs [0].SetActive (true);
			crosshairs [1].SetActive (false);
		}
		crosshairs [5].SetActive (true);

		crosshairs [4].SetActive (false);
		crosshairs [6].SetActive (false);
	}
	public void SetCurrentRImage(int CurImage){
		if(CurImage == 1){
			crosshairs [2].SetActive (false);
			crosshairs [3].SetActive (true);
		}else if(CurImage == 0){
			crosshairs [2].SetActive (true);
			crosshairs [3].SetActive (false);
		}
		crosshairs [5].SetActive (true);

		crosshairs [4].SetActive (false);
		crosshairs [6].SetActive (false);
	}
	public void NoCrosshairL(){
		crosshairs [0].SetActive (false);
		crosshairs [1].SetActive (false);

		crosshairs [4].SetActive (false);
		crosshairs [5].SetActive (true);
		crosshairs [6].SetActive (false);
	}
	public void NoCrosshairR(){
		crosshairs [2].SetActive (false);
		crosshairs [3].SetActive (false);

		crosshairs [4].SetActive (false);
		crosshairs [5].SetActive (true);
		crosshairs [6].SetActive (false);
	}
	public void RCLcrosshair(){  // middlecross turns off only when RCL is on  
		crosshairs [4].SetActive (true);
		crosshairs [5].SetActive (false);
		crosshairs [6].SetActive (true);
	}
}
