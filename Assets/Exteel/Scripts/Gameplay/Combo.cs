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
		if (!pv.isMine)
			return;
		
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
		if (!pv.isMine)
			return;
		
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
		if (!pv.isMine)
			return;
		
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
							print (mcam.GetCamAngle ());
							if(mcam.GetCamAngle()<=-20)
								pv.RPC ("SlashRPC", PhotonTargets.All, 0, 1);
							else if(mcam.GetCamAngle()>=20)
								pv.RPC ("SlashRPC", PhotonTargets.All, 0, 3);
							else
								pv.RPC ("SlashRPC", PhotonTargets.All, 0, 2);
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
							if(mcam.GetCamAngle()<=-20)
								pv.RPC ("SlashRPC", PhotonTargets.All, 1, 1);
							else if(mcam.GetCamAngle()>=20)
								pv.RPC ("SlashRPC", PhotonTargets.All, 1, 3);
							else
								pv.RPC ("SlashRPC", PhotonTargets.All, 1, 2);
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
	void SlashRPC(int hand, int mode){//0 : SlashL1 , 1 : SlashLlow , 2 : SlashLmiddle , 3 : SlashLhigh
		if (hand == 0) {
			switch(mode){
			case 0:
				animator.Play ("SlashL1");
				break;
			case 1:
				animator.Play ("SlashLlow");
				break;
			case 2:
				animator.Play ("SlashLmiddle");
				break;
			case 3:
				animator.Play ("SlashLhigh");
				break;
			}
		}else{
			switch(mode){
			case 0:
				animator.Play ("SlashR1");
				break;
			case 1:
				animator.Play ("SlashRlow");
				break;
			case 2:
				animator.Play ("SlashRmiddle");
				break;
			case 3:
				animator.Play ("SlashRhigh");
				break;
			}
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
		if (!pv.isMine)
			return;

		mechCombat.SlashDetect (hand);
	}

	public void CallSetSlashMoving(float speed){//called by animation ( also by BCN shoot with speed < 0)
		if (!pv.isMine)
			return;
		
		if(speed >0){
			mcam.LockCamRotation (true);
			List<Transform> targets = SlashDetector.getCurrentTargets ();
			if(targets.Count == 0){
				mctrl.SetSlashMoving(speed);
			}else{
				//check if there is any target in front & the distance between
				foreach (Transform t in targets) {
					if (t == null || t.root.GetComponent<MechCombat> ().isDead)
						continue;

					if (Vector3.Distance (transform.position, t.position) >= 10) {
						mctrl.SetSlashMoving (speed / 2);
					}
				}
			}
		}else if(speed < 0){
			mctrl.SetSlashMoving(speed);
		}
	}

	public void CallBCNShoot(int b){
		if (!pv.isMine)
			return;
		
		mctrl.on_BCNShoot = (b==1);
	}

}
