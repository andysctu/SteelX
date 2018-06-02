using System;
using UnityEngine;
using System.Collections.Generic;

public class AOESkillBehaviour : MonoBehaviour, ISkill {
    private int playerLayer = 8;
    private MechCombat mcbt;
    private SkillController SkillController;
    private Sounds Sounds;
    private PhotonView player_pv;
    private AOESkillConfig config;
    
    private void Start() {
        InitComponent();
    }

    private void InitComponent() {
        player_pv = GetComponent<PhotonView>();
        SkillController = GetComponent<SkillController>();
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
        int[] target_pvIDs = DectectTargetInSphere(transform.position, config.radius);

        player_pv.RPC("CastAOESkill", PhotonTargets.All, target_pvIDs, num);
    }

    private int[] DectectTargetInSphere(Vector3 center, int radius) {
        Collider[] hits ;
        hits = Physics.OverlapSphere(center, radius, 1<<playerLayer);

        List<int> target_pvIDs = new List<int>();

        foreach(Collider hit in hits) {
            PhotonView targetPV = hit.transform.root.GetComponent<PhotonView>();

            if (hit.transform.root == transform.root)
                continue;

            if (GameManager.isTeamMode) {
                if(player_pv.owner.GetTeam() != targetPV.owner.GetTeam()) {
                    if (hit.tag != "Shield") { //shield is on player layer 
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
    void CastAOESkill(int[] target_pvIDs, int skill_num) {
        List<Transform> targets = new List<Transform>();
        SetConfig(skill_num);

        foreach(int target_pvID in target_pvIDs) {
            PhotonView target_pv = PhotonView.Find(target_pvID);
            
            if (target_pv == null) continue;

            if (player_pv.isMine) {
                target_pv.RPC("OnHit", PhotonTargets.All, config.damage, player_pv.viewID, SkillController.GetSkillName(skill_num), false);
            }

            Transform target = target_pv.transform;
            targets.Add(target);
        }
            SetEffectsTarget(targets.ToArray());

            //Play skill animation
            SkillController.PlayPlayerAnimation(skill_num);
            
            SkillController.PlayWeaponAnimation(skill_num);

            //Play skill sound
            SkillController.PlaySkillSound(skill_num);
    }

    void SetEffectsTarget(Transform[] targets) {//TODO : improve this
        if (mcbt.GetCurrentWeaponOffset() == 0)
            foreach (RequireSkillInfo g in SkillController.weaponEffects_1) {
                g.SetTargets(targets);
            } else
            foreach (RequireSkillInfo g in SkillController.weaponEffects_2) {
                g.SetTargets(targets);
            }
    }

    void OnDrawGizmosSelected() {
        if(config == null)
            return;

            
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, config.radius);
    }
}