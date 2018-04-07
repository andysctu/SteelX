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
	private int onMelee_id;

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
		onMelee_id = AnimatorVars.onMelee_id;
	}

	public void CallLMeleePlaying(int isPlaying){//this is called when melee attack in air , to make the mech drop when attacking ( for nicier look)
		mechCombat.SetLMeleePlaying(isPlaying);
	}
	public void CallRMeleePlaying(int isPlaying){
		mechCombat.SetRMeleePlaying(isPlaying);
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
					if (!animator.GetBool (onMelee_id)) {
						if (animator.GetBool (grounded_id)) {
							pv.RPC ("SlashRPC", PhotonTargets.All, 0, 0);
						}else{
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
					if (!animator.GetBool (onMelee_id)) {
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

	public void Smash(int handposition){
		if (handposition == 0)
			mechCombat.isLMeleePlaying = 1;
		else
			mechCombat.isRMeleePlaying = 1;


		if (animator.GetBool (grounded_id)) {
			pv.RPC ("SmashRPC", PhotonTargets.All, handposition, 0);
		} else {
			if (mcam.GetCamAngle () <= -10)
				pv.RPC ("SmashRPC", PhotonTargets.All, handposition, 1);
			else if (mcam.GetCamAngle () >= 10)
				pv.RPC ("SmashRPC", PhotonTargets.All, handposition, 3);
			else
				pv.RPC ("SmashRPC", PhotonTargets.All, handposition, 2);
		}
	}

	[PunRPC]
	void SlashRPC(int hand, int mode){//0 : Slash 1 , 1 : Slash low , 2 : Slash middle , 3 : Slash high
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
	void SmashRPC(int hand, int mode){
		if (hand == 0) {
			switch (mode) {
			case 0:
				animator.Play ("SmashL");
				break;
			case 1:
				animator.Play ("SmashLlow");
				break;
			case 2:
				animator.Play ("SmashLmiddle");
				break;
			case 3:
				animator.Play ("SmashLhigh");
				break;
			}
		} else {
			switch (mode) {
			case 0:
				animator.Play ("SmashR");
				break;
			case 1:
				animator.Play ("SmashRlow");
				break;
			case 2:
				animator.Play ("SmashRmiddle");
				break;
			case 3:
				animator.Play ("SmashRhigh");
				break;
			}
		}
	}

	public void CallSlashDetect(int hand){
		if (!pv.isMine)
			return;
		mechCombat.SlashDetect (hand);
	}

	/*public void CallSmashDetect(int hand){
		if (!pv.isMine)
			return;
		mechCombat.SlashDetect (hand);
	}*/

	public void CallMoving(float speed){//called by animation ( also by BCN shoot with speed < 0)
		if (!pv.isMine)
			return;
		
		if(speed >0){
			mcam.LockCamRotation (true);
			List<Transform> targets = SlashDetector.getCurrentTargets ();
			if(targets.Count == 0){
				mctrl.SetMoving(speed);
			}else{
				//check if there is any target in front & the distance between
				foreach (Transform t in targets) {
					if (t == null || t.root.GetComponent<MechCombat> ().isDead)
						continue;

					if (Vector3.Distance (transform.position, t.position) >= 10) {
						mctrl.SetMoving (speed / 2);
					}
				}
			}
		}else if(speed < 0){
			mctrl.SetMoving(speed);
		}
	}

	public void CallBCNShoot(int b){
		if (!pv.isMine)
			return;
		
		mctrl.on_BCNShoot = (b==1);
	}

}
