using UnityEngine;

public class SingleTargetSkillBehaviour : MonoBehaviour, ISkill {
    private SkillController SkillController;
    private Sounds Sounds;
    private Crosshair Crosshair;
    private PhotonView player_pv;
    private SingleTargetSkillConfig config;
    private Transform target;

    private void Start() {
        InitComponent();
    }

    private void InitComponent() {
        player_pv = GetComponent<PhotonView>();
        SkillController = GetComponent<SkillController>();
        Crosshair = SkillController.GetCamera().GetComponent<Crosshair>();

        Transform CurrentMech = transform.Find("CurrentMech");
        Sounds = CurrentMech.GetComponent<Sounds>();        
    }

    public void SetConfig(SkillConfig config) {
        this.config = (SingleTargetSkillConfig)config;
    }

    public void Use() {
        //Detect target
        Transform target = Crosshair.DectectTarget(config.crosshairRadius, config.detectRange, false); //temp
        
        //Cast skill
        if (target != null) {
            PhotonView target_pv = target.GetComponent<PhotonView>();
            //Move to the right position
            transform.position = (transform.position - target.position).normalized * config.distance + target.position;

            //Adjust rotation
            transform.LookAt(target.position + new Vector3(0, 5, 0));

            player_pv.RPC("CastSkill", PhotonTargets.All, target_pv.viewID, config.GetPlayerAniamtion().name, transform.position, transform.forward);
        } else {
            player_pv.RPC("CastSkill", PhotonTargets.All, -1, "", Vector3.zero, Vector3.zero);
        }
    }

    [PunRPC]
    void CastSkill(int targetpv_id, string skill_name, Vector3 start_pos, Vector3 direction) {
        if(targetpv_id != -1) {
            Debug.Log("Called play " + "skill_" + config.GetSkillNum());

            PhotonView target_pv = PhotonView.Find(targetpv_id);
            if (target_pv == null) {Debug.Log("Can't find target photonView when casting skill");return;}
            SkillController target_SkillController = target_pv.GetComponent<SkillController>();
            target = target_pv.transform;

            //Attach Effects on target
            if (target != null) SetEffectsTarget(target);

            //rotate target to the right direction
            target.transform.LookAt(transform.position + new Vector3(0, 5, 0));
            target.transform.rotation = Quaternion.Euler(0, target.transform.rotation.eulerAngles.y, 0);
            //Play target on skill animation
            if (target_SkillController != null)target_SkillController.TargetOnSkill(config.GetTargetAnimation());

            target_pv.RPC("OnHit", PhotonTargets.All, config.damage, player_pv.viewID, config.GetTargetAnimation().name, false);

            //Play skill animation
            SkillController.PlaySkill(config.GetSkillNum());

            SkillController.PlayWeaponAnimation(config.GetPlayerAniamtion().name);

            //Play skill sound
            PlaySkillSound();

        } else {//target is null => cancel skill 
            SkillController.PlayCancelSkill();
        }
    }

    private void PlaySkillSound() {
        if(config.GetSkillSound()!=null)Sounds.PlayClip(config.GetSkillSound());
    }

    public Transform GetCurrentOnSkillTarget() {
        return target;
    }

    void SetEffectsTarget(Transform target) {
        foreach(RequireSkillInfo g in config.weaponEffects_1) {
            g.SetTarget(target);
        }
        foreach (RequireSkillInfo g in config.weaponEffects_2) {
            g.SetTarget(target);
        }
    }
}