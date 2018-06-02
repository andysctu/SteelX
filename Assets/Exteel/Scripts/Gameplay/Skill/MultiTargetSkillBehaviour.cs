using UnityEngine;
using System.Collections.Generic;

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

    public void Use(int skill_num) {
        //Detect target
        MultiTargetSkillConfig config = (MultiTargetSkillConfig)(SkillController.GetSkillConfig(skill_num));
        Transform[] targets_in_range = Crosshair.DectectMultiTargets(config.crosshairRadius, config.detectRange, false); //temp

        if (targets_in_range != null && targets_in_range.Length > 0) {
            List<int> target_pvIDs = new List<int>();

            foreach (Transform t in targets_in_range) {
                PhotonView target_pv = t.GetComponent<PhotonView>();
                target_pvIDs.Add(target_pv.viewID);
            }
            player_pv.RPC("CastMultiTargetSkill", PhotonTargets.All, target_pvIDs.ToArray(), skill_num);
        }
        //if no target => do nothing
    }

    [PunRPC]
    void CastMultiTargetSkill(int[] target_pvIDs, int skill_num) {
        if (target_pvIDs != null) {
            MultiTargetSkillConfig config = (MultiTargetSkillConfig)(SkillController.GetSkillConfig(skill_num));

            Debug.Log("Called play " + "skill_" + skill_num);
            List<Transform> targets = new List<Transform>();

            foreach(int target_pvID in target_pvIDs) {
                PhotonView target_pv = PhotonView.Find(target_pvID);
                if (target_pv == null) { Debug.Log("Can't find target photonView"); continue; }

                targets.Add(target_pv.transform);

                //TODO : improve this check
                if (config.GetPlayerAniamtion(true) == null) {//instantiate immdiately
                    GameObject g = Instantiate(config.GetbulletParticle(), transform.position + new Vector3(0, 5, 0), Quaternion.identity);
                    BulletTrace bulletTrace = g.GetComponent<BulletTrace>();
                    bulletTrace.ShowHitOnBulletCollision(true);
                    bulletTrace.SetCamera(cam);
                    bulletTrace.SetSpeed(150);
                    bulletTrace.SetTarget(target_pv.transform, false);
                    bulletTrace.SetStartTransform(transform);//temp

                    //target_pv.GetComponent<HUD>().DisplayHit(cam);
                }

                if (target_pv.isMine) target_pv.RPC("OnHit", PhotonTargets.All, config.GeneralSkillParams.damage, player_pv.viewID, SkillController.GetSkillName(skill_num), false);
            }


            //use cam ?
            /*
            if (skillcam != null)
                skillcam.SetTarget(transform.root);    
            */

            //play anim ?
            //Play skill animation
            //SkillController.PlayPlayerAnimation(skill_num);

            //SkillController.PlayWeaponAnimation(skill_num);

            //Play skill sound
            SkillController.PlaySkillSound(skill_num);
        } else {//target is null => do nothing
            Debug.Log("should not go here");
            //SkillController.PlayCancelSkill();
        }
    }

    void SetEffectsTarget(Transform target) {//TODO : improve this
        if (mcbt.GetCurrentWeaponOffset() == 0)
            foreach (RequireSkillInfo g in SkillController.weaponEffects_1) {
                g.SetTarget(target);
            } else
            foreach (RequireSkillInfo g in SkillController.weaponEffects_2) {
                g.SetTarget(target);
            }
    }
}