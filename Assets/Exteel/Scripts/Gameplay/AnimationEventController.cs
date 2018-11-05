using System.Collections.Generic;
using UnityEngine;
using Weapons;

public class AnimationEventController : MonoBehaviour {
    [SerializeField] private MechCombat MechCombat;
    [SerializeField] private BuildMech bm;
    [SerializeField] private MechController MechController;
    [SerializeField] private MechCamera MechCamera;
    [SerializeField] private Animator Animator;
    [SerializeField] private AnimatorVars AnimatorVars;
    [SerializeField] private SlashDetector SlashDetector;
    private PhotonView pv;
    private int minCallMoveDistance = 6;

    private void Awake(){
        pv = GetComponent<PhotonView>();
    }

    public void CallLMeleePlaying(int isPlaying) {//this can control the drop time when attacking in air
        MechCombat.SetMeleePlaying(isPlaying == 1);
    }

    public void CallRMeleePlaying(int isPlaying) {
        MechCombat.SetMeleePlaying(isPlaying == 1);
    }

    public void CallShowTrailL(int show) {
        Sword sword = bm.Weapons[MechCombat.GetCurrentWeaponOffset()] as Sword;
        if(sword != null)sword.EnableWeaponTrail(show == 1);
    }

    public void CallShowTrailR(int show) {
        Sword sword = bm.Weapons[MechCombat.GetCurrentWeaponOffset()+1] as Sword;
        if (sword != null)sword.EnableWeaponTrail(show == 1);
    }

    public void Slash(int hand, int combo) {//combo starts from 0
        if (hand == 0) {
            if(combo == 3) {
                Animator.SetBool(AnimatorVars.FinalSlashHash, true);
            } else {
                if (!Animator.GetBool(AnimatorVars.OnMeleeHash)) {
                    if (Animator.GetBool(AnimatorVars.GroundedHash)) {
                        pv.RPC("SlashRPC", PhotonTargets.All, 0, 0);
                    } else {
                        if (MechCamera.GetCamAngle() <= -10)
                            pv.RPC("SlashRPC", PhotonTargets.All, 0, 1);
                        else if (MechCamera.GetCamAngle() >= 10)
                            pv.RPC("SlashRPC", PhotonTargets.All, 0, 3);
                        else
                            pv.RPC("SlashRPC", PhotonTargets.All, 0, 2);
                    }
                } else {
                    Animator.SetBool(AnimatorVars.SlashHash, true);
                }
            }
        } else {
            if (combo == 3) {
                Animator.SetBool(AnimatorVars.FinalSlashHash, true);
            } else {
                if (!Animator.GetBool(AnimatorVars.OnMeleeHash)) {
                    if (Animator.GetBool(AnimatorVars.GroundedHash)) {
                        pv.RPC("SlashRPC", PhotonTargets.All, 1, 0);
                    } else {
                        if (MechCamera.GetCamAngle() <= -10)
                            pv.RPC("SlashRPC", PhotonTargets.All, 1, 1);
                        else if (MechCamera.GetCamAngle() >= 10)
                            pv.RPC("SlashRPC", PhotonTargets.All, 1, 3);
                        else
                            pv.RPC("SlashRPC", PhotonTargets.All, 1, 2);
                    }
                } else {
                    Animator.SetBool(AnimatorVars.SlashHash, true);
                }
            }
        }
    }

    public void Smash(int hand) {
        MechCombat.SetMeleePlaying(true);

        if (Animator.GetBool(AnimatorVars.GroundedHash)) {
            pv.RPC("SmashRPC", PhotonTargets.All, hand, 0);
        } else {
            if (MechCamera.GetCamAngle() <= -10)
                pv.RPC("SmashRPC", PhotonTargets.All, hand, 1);
            else if (MechCamera.GetCamAngle() >= 10)
                pv.RPC("SmashRPC", PhotonTargets.All, hand, 3);
            else
                pv.RPC("SmashRPC", PhotonTargets.All, hand, 2);
        }
    }

    [PunRPC]
    private void SlashRPC(int hand, int mode) {//0 : Slash 1 , 1 : Slash low , 2 : Slash middle , 3 : Slash high
        if (hand == 0) {
            switch (mode) {
                case 0:
                Animator.Play("SlashL");
                break;
                case 1:
                Animator.Play("SlashLlow");
                break;
                case 2:
                Animator.Play("SlashLmiddle");
                break;
                case 3:
                Animator.Play("SlashLhigh");
                break;
            }
        } else {
            switch (mode) {
                case 0:
                Animator.Play("SlashR");
                break;
                case 1:
                Animator.Play("SlashRlow");
                break;
                case 2:
                Animator.Play("SlashRmiddle");
                break;
                case 3:
                Animator.Play("SlashRhigh");
                break;
            }
        }
    }

    [PunRPC]
    private void SmashRPC(int hand, int mode) {
        if (hand == 0) {
            switch (mode) {
                case 0:
                Animator.Play("SmashL");
                break;
                case 1:
                Animator.Play("SmashLlow");
                break;
                case 2:
                Animator.Play("SmashLmiddle");
                break;
                case 3:
                Animator.Play("SmashLhigh");
                break;
            }
        } else {
            switch (mode) {
                case 0:
                Animator.Play("SmashR");
                break;
                case 1:
                Animator.Play("SmashRlow");
                break;
                case 2:
                Animator.Play("SmashRmiddle");
                break;
                case 3:
                Animator.Play("SmashRhigh");
                break;
            }
        }
    }

    public void CallMoving(float speed) {//also called by Cn shoot with speed < 0
        if (!pv.isMine)
            return;

        if (speed > 0) {
            List<Transform> targets = SlashDetector.getCurrentTargets();
            if (targets.Count == 0 || !MechController.Grounded) {
                MechController.SetMoving(speed);//complete move
            } else {
                //check if there is any target in front & the distance between is higher than a number
                foreach (Transform t in targets) {
                    if (t == null || (t.tag != "Drone" && t.root.GetComponent<MechCombat>().isDead))
                        continue;

                    if (Vector3.Distance(transform.position, t.position) >= minCallMoveDistance) {
                        MechController.SetMoving(speed / 2);//not getting too far
                    }
                }
            }
        } else if (speed < 0) {
            MechController.SetMoving(speed);
        }
    }

    public void CnPose() {
        pv.RPC("CnPoseRPC", PhotonTargets.All);
    }

    [PunRPC]
    public void CnPoseRPC() {
        Animator.Play("CnPoseStart");
    }

    public void CallJump() {
        //MechController.Jump();
    }
}