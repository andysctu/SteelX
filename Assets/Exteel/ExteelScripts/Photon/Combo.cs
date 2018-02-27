using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour {

	private MechCombat mechCombat;

	void Start(){
		mechCombat = GetComponentInParent<MechCombat> ();
	}
	public void CallLSlashPlaying(int isPlaying){
		mechCombat.SetLSlashPlaying(isPlaying);

		mechCombat.ShowTrailL ((isPlaying==1)? true : false);
	}

	public void CallSlashLToFalse(){
		mechCombat.SetSlashLToFalse();
	}

	public void CallRSlashPlaying(int isPlaying){
		mechCombat.SetRSlashPlaying(isPlaying);

		mechCombat.ShowTrailR ((isPlaying==1)? true : false);
	}

	public void CallSlashRToFalse(){
		mechCombat.SetSlashRToFalse();
	}

	public void ReceiveNextSlash(int receive){
		mechCombat.SetReceiveNextSlash (receive);
	}

}
