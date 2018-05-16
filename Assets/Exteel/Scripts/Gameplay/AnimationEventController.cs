using System.Collections.Generic;
using UnityEngine;

public class AnimationEventController : MonoBehaviour {
	[SerializeField]private MechCombat MechCombat;
    [SerializeField]private BuildMech bm;
    [SerializeField]private MechController MechController;
	[SerializeField]private MechCamera MechCamera;
	[SerializeField]private Animator Animator;
	[SerializeField]private AnimatorVars AnimatorVars;
	[SerializeField]private SlashDetector SlashDetector;
    [SerializeField]private Sounds Sounds;
    private Animator[] weaponAnimators = new Animator[4];
    private PhotonView pv;
    private int minCallMoveDistance = 10, weaponOffset = 0;


    void Awake() {
        if(MechCombat!=null)MechCombat.OnWeaponSwitched += UpdateAnimationEventController;
        if (bm != null) bm.OnWeaponBuilt += InitWeaponAnimators;
    }

    void Start(){
        pv = GetComponent<PhotonView> ();
	}

    private void InitWeaponAnimators() {
        for(int i = 0; i < 4; i++) {          
            if(bm.weapons[i]!=null)
                weaponAnimators[i] = bm.weapons[i].GetComponent<Animator>();
        }
    }

    private void UpdateAnimationEventController() {
        weaponOffset = MechCombat.GetCurrentWeaponOffset();
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
		if (!pv.isMine)//other player does not have the hash id
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

	public void ReceiveNextSlash(int receive){//if receive => get mouse button
		MechCombat.SetReceiveNextSlash (receive);
	}

	public void Slash(int hand){
		if(hand==0){
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

	public void Smash(int hand){
		if (hand == 0)
			MechCombat.isLMeleePlaying = 1;
		else
			MechCombat.isRMeleePlaying = 1;


		if (Animator.GetBool (AnimatorVars.grounded_id)) {
			pv.RPC ("SmashRPC", PhotonTargets.All, hand, 0);
		} else {
			if (MechCamera.GetCamAngle () <= -10)
				pv.RPC ("SmashRPC", PhotonTargets.All, hand, 1);
			else if (MechCamera.GetCamAngle () >= 10)
				pv.RPC ("SmashRPC", PhotonTargets.All, hand, 3);
			else
				pv.RPC ("SmashRPC", PhotonTargets.All, hand, 2);
		}
	}

	[PunRPC]
	void SlashRPC(int hand, int mode){//0 : Slash 1 , 1 : Slash low , 2 : Slash middle , 3 : Slash high
		if (hand == 0) {
			switch(mode){
			case 0:
				Animator.Play ("SlashL");
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
				Animator.Play ("SlashR");
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
				MechController.SetMoving(speed);//complete move
			}else{
				//check if there is any target in front & the distance between is higher than a number
				foreach (Transform t in targets) {
					if (t == null || (t.tag!="Drone" && t.root.GetComponent<MechCombat> ().isDead))
						continue;

					if (Vector3.Distance (transform.position, t.position) >= minCallMoveDistance) {
						MechController.SetMoving (speed / 2);//not getting too far
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

    public void CallShoot(int hand) {
        if(weaponAnimators[weaponOffset + hand] != null) {
            weaponAnimators[weaponOffset + hand].SetTrigger("Atk");
        }

        MechCombat.InstantiateBulletTrace(hand);
    }

    public void CallReload(int hand) {
        if (weaponAnimators[weaponOffset + hand] != null) {
            weaponAnimators[weaponOffset + hand].SetTrigger("Reload");
        }
    }
}
