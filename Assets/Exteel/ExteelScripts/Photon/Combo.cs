﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour {

	[SerializeField]private MechCombat mechCombat;
	[SerializeField]private MechController mctrl;
	[SerializeField]private Animator animator;
	[SerializeField]private AnimatorVars AnimatorVars;

	private int slashL_id;
	private int slashL2_id;
	private int slashL3_id;
	private int slashR_id;
	private int slashR2_id;
	private int slashR3_id;

	public void InitVars(){// called by AnimatorVars
		slashL_id = AnimatorVars.SlashL_id;
		slashL2_id = AnimatorVars.SlashL2_id;
		slashL3_id = AnimatorVars.SlashL3_id;
		slashR_id = AnimatorVars.SlashR_id;
		slashR2_id = AnimatorVars.SlashR2_id;
		slashR3_id = AnimatorVars.SlashR3_id;
	}

	public void CallLSlashPlaying(int isPlaying){
		mechCombat.SetLSlashPlaying(isPlaying);
		mechCombat.ShowTrailL (isPlaying==1);
	}
	public void CallRSlashPlaying(int isPlaying){
		mechCombat.SetRSlashPlaying(isPlaying);
		mechCombat.ShowTrailR (isPlaying==1);
	}
		
	public void CallSlashLToFalse(int num){
		switch(num){
		case 1:
			animator.SetBool (slashL_id, false);
			break;
		case 2:
			animator.SetBool (slashL2_id, false);
			break;
		case 3:
			animator.SetBool (slashL3_id, false);
			break;
		}
	}

	public void CallSlashRToFalse(int num){
		switch(num){
		case 1:
			animator.SetBool (slashR_id, false);
			break;
		case 2:
			animator.SetBool (slashR2_id, false);
			break;
		case 3:
			animator.SetBool (slashR3_id, false);
			break;
		}
	}

	public void ReceiveNextSlash(int receive){
		mechCombat.SetReceiveNextSlash (receive);
	}

	public void Slash(int handposition){
		if(handposition==0){
			if(animator.GetBool(slashL2_id)){
				animator.SetBool (slashL3_id,true);
			}else{
				if(animator.GetBool(slashL_id)){
					animator.SetBool (slashL2_id, true);
				}else{
					animator.SetBool (slashL_id, true);
				}
			}
		}else{
			if(animator.GetBool(slashR2_id)){
				animator.SetBool (slashR3_id,true);
			}else{
				if(animator.GetBool(slashR_id)){
					animator.SetBool (slashR2_id, true);
				}else{
					animator.SetBool (slashR_id, true);
				}
			}
		}
	}

	public void CallSetSlashMoving(float speed){//called by animation
		mctrl.SetSlashMoving(speed);
	}
}
