﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HeatBar : MonoBehaviour {

	[SerializeField]public Image barL,barR;
	[SerializeField]private Image barL_fill,barR_fill;
	[SerializeField]private BuildMech bm;
	[SerializeField]private MechCombat mcbt;
	[SerializeField]private PhotonView pv;//mech combat's pv

	private Weapon[] weaponScripts;
	private float[] curValue = new float[4];
	private int weaponOffset;
	private int cooldown;
	private int MaxHeat;
	private Color32 RED = new Color32 (255, 0, 0, 200) , YELLOW = new Color32 (255, 255, 85, 200);

	public void InitVars(){//called when finished buildmech
		weaponOffset = 0;
		weaponScripts = bm.weaponScripts;
		barL_fill.fillAmount = 0;
		barR_fill.fillAmount = 0;
		cooldown = mcbt.cooldown;
		MaxHeat = mcbt.MaxHeat;

		for (int i = 0; i < 4; i++)
			mcbt.is_overheat [i] = false;

		UpdateHeatBar (weaponOffset);
		ResetHeatBar ();
	}

	void FixedUpdate(){
		
		for(int i=0;i<4;i++){
			curValue[i] -= ((mcbt.is_overheat[i])? cooldown : cooldown/2) *Time.fixedDeltaTime;// cooldown faster when overheat

			if (curValue [i] <= 0){
				if(mcbt.is_overheat[i]){ // if previous is overheated => change color
					if(i==weaponOffset){
						barL_fill.color = YELLOW;
					}else if(i==weaponOffset+1){
						barR_fill.color = YELLOW;
					}
					pv.RPC ("SetOverHeat", PhotonTargets.All, false, i);
				}
					mcbt.is_overheat [i] = false;
					curValue [i] = 0;
			}
		}

		DrawBarL ();
		DrawBarR ();
	}

	public void UpdateHeatBar(int offset){
		weaponOffset = offset;

		if(bm.weaponScripts[offset].isTwoHanded){
			EnableHeatBar (weaponOffset, true);
			EnableHeatBar (weaponOffset+1, false);
		}else{
			EnableHeatBar (weaponOffset, bm.weaponScripts [weaponOffset].Animation != "");
			EnableHeatBar (weaponOffset+1, bm.weaponScripts [weaponOffset+1].Animation != "");
		}

		if(mcbt.is_overheat[offset]){//update color
			barL_fill.color = RED;
		}else{
			barL_fill.color = YELLOW;
		}

		if(mcbt.is_overheat[offset+1]){
			barR_fill.color = RED;
		}else{
			barR_fill.color = YELLOW;
		}
	}

	void ResetHeatBar(){
		for(int i=0;i<4;i++){
			curValue [i] = 0;
		}
	}

	public void IncreaseHeatBarL(float value){ //value : [0,100]
		curValue [weaponOffset] += value;
		if (curValue [weaponOffset] >= MaxHeat) {
			curValue[weaponOffset] = MaxHeat;
			mcbt.is_overheat [weaponOffset] = true;

			pv.RPC ("SetOverHeat", PhotonTargets.All, true, weaponOffset);

			barL_fill.color = RED;
		}
	}

	public void IncreaseHeatBarR(float value){
		curValue [weaponOffset+1] += value;
		if (curValue [weaponOffset + 1] >= MaxHeat) {
			curValue[weaponOffset + 1] = MaxHeat;
			mcbt.is_overheat [weaponOffset + 1] = true;

			pv.RPC ("SetOverHeat", PhotonTargets.All, true, weaponOffset+1);

			barR_fill.color = RED;
		}
	}

	public void EnableHeatBar(int hand, bool b){
		if(hand==0){
			barL.enabled = b;
			barL_fill.enabled = b;
		}else{
			barR.enabled = b;
			barR_fill.enabled = b;
		}
	}

	private void DrawBarL(){
		barL_fill.fillAmount = curValue[weaponOffset]/MaxHeat;
	}

	private void DrawBarR(){
		barR_fill.fillAmount = curValue [weaponOffset + 1]/MaxHeat;
	}
}