using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour {

	public void CallLSlashPlaying(int isPlaying){
		GetComponentInParent<MechCombat> ().SetIsLSlashPlaying(isPlaying);
	}
	public void CallSlashL1ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashL1ToFalse();
	}
	public void CallSlashR1ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashR1ToFalse();
	}
	public void CallSlashL2ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashL2ToFalse();
	}
	public void CallSlashL3ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashL3ToFalse();
	}
	public void CallRSlashPlaying(int isPlaying){
		GetComponentInParent<MechCombat> ().SetIsRSlashPlaying(isPlaying);
	}
	public void CallSlashR2ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashR2ToFalse();
	}
	public void CallSlashR3ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashR3ToFalse();
	}
}
