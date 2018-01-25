using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour {

	public void CallLSlashPlaying(int isPlaying){
		print ("isPlaying : " + isPlaying);
		GetComponentInParent<MechCombat> ().SetIsLSlashPlaying(isPlaying);
	}
	public void CallSlashL2ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashL2ToFalse();
	}
	public void CallSlashL3ToFalse(){
		GetComponentInParent<MechCombat> ().SetSlashL3ToFalse();
	}
}
