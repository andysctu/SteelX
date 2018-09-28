using UnityEngine;

public class SingleTargetSkillBehaviour : MonoBehaviour, ISkill {
    private SkillController SkillController;
    private MechCombat mcbt;
    private Sounds Sounds;
    private CrosshairController Crosshair;
    private PhotonView player_pv;
    private Transform target;
    private int TerrainLayer;

    private void Start() {
        InitComponent();
    }

    private void InitComponent() {
        player_pv = GetComponent<PhotonView>();
        SkillController = GetComponent<SkillController>();
        Crosshair = SkillController.GetCamera().GetComponent<CrosshairController>();
        mcbt = GetComponent<MechCombat>();
        TerrainLayer = LayerMask.GetMask("Terrain");

        Transform CurrentMech = transform.Find("CurrentMech");
        Sounds = CurrentMech.GetComponent<Sounds>();
    }

    public bool Use(int skill_num) {
        //Get the config
        SingleTargetSkillConfig config = (SingleTargetSkillConfig)(SkillController.GetSkillConfig(skill_num));

        //Detect target
        Transform target = Crosshair.DectectTarget(config.SingleTargetSkillParams.crosshairRadius, config.SingleTargetSkillParams.detectRange, 0, false);

        if (target != null) {
            PhotonView target_pv = target.GetComponent<PhotonView>();

            RaycastHit hit;
            Physics.Raycast(target.position, - Vector3.up, out hit, Mathf.Infinity, TerrainLayer);

            Vector3 target_onGroundPos = hit.point;

            if(Mathf.Abs( hit.point.y - transform.position.y) > 5) {//TODO : improve this
                //target is on something higher or lower
                player_pv.RPC("CastSingleTargetSkill", PhotonTargets.All, -1, 0, Vector3.zero, Vector3.zero);
                return true;
            }

            //Move to the right position
            Vector3 posProj = new Vector3(transform.position.x, 0 , transform.position.z);
            Vector3 target_posProj = new Vector3(target_onGroundPos.x, 0, target_onGroundPos.z);

            float distanceBetween = (posProj - target_posProj).magnitude;

            Vector3 idealPos = (posProj - target_posProj).normalized * (config.SingleTargetSkillParams.distance - distanceBetween) + transform.position;

            //check if the point is inside wall
            bool goBackToOriginalPoint = false;
            Vector3 orgPoint = transform.position;
            if (Physics.CheckSphere(idealPos + new Vector3(0, 5, 0), 1, TerrainLayer)) {
                goBackToOriginalPoint = true;
            }

            GetComponent<CharacterController>().Move((posProj - target_posProj).normalized * (config.SingleTargetSkillParams.distance - distanceBetween));

            player_pv.RPC("CastSingleTargetSkill", PhotonTargets.All, target_pv.viewID, skill_num, idealPos, transform.forward);

            if (goBackToOriginalPoint) {
                transform.position = orgPoint;
            }
        } else {
            player_pv.RPC("CastSingleTargetSkill", PhotonTargets.All, -1, 0, Vector3.zero, Vector3.zero);
        }
        return true;
    }

    [PunRPC]
    private void CastSingleTargetSkill(int targetpv_id, int skill_num, Vector3 start_pos, Vector3 direction) {
        if (targetpv_id != -1) {
            SingleTargetSkillConfig config = (SingleTargetSkillConfig)(SkillController.GetSkillConfig(skill_num));

            PhotonView target_pv = PhotonView.Find(targetpv_id);
            if (target_pv == null) { Debug.Log("Can't find target photonView when using skill"); return; }
            SkillController target_SkillController = target_pv.GetComponent<SkillController>();

            target = target_pv.transform;            
            //Attach effects on target
            SetEffectsInfo(target, skill_num);

            //rotate target to the right direction
            float angle = Vector3.Angle(direction, target.transform.forward);

            target.transform.position = start_pos + direction * config.SingleTargetSkillParams.distance;
 
            target.transform.LookAt((angle > 90) ? start_pos + new Vector3(0, 5, 0) : start_pos + new Vector3(0, 5, 0) + direction * 9999);            
            target.transform.rotation = Quaternion.Euler(0, target.transform.rotation.eulerAngles.y, 0);

            //Play target on skill animation
            if (target_SkillController != null) {
                target_SkillController.SetSkillUser(transform);
                target_SkillController.TargetOnSkill((angle > 90) ? config.GetTargetFrontAnimation() : config.GetTargetBackAnimation(), config.GetTargetCamAnimation(), start_pos);
            }

            //Sync the start position
            transform.position = start_pos;

            //Sync the start rotation
            transform.LookAt(target.position + new Vector3(0, 5, 0));
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            //Play skill animation
            SkillController.PlayPlayerAnimation(skill_num);

            SkillController.PlayWeaponAnimation(skill_num);

            //Play skill sound
            SkillController.PlaySkillSound(skill_num);

            //SkillController.PlayPlayerEffects(skill_num);
            foreach (GameObject g in config.GetPlayerEffects()) {
                Instantiate(g, transform.position + new Vector3(0, 5, 0), transform.rotation, transform);
                g.SetActive(true);
            }

            //apply damage
            if (target_pv.isMine) {
                target_pv.RPC("OnHit", PhotonTargets.All, config.GeneralSkillParams.damage, player_pv.viewID, SkillController.GetSkillName(skill_num), false);
            }
        } else {//target is null => cancel skill
            SkillController.PlayCancelSkill();
        }
    }

    private void SetEffectsInfo(Transform target, int skill_num) {
        foreach (RequireSkillInfo g in SkillController.RequireInfoSkills[skill_num]) {
            g.SetTarget(target);
        }
    }
}