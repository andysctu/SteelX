using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour {

	private MechCombat mechCombat;
	private Animator animator;

	void Start(){
		mechCombat = GetComponentInParent<MechCombat> ();
		animator = GetComponent<Animator> ();
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
			animator.SetBool ("SlashL", false);
			break;
		case 2:
			animator.SetBool ("SlashL2", false);
			break;
		case 3:
			animator.SetBool ("SlashL3", false);
			break;
		}
	}
	public void CallSlashRToFalse(int num){
		switch(num){
		case 1:
			animator.SetBool ("SlashR", false);
			break;
		case 2:
			animator.SetBool ("SlashR2", false);
			break;
		case 3:
			animator.SetBool ("SlashR3", false);
			break;
		}
	}

	public void ReceiveNextSlash(int receive){
		mechCombat.SetReceiveNextSlash (receive);
	}

	public void Slash(int handposition){
		if(handposition==0){
			if(animator.GetBool("SlashL2")){
				animator.SetBool ("SlashL3",true);
			}else{
				if(animator.GetBool("SlashL")){
					animator.SetBool ("SlashL2", true);
				}else{
					animator.SetBool ("SlashL", true);
				}
			}
		}else{
			if(animator.GetBool("SlashR2")){
				animator.SetBool ("SlashR3",true);
			}else{
				if(animator.GetBool("SlashR")){
					animator.SetBool ("SlashR2", true);
				}else{
					animator.SetBool ("SlashR", true);
				}
			}
		}
	}
}
