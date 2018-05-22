using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SingleTargetSkillBehaviour : MonoBehaviour, ISkill {
    private SkillController SkillController;
    private MechController MechController;
    private Camera cam,skillcam;
    private Crosshair Crosshair;
    private PhotonView photonView;
    private SingleTargetSkillConfig config;
    private Animator skillAnimtor, mainAnimator;
    private Transform target;

    private void Start() {
        InitComponent();
    }

    private void InitComponent() {
        skillAnimtor = GetComponent<Animator>();
        mainAnimator = transform.Find("CurrentMech").GetComponent<Animator>();
        MechController = GetComponent<MechController>();
        photonView = GetComponent<PhotonView>();
        SkillController = GetComponent<SkillController>();
        cam = SkillController.GetCamera();
        skillcam = transform.root.Find("SkillCam").GetComponent<Camera>();
        Crosshair = cam.GetComponent<Crosshair>();
    }

    public void SetConfig(SkillConfig config) {
        this.config = (SingleTargetSkillConfig)config;
    }

    public void Use() {
        //detect target
        Transform target = Crosshair.DectectTarget(config.crosshairRadius, config.detectRange, false); //temp
        PhotonView target_pv = ((target==null)? null : target.GetComponent<PhotonView>());

        //RPC if there is one
        if (target != null) {
            StartCoroutine(ReturnDefaultStateWhenEnd("skill_"+ config.GetSkillNum()));
            transform.position = (transform.position - target.position).normalized * config.distance + target.position;
            transform.LookAt(target.position + new Vector3(0, 5, 0));
            photonView.RPC("CastSkill", PhotonTargets.All, target_pv.viewID, config.GetPlayerAniamtion().name, transform.position, transform.forward);
            SkillController.PlayWeaponAnimation(config.GetPlayerAniamtion().name);//todo : rpc this
        } else {
            StartCoroutine(ReturnDefaultStateWhenEnd("Skill_Cancel_01"));//TODO : move to RPC
            skillAnimtor.Play("Skill_Cancel_01");
        }
    }

    IEnumerator ReturnDefaultStateWhenEnd(string stateToWait) {
        SwitchToSkillCam(true);
        SkillController.OnSkill(true);
        yield return new WaitForSeconds(0.2f);//TODO : remake this logic

        yield return new WaitWhile(() => skillAnimtor.GetCurrentAnimatorStateInfo(0).IsName(stateToWait));
        skillcam.enabled = false;
        cam.enabled = true;
        SkillController.OnSkill(false);
        SwitchToSkillCam(false);
    }

    [PunRPC]
    void CastSkill(int targetpv_id, string skill_name, Vector3 start_pos, Vector3 direction) {
        PhotonView pv = PhotonView.Find(targetpv_id);
        SkillController target_SkillController = pv.GetComponent<SkillController>();

        target = pv.transform;

        //MechCombat target_mcbt = 
        //play target on skill animation
        if (target_SkillController != null)
            target_SkillController.TargetOnSkill(config.GetTargetAnimation());

        if (pv != null && pv.isMine) {
            //play on hit animation

        }
        Debug.Log("call play " + "skill_" + config.GetSkillNum());
        skillAnimtor.Play("skill_"+config.GetSkillNum());

        //play target on skill animation
        if (target_SkillController != null)
            target_SkillController.TargetOnSkill(config.GetTargetAnimation());
    }

    public Transform GetCurrentOnSkillTarget() {
        return target;
    }

    private void SwitchToSkillCam(bool b) {
        skillcam.enabled = b;
        cam.enabled = !b;
    }
}