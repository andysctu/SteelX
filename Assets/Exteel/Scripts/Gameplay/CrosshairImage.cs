using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairImage : MonoBehaviour {

	private float radiusL;
	private float radiusR;

	[SerializeField]private GameObject[] crosshairs;// 4 : RCL's circle, 5 : middlecross , 6:RCL middlecross
	[SerializeField]private RectTransform crosshairsL0, crosshairsL1, crosshairsR0, crosshairsR1;
	public Image targetMark;

	private int radiusCoeff = 22;
	public bool noCrosshairL = false;
	public bool noCrosshairR = false;

	private bool isShaking = false;
	private Coroutine[] shakeCoroutine = new Coroutine[2];

	//shaking
	private float orgRadiusL, orgRadiusR;
	private float shakingAmount = 1.33f;

	void Update(){
		if(isShaking){
			SetLoffset (radiusL);
			SetRoffset (radiusR);

			radiusL = Mathf.Lerp (radiusL, orgRadiusL, 0.05f);
			radiusR = Mathf.Lerp (radiusR, orgRadiusR, 0.05f);
		}
	}

	public void SetRadius(float L, float R){
		radiusL = L * radiusCoeff;
		radiusR = R * radiusCoeff;

		orgRadiusL = radiusL;
		orgRadiusR = radiusR;

		SetRoffset (radiusR);
		SetLoffset (radiusL);
	}
	void SetLoffset(float radiusL){
		crosshairsL0.offsetMin = new Vector2 (-radiusL, -radiusL);
		crosshairsL0.offsetMax = new Vector2 (radiusL, radiusL);

		crosshairsL1.offsetMin = new Vector2 (-radiusL, -radiusL);
		crosshairsL1.offsetMax = new Vector2 (radiusL, radiusL);
	}
	void SetRoffset(float radiusR){
		crosshairsR0.offsetMin = new Vector2 (-radiusR, -radiusR);
		crosshairsR0.offsetMax = new Vector2 (radiusR, radiusR);

		crosshairsR1.offsetMin = new Vector2 (-radiusR, -radiusR);
		crosshairsR1.offsetMax = new Vector2 (radiusR, radiusR);
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

	public void ShakingEffect(int handPosition, float rate,  int bulletNum){
		isShaking = true;
		if (shakeCoroutine[handPosition] == null) {
			shakeCoroutine[handPosition] = StartCoroutine (Shaking (handPosition, rate, bulletNum));
		}else{//stop the current shaking & start a new one
			StopCoroutine (shakeCoroutine[handPosition]);
			shakeCoroutine[handPosition] = StartCoroutine (Shaking (handPosition, rate, bulletNum));
		}
	}

	IEnumerator Shaking(int handPosition, float rate,  int bulletNum){
		for(int i=0;i<bulletNum;i++){
			if(handPosition==0){
				radiusL = orgRadiusL*shakingAmount;
			}else{
				radiusR = orgRadiusR*shakingAmount;
			}
			yield return (bulletNum==1)? new WaitForSeconds (0.2f):new WaitForSeconds (1 / rate / bulletNum);
		}
		isShaking = false;

		radiusL = orgRadiusL;
		radiusR = orgRadiusR;
		SetLoffset (radiusL);
		SetRoffset (radiusR);
	}
}
