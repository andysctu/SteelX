using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HeatBar : MonoBehaviour {

	[SerializeField]
	public Image barL,barR;
	[SerializeField]
	private Image circleL,circleR;
	[SerializeField]
	private BuildMech bm;
	private Weapon[] weaponScripts;
	private float[] curValue = new float[4]; // [0,100]
	private int weaponOffset = 0;
	private float rateL,rateR;
	//private float FillAmountMin = 0.25f, FillAmountMax = 0;
	private bool[] is_overheat = new bool[4];

	// Use this for initialization
	void Start () {
		weaponScripts = bm.weaponScripts;
		circleL.fillAmount = 0.25f;
		circleR.fillAmount = 0.25f;
		rateL = 0.2f;
		rateR = 0.2f;

		for (int i = 0; i < 4; i++)
			is_overheat [i] = false;
	}

	void FixedUpdate(){

		for(int i=0;i<4;i++){
			curValue[i] -= ((i%2)==0)? rateL : rateR;

			if (curValue [i] <= 0){
				if(is_overheat[i]){ // if previous is overheated => change color
					if(i==weaponOffset){
						barL.color = new Color32 (255, 255, 0, 200);
					}else if(i==weaponOffset+1){
						barR.color = new Color32 (255, 255, 0, 200);
					}
				}
				is_overheat [i] = false;
				curValue [i] = 0;

			}
		}

		DrawBarL ();
		DrawBarR ();
	}

	public void UpdateHeatBar(int offset){
		weaponOffset = offset;

		if(bm.weaponScripts[offset].isTwoHanded){
			barL.enabled = true;
			circleL.enabled = true;
			barR.enabled = false;
			circleR.enabled = false;
		}else{
			if (bm.weaponScripts [offset].Animation != "") {//Empty weapon 
				barL.enabled = true;
				circleL.enabled = true;
			} else {
				barL.enabled = false;
				circleL.enabled = false;
			}

			if (bm.weaponScripts [offset + 1].Animation != "") {
				barR.enabled = true;
				circleR.enabled = true;
			} else {
				barR.enabled = false;
				circleR.enabled = false;
			}
		}

		if(is_overheat[offset]){//update color
			barL.color =new Color32 (255, 0, 0, 200);
		}else{
			barL.color =new Color32 (255, 255, 0, 200);
		}

		if(is_overheat[offset+1]){
			barR.color =new Color32 (255, 0, 0, 200);
		}else{
			barR.color =new Color32 (255, 255, 0, 200);
		}
	}

	public void IncreaseHeatBarL(float value){ //value : [0,100]
		curValue [weaponOffset] += value;
		if (curValue [weaponOffset] >= 100) {
			curValue[weaponOffset] = 100;
			is_overheat [weaponOffset] = true;
			barL.color =new Color32 (255, 0, 0, 200);
		}
	}

	public void IncreaseHeatBarR(float value){
		curValue [weaponOffset+1] += value;
		if (curValue [weaponOffset + 1] >= 100) {
			curValue[weaponOffset + 1] = 100;
			is_overheat [weaponOffset + 1] = true;
			barR.color = new Color32 (255, 0, 0, 200);
		}
	}
	public void ResetHeatBar(){
		for(int i=0;i<4;i++){
			curValue [i] = 0;
		}
	}

	public bool Is_HeatBarL_Overheat(){
		return is_overheat [weaponOffset];
	}

	public bool Is_HeatBarR_Overheat(){
		return is_overheat [weaponOffset+1];
	}

	public void NoHeatBarL(){
		barL.enabled = false;
		circleL.enabled = false;
	}

	public void NoHeatBarR(){
		barR.enabled = false;
		circleR.enabled = false;
	}

	private void DrawBarL(){//heat value : [0,100] -> [0.25,0]
		circleL.fillAmount = 0.25f - curValue[weaponOffset] * 0.0025f;
	}

	private void DrawBarR(){
		circleR.fillAmount = 0.25f - curValue[weaponOffset+1] * 0.0025f;
	}
}
