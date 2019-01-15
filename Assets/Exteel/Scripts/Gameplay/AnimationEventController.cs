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

    public void CallShowTrailL(int show) {
        Sword sword = bm.Weapons[MechCombat.GetCurrentWeaponOffset()] as Sword;
        if(sword != null)sword.EnableWeaponTrail(show == 1);
    }

    public void CallShowTrailR(int show) {
        Sword sword = bm.Weapons[MechCombat.GetCurrentWeaponOffset()+1] as Sword;
        if (sword != null)sword.EnableWeaponTrail(show == 1);
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