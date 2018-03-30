using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Combo : MonoBehaviour {

	[SerializeField]private MechCombat mechCombat;
	[SerializeField]private MechController mctrl;
	[SerializeField]private MechCamera mcam;
	[SerializeField]private Animator animator;
	[SerializeField]private AnimatorVars AnimatorVars;
	[SerializeField]private SlashDetector SlashDetector;
	private PhotonView pv;
	private int slashL_id;
	private int slashL2_id;
	private int slashL3_id;
	private int slashR_id;
	private int slashR2_id;
	private int slashR3_id;

	private int grounded_id;
	private int onSlash_id;

	void Start(){
		pv = GetComponent<PhotonView> ();
	}

	public void InitVars(){// called by AnimatorVars
		slashL_id = AnimatorVars.SlashL_id;
		slashL2_id = AnimatorVars.SlashL2_id;
		slashL3_id = AnimatorVars.SlashL3_id;
		slashR_id = AnimatorVars.SlashR_id;
		slashR2_id = AnimatorVars.SlashR2_id;
		slashR3_id = AnimatorVars.SlashR3_id;

		grounded_id = AnimatorVars.grounded_id;
		onSlash_id = AnimatorVars.onSlash_id;
	}

	public void CallLSlashPlaying(int isPlaying){
		mechCombat.SetLSlashPlaying(isPlaying);
	}
	public void CallRSlashPlaying(int isPlaying){
		mechCombat.SetRSlashPlaying(isPlaying);
	}

	public void CallShowTrailL(int show){
		mechCombat.ShowTrailL (show==1);
	}

	public void CallShowTrailR(int show){
		mechCombat.ShowTrailR (show==1);
	}
		
	public void CallSlashLToFalse(int num){
		mcam.LockCamRotation (false);
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
		mcam.LockCamRotation (false);
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
					if (!animator.GetBool (onSlash_id)) {
						if (animator.GetBool (grounded_id)) {
							pv.RPC ("SlashRPC", PhotonTargets.All, 0, 0);
						}else{
							pv.RPC ("SlashRPC", PhotonTargets.All, 0, 1);
						}
						animator.SetBool (slashL_id, true);
					}
				}
			}
		}else{
			if(animator.GetBool(slashR2_id)){
				animator.SetBool (slashR3_id,true);
			}else{
				if(animator.GetBool(slashR_id)){
					animator.SetBool (slashR2_id, true);
				}else{
					if (!animator.GetBool (onSlash_id)) {
						if (animator.GetBool (grounded_id)) {
							pv.RPC ("SlashRPC", PhotonTargets.All, 1, 0);
						}else{
							pv.RPC ("SlashRPC", PhotonTargets.All, 1, 1);
						}
						animator.SetBool (slashR_id, true);
					}
				}
			}
		}
	}

	public void Lance(int handposition){
		pv.RPC ("LanceRPC", PhotonTargets.All, handposition, (animator.GetBool(grounded_id)? 0 : 1 ));
	}

	[PunRPC]
	void SlashRPC(int hand, int mode){
		if (hand == 0) {
			if(mode==0)
				animator.Play ("Sword L1");
			else
				animator.Play ("Ldown F");
		}else{
			if(mode==0)
				animator.Play ("Sword R1");
			else
				animator.Play ("Rdown F");
		}
	}

	[PunRPC]
	void LanceRPC(int hand, int mode){
		if (hand == 0) {
			if(mode==0)
				animator.Play ("Lance L");
			else
				animator.Play ("LancedownL");
		}else{
			if(mode==0)
				animator.Play ("Lance R");
			else
				animator.Play ("LancedownR");
		}
	}

	public void CallSlashDetect(int hand){
		mechCombat.SlashDetect (hand);
	}

	public void CallSetSlashMoving(float speed){//called by animation ( also by BCN shoot with speed < 0)
		if(speed >0){
			mcam.LockCamRotation (true);
			List<Transform> targets = SlashDetector.getCurrentTargets ();
			if(targets.Count == 0){
				mctrl.SetSlashMoving(speed);
			}else{
				//choose the first one
				if(Vector3.Distance(transform.position, targets[0].position) >= 10 ){
					mctrl.SetSlashMoving(speed/2);
				}
			}
		}else if(speed < 0){
			mctrl.SetSlashMoving(speed);
		}
	}

	public void CallBCNShoot(int b){
		mctrl.on_BCNShoot = (b==1);
	}

}
