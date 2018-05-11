using System.Collections.Generic;
using UnityEngine;

public class AnimationEventController : MonoBehaviour {
	[SerializeField]private MechCombat MechCombat;
	[SerializeField]private MechController MechController;
	[SerializeField]private MechCamera MechCamera;
	[SerializeField]private Animator Animator;
	[SerializeField]private AnimatorVars AnimatorVars;
	[SerializeField]private SlashDetector SlashDetector;
	private PhotonView pv;

	void Start(){
		pv = GetComponent<PhotonView> ();
	}

	public void CallLMeleePlaying(int isPlaying){//this can control the drop time when attacking in air
		MechCombat.SetLMeleePlaying(isPlaying);
	}
	public void CallRMeleePlaying(int isPlaying){
		MechCombat.SetRMeleePlaying(isPlaying);
	}

	public void CallShowTrailL(int show){
		MechCombat.ShowTrailL (show==1);
	}

	public void CallShowTrailR(int show){
		MechCombat.ShowTrailR (show==1);
	}
		
	public void CallSlashLToFalse(int num){
		if (!pv.isMine)
			return;

		switch(num){
		case 1:
			Animator.SetBool (AnimatorVars.slashL_id, false);
			break;
		case 2:
			Animator.SetBool (AnimatorVars.slashL2_id, false);
			break;
		case 3:
			Animator.SetBool (AnimatorVars.slashL3_id, false);
			break;
		case 4:
			Animator.SetBool (AnimatorVars.slashL4_id, false);
			break;
		}
	}

	public void CallSlashRToFalse(int num){
		if (!pv.isMine)
			return;

		switch(num){
		case 1:
			Animator.SetBool (AnimatorVars.slashR_id, false);
			break;
		case 2:
			Animator.SetBool (AnimatorVars.slashR2_id, false);
			break;
		case 3:
			Animator.SetBool (AnimatorVars.slashR3_id, false);
			break;
		case 4:
			Animator.SetBool (AnimatorVars.slashR4_id, false);
			break;
		}
	}

	public void ReceiveNextSlash(int receive){
		MechCombat.SetReceiveNextSlash (receive);
	}

	public void Slash(int handposition){
		if(handposition==0){
			if (Animator.GetBool (AnimatorVars.slashL3_id)) {
				Animator.SetBool (AnimatorVars.slashL4_id, true);
			} else if (Animator.GetBool (AnimatorVars.slashL2_id)) {
				Animator.SetBool (AnimatorVars.slashL3_id, true);
			}else{
				if(Animator.GetBool(AnimatorVars.slashL_id)){
					Animator.SetBool (AnimatorVars.slashL2_id, true);
				}else{
					if (!Animator.GetBool (AnimatorVars.onMelee_id)) {
						if (Animator.GetBool (AnimatorVars.grounded_id)) {
							pv.RPC ("SlashRPC", PhotonTargets.All, 0, 0);
						}else{
							if(MechCamera.GetCamAngle()<=-10)
								pv.RPC ("SlashRPC", PhotonTargets.All, 0, 1);
							else if(MechCamera.GetCamAngle()>=10)
								pv.RPC ("SlashRPC", PhotonTargets.All, 0, 3);
							else
								pv.RPC ("SlashRPC", PhotonTargets.All, 0, 2);
						}
						Animator.SetBool (AnimatorVars.slashL_id, true);
					}
				}
			}
		}else{
			if (Animator.GetBool (AnimatorVars.slashR3_id)) {
				Animator.SetBool (AnimatorVars.slashR4_id, true);
			} else 	if(Animator.GetBool(AnimatorVars.slashR2_id)){
				Animator.SetBool (AnimatorVars.slashR3_id,true);
			}else{
				if(Animator.GetBool(AnimatorVars.slashR_id)){
					Animator.SetBool (AnimatorVars.slashR2_id, true);
				}else{
					if (!Animator.GetBool (AnimatorVars.onMelee_id)) {
						if (Animator.GetBool (AnimatorVars.grounded_id)) {
							pv.RPC ("SlashRPC", PhotonTargets.All, 1, 0);
						}else{
							if(MechCamera.GetCamAngle()<=-10)
								pv.RPC ("SlashRPC", PhotonTargets.All, 1, 1);
							else if(MechCamera.GetCamAngle()>=10)
								pv.RPC ("SlashRPC", PhotonTargets.All, 1, 3);
							else
								pv.RPC ("SlashRPC", PhotonTargets.All, 1, 2);
						}
						Animator.SetBool (AnimatorVars.slashR_id, true);
					}
				}
			}
		}
	}

	public void Smash(int handposition){
		if (handposition == 0)
			MechCombat.isLMeleePlaying = 1;
		else
			MechCombat.isRMeleePlaying = 1;


		if (Animator.GetBool (AnimatorVars.grounded_id)) {
			pv.RPC ("SmashRPC", PhotonTargets.All, handposition, 0);
		} else {
			if (MechCamera.GetCamAngle () <= -10)
				pv.RPC ("SmashRPC", PhotonTargets.All, handposition, 1);
			else if (MechCamera.GetCamAngle () >= 10)
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
				Animator.Play ("SlashL1");
				break;
			case 1:
				Animator.Play ("SlashLlow");
				break;
			case 2:
				Animator.Play ("SlashLmiddle");
				break;
			case 3:
				Animator.Play ("SlashLhigh");
				break;
			}
		}else{
			switch(mode){
			case 0:
				Animator.Play ("SlashR1");
				break;
			case 1:
				Animator.Play ("SlashRlow");
				break;
			case 2:
				Animator.Play ("SlashRmiddle");
				break;
			case 3:
				Animator.Play ("SlashRhigh");
				break;
			}
		}
	}

	[PunRPC]
	void SmashRPC(int hand, int mode){
		if (hand == 0) {
			switch (mode) {
			case 0:
				Animator.Play ("SmashL");
				break;
			case 1:
				Animator.Play ("SmashLlow");
				break;
			case 2:
				Animator.Play ("SmashLmiddle");
				break;
			case 3:
				Animator.Play ("SmashLhigh");
				break;
			}
		} else {
			switch (mode) {
			case 0:
				Animator.Play ("SmashR");
				break;
			case 1:
				Animator.Play ("SmashRlow");
				break;
			case 2:
				Animator.Play ("SmashRmiddle");
				break;
			case 3:
				Animator.Play ("SmashRhigh");
				break;
			}
		}
	}

	public void CallSlashDetect(int hand){
		if (!pv.isMine)
			return;
		MechCombat.SlashDetect (hand);
	}

	public void CallSmashDetect(int hand){
		if (!pv.isMine)
			return;
		MechCombat.SlashDetect (hand);
	}

	public void CallMoving(float speed){//also called by BCN shoot with speed < 0
		if (!pv.isMine)
			return;
		
		if(speed >0){
			List<Transform> targets = SlashDetector.getCurrentTargets ();
			if(targets.Count == 0 || !MechController.grounded){
				MechController.SetMoving(speed);
			}else{
				//check if there is any target in front & the distance between
				foreach (Transform t in targets) {
					if (t == null || (t.tag!="Drone" && t.root.GetComponent<MechCombat> ().isDead))
						continue;

					if (Vector3.Distance (transform.position, t.position) >= 10) {
						MechController.SetMoving (speed / 2);
					}
				}
			}
		}else if(speed < 0){
			MechController.SetMoving(speed);
		}
	}

	public void BCNPose(){
        if (!pv.isMine)
            return;
		pv.RPC ("BCNPoseRPC", PhotonTargets.All);
	}

	[PunRPC]
	public void BCNPoseRPC(){
		Animator.Play ("BCNPose_Start");
	}

	public void CallBCNShoot(int b){
		if (!pv.isMine)
			return;
		
		MechController.on_BCNShoot = (b==1);
		if(b==0){
			Animator.SetBool ("BCNPose", false);
			Animator.SetBool ("OnBCN", false);
		}
	}

	public void SetBCNLoadToFalse(){
		Animator.SetBool ("BCNLoad", false);
		Animator.SetBool ("OnBCN", false);
		Animator.SetBool ("BCNPose", false);
		MechCombat.BCNbulletNum = 2;
	}

	public void CallJump(){
		MechController.Jump ();
	}
    //
    //Ranged weapon animation events
    //
    public void CallShoot() {
        
    }

}
