using System.Collections.Generic;
using UnityEngine;

public class MultiTargetSkillBehaviour : MonoBehaviour, ISkill {
    private SkillController SkillController;
    private MechCombat mcbt;
    private Camera cam;
    private Sounds Sounds;
    private Crosshair Crosshair;
    private PhotonView player_pv;
    private MultiTargetSkillConfig config;
    private Transform[] targets;

    private void Start() {
        InitComponent();
    }

    private void InitComponent() {
        player_pv = GetComponent<PhotonView>();
        SkillController = GetComponent<SkillController>();
        Crosshair = SkillController.GetCamera().GetComponent<Crosshair>();
        mcbt = GetComponent<MechCombat>();
        cam = SkillController.GetCamera();

        Transform CurrentMech = transform.Find("CurrentMech");
        Sounds = CurrentMech.GetComponent<Sounds>();
    }

    public bool Use(int skill_num) {
        //Detect target
        MultiTargetSkillConfig config = (MultiTargetSkillConfig)(SkillController.GetSkillConfig(skill_num));
        Transform[] targets_in_range = Crosshair.DectectMultiTargets(config.MultiTargetSkillParams.crosshairRadius, config.MultiTargetSkillParams.detectRange, false);

        if (targets_in_range != null && targets_in_range.Length > 0) {
            List<int> target_pvIDs = new List<int>();

            foreach (Transform t in targets_in_range) {
                PhotonView target_pv = t.GetComponent<PhotonView>();
                target_pvIDs.Add(target_pv.viewID);
            }
            player_pv.RPC("CastMultiTargetSkill", PhotonTargets.All, target_pvIDs.ToArray(), skill_num);
            return true;
        } else {
            //no target => do nothing
            return false;
        }
    }

    [PunRPC]
    private void CastMultiTargetSkill(int[] target_pvIDs, int skill_num) {
        if (target_pvIDs != null) {
            MultiTargetSkillConfig config = (MultiTargetSkillConfig)(SkillController.GetSkillConfig(skill_num));

            Debug.Log("Called play " + "skill_" + skill_num);
            List<Transform> targets = new List<Transform>();

            foreach (int target_pvID in target_pvIDs) {
                PhotonView target_pv = PhotonView.Find(target_pvID);
                if (target_pv == null) { Debug.Log("Can't find target photonView"); continue; }

                targets.Add(target_pv.transform);

                if (player_pv.isMine) target_pv.RPC("OnHit", PhotonTargets.All, config.GeneralSkillParams.damage, player_pv.viewID, SkillController.GetSkillName(skill_num), false);
            }

            SetEffectsTarget(targets.ToArray(), skill_num);

            if (config.GeneralSkillParams.IsDamageLessWhenUsing) {
                SkillController.PlayPlayerAnimation(skill_num);

                SkillController.PlayerBoosterAnimation(skill_num);
            } else {//instantiate immdiately in animation
                //only booster animation
                SkillController.PlayerBoosterAnimation(skill_num);
            }

            //Play skill sound
            SkillController.PlaySkillSound(skill_num);
        }
    }

    private void SetEffectsTarget(Transform[] targets, int skill_num) {//TODO : improve this
        foreach (RequireSkillInfo g in SkillController.RequireInfoSkills[skill_num]) {
            g.SetTargets(targets);
            
        }
    }
}