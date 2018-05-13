using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Ctype{// order of crosshairs in crosshairImage gameObject
	N_L0, N_L1, N_R0, N_R1, RCL_0, RCL_1, ENG   // N: normal , L0 : BLUE , L1 : RED 
}

public class CrosshairImage : MonoBehaviour {

	private float radiusL;
	private float radiusR;
	[SerializeField]private GameObject[] crosshairs;
	[SerializeField]private RectTransform crosshairsL0, crosshairsL1, crosshairsR0, crosshairsR1;
	public Image targetMark, EngTargetMark, middlecross;//pos control by Crosshair.cs

	private int radiusCoeff = 22;
	private int curImageL, curImageR;

	private bool isShakingL = false, isShakingR = false;
	private Coroutine[] shakeCoroutine = new Coroutine[2];

	//shaking
	private float orgRadiusL, orgRadiusR;
	private float shakingAmount = 1.33f;

	void Update(){
		if(isShakingL){
			SetLoffset (radiusL);
			radiusL = Mathf.Lerp (radiusL, orgRadiusL, 0.05f);
		}
        if (isShakingR) {
            SetRoffset(radiusR);
            radiusR = Mathf.Lerp(radiusR, orgRadiusR, 0.05f);
        }
    }

	public void SetRadius(float L, float R){
        Initvariables();

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

    void Initvariables() {
        isShakingL = false;
        isShakingR = false;

        for (int i = 0; i < 2; i++) {
           if (shakeCoroutine[i] != null) {
                StopCoroutine(shakeCoroutine[i]);
            }
        }
    }

	public void OnTargetL(bool b){
		switch(curImageL){
			case (int)Ctype.N_L0:
			case (int)Ctype.N_L1:
				SetCurrentLImage ((b)? (int)Ctype.N_L1 : (int)Ctype.N_L0);
				break;
			case (int)Ctype.RCL_0:
			case (int)Ctype.RCL_1:
				SetCurrentLImage ((b)? (int)Ctype.RCL_1 : (int)Ctype.RCL_0);
				break;
		default :
			return;
		}
	}
	public void OnTargetR(bool b){
		switch(curImageR){
		case (int)Ctype.N_R0:
		case (int)Ctype.N_R1:
			SetCurrentRImage ((b)? (int)Ctype.N_R1 : (int)Ctype.N_R0);
			break;
		default :
			return;
		}
	}

	public void SetCurrentLImage(int CurImage){
		curImageL = CurImage;
		switch(CurImage){
			case (int)Ctype.N_L0:
				crosshairs [(int)Ctype.N_L0].SetActive (true);
				crosshairs [(int)Ctype.N_L1].SetActive (false);
				break;
			case (int)Ctype.N_L1:
				crosshairs [(int)Ctype.N_L0].SetActive (false);
				crosshairs [(int)Ctype.N_L1].SetActive (true);
				break;
			case (int)Ctype.RCL_0:
				crosshairs [(int)Ctype.RCL_0].SetActive (true);
				crosshairs [(int)Ctype.RCL_1].SetActive (false);
				break;
			case (int)Ctype.RCL_1:
				crosshairs [(int)Ctype.RCL_0].SetActive (false);
				crosshairs [(int)Ctype.RCL_1].SetActive (true);
				break;
		}
	}
	public void SetCurrentRImage(int CurImage){
		curImageR = CurImage;
		switch(CurImage){
			case (int)Ctype.N_R0:
				crosshairs [(int)Ctype.N_R0].SetActive (true);
				crosshairs [(int)Ctype.N_R1].SetActive (false);
				break;
			case (int)Ctype.N_R1:
				crosshairs [(int)Ctype.N_R0].SetActive (false);
				crosshairs [(int)Ctype.N_R1].SetActive (true);
				break;
		}
	}
	public void CloseAllCrosshairs_L(){
		crosshairs [(int)Ctype.N_L0].SetActive (false);
		crosshairs [(int)Ctype.N_L1].SetActive (false);

		crosshairs [(int)Ctype.RCL_0].SetActive (false);
		crosshairs [(int)Ctype.RCL_1].SetActive (false);
	}
	public void CloseAllCrosshairs_R(){
		crosshairs [(int)Ctype.N_R0].SetActive (false);
		crosshairs [(int)Ctype.N_R1].SetActive (false);
	}

	public void ShakingEffect(int hand){
        if (hand == 0)
            isShakingL = true;
        else
            isShakingR = true;

		if (shakeCoroutine[hand] == null) {
			shakeCoroutine[hand] = StartCoroutine (PlayShakingEffect (hand));
		}else{//stop the current shaking & start a new one
			StopCoroutine (shakeCoroutine[hand]);
			shakeCoroutine[hand] = StartCoroutine (PlayShakingEffect (hand));
		}
	}

	IEnumerator PlayShakingEffect(int hand){
		if(hand==0){
			radiusL = orgRadiusL*shakingAmount;
		}else{
			radiusR = orgRadiusR*shakingAmount;
		}
		yield return new WaitForSeconds (0.2f);

        if (hand == 0) {
            isShakingL = false;
            radiusL = orgRadiusL;
            SetLoffset(radiusL);
        } else {
            isShakingR = false;
            radiusR = orgRadiusR;
            SetRoffset(radiusR);
        }
	}
}
