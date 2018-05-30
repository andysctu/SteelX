using System;
using UnityEngine;
using System.Collections.Generic;

public class AOESkillBehaviour : MonoBehaviour, ISkill {
    private int playerLayer = 8;
    private SkillController SkillController;
    private MechCombat mcbt;
    private Sounds Sounds;
    private Crosshair Crosshair;
    private PhotonView player_pv;
    private AOESkillConfig config;
    private Transform[] targets;

    private void Start() {
        InitComponent();
    }

    private void InitComponent() {
        player_pv = GetComponent<PhotonView>();
        SkillController = GetComponent<SkillController>();
        Crosshair = SkillController.GetCamera().GetComponent<Crosshair>();
        mcbt = GetComponent<MechCombat>();

        Transform CurrentMech = transform.Find("CurrentMech");
        Sounds = CurrentMech.GetComponent<Sounds>();
    }

    public void SetConfig(int skill_num) {
        this.config = (AOESkillConfig)(SkillController.GetSkillConfig(skill_num));
    }

    public void SetConfig(SkillConfig config) {
        this.config = (AOESkillConfig)config;
    }

    public void Use(int num) {
        int[] target_pvIDs = DectectTargetInSphere(transform.position, config.radius, config.detectRange);

        player_pv.RPC("CastSkill", PhotonTargets.All, target_pvIDs, num);
    }

    private int[] DectectTargetInSphere(Vector3 center, int radius, int detectRange) {
        Collider[] hits ;
        hits = Physics.OverlapSphere(center, radius, 1<<playerLayer);

        List<int> target_pvIDs = new List<int>();

        foreach(Collider hit in hits) {
            Debug.Log("hit : " + hit.transform.name);
            PhotonView targetPV = hit.transform.root.GetComponent<PhotonView>();

            if (hit.transform.root == transform.root)
                continue;

            if (GameManager.isTeamMode) {
                if(player_pv.owner.GetTeam() != targetPV.owner.GetTeam()) {
                    if (hit.tag != "Shield") {
                        target_pvIDs.Add(targetPV.viewID);
                    }
                }
            } else {
                if (hit.tag != "Shield") {
                    target_pvIDs.Add(hit.transform.root.GetComponent<PhotonView>().viewID);
                }
            }
        }
        return target_pvIDs.ToArray();
    }

    [PunRPC]
    void CastSkill(int[] target_pvIDs, int skill_num) {
        SetConfig(skill_num);

        foreach(int target_pvID in target_pvIDs) {
            PhotonView target_pv = PhotonView.Find(target_pvID);
            
            if (target_pv == null) continue;

            if (player_pv.isMine) {
                target_pv.RPC("OnHit", PhotonTargets.All, config.damage, player_pv.viewID, SkillController.GetSkillName(skill_num), false);
            }

            Transform target = target_pv.transform;
            Debug.Log("Attaching effects on target : " + target.name);
            SkillController.AttachEffectsOnTarget(target, skill_num);
            //SetEffectsTarget(target);
        }
            //Play skill animation
            SkillController.PlayPlayerAnimation(skill_num);

            SkillController.PlayWeaponAnimation(skill_num);

            //Play skill sound
            SkillController.PlaySkillSound(skill_num);
    }

    public Transform[] GetCurrentOnSkillTargets() {
        return targets;
    }

    /*
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 20);
    }*/

    /*
    void SetEffectsTarget(Transform target) {
        if (mcbt.GetCurrentWeaponOffset() == 0)
            foreach (RequireSkillInfo g in SkillController.weaponEffects_1) {
                g.SetTarget(target);
            } else
            foreach (RequireSkillInfo g in SkillController.weaponEffects_2) {
                g.SetTarget(target);
            }
    }*/
}