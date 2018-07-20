using System.Collections;
using UnityEngine;

public class DroneCombat : Combat {
    public Transform[] Hands;

    [SerializeField] private SkillController SkillController;
    [SerializeField] private LayerMask Terrain;
    private int default_layer = 0, player_layer = 8;
    private EffectController EffectController;
    private bool onSkill = false, onSkillMoving = false;
    private float instantMoveSpeed, curInstantMoveSpeed;
    private Vector3 instantMoveDir;
    private CharacterController CharacterController;
    private float TeleportMinDistance = 3f;
    
    public Transform Shield;

    private void Awake() {
        if (SkillController != null) SkillController.OnSkill += OnSkill;
    }

    private void Start() {
        CurrentHP = MAX_HP;
        EffectController = GetComponent<EffectController>();
        findGameManager();
        EffectController.RespawnEffect();
        CharacterController = GetComponent<CharacterController>();
        gm.RegisterPlayer(photonView.viewID);
    }

    [PunRPC]
    public override void OnHit(int d, int shooter_viewID, string weapon, bool isSlowDown = false) {
        CurrentHP -= d;

        if (CheckIsSwordByStr(weapon)) {
            EffectController.SlashOnHitEffect(false, 0);
        }

        if (CurrentHP <= 0) {
            DisableDrone();
            //gm.RegisterKill(shooter_viewID, photonView.viewID);
        }
    }

    [PunRPC]
    public void ShieldOnHit(int d, int shooter_viewID, int hand, string weapon) {
        CurrentHP -= d;

        if (CheckIsSwordByStr(weapon)) {
            EffectController.SlashOnHitEffect(true, hand);
        } else if (CheckIsSpearByStr(weapon)) {
            EffectController.SmashOnHitEffect(true, hand);
        }

        if (CurrentHP <= 0) {
            DisableDrone();
            gm.RegisterKill(shooter_viewID, photonView.viewID);
        }
    }

    private void Update() {
        if (onSkillMoving) {
            InstantMove();
        }
    }

    public void SkillSetMoving(Vector3 v) {
        onSkillMoving = true;
        instantMoveSpeed = v.magnitude;
        instantMoveDir = v;
        curInstantMoveSpeed = instantMoveSpeed;
    }

    private void InstantMove() {
        instantMoveSpeed /= 1.6f;//1.6 : decrease coeff.

        CharacterController.Move(instantMoveDir * instantMoveSpeed);

        //cast a ray downward to check if not jumping but not grounded => if so , directly teleport to ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Terrain)) {
            if (Vector3.Distance(hit.point, transform.position) >= TeleportMinDistance && !Physics.CheckSphere(hit.point + new Vector3(0, 2.1f, 0), CharacterController.radius, Terrain)) {
                transform.position = hit.point;
            }
        }
    }
    [PunRPC]
    private void KnockBack(Vector3 dir, float length) {
        transform.position += dir * length;
    }

    public void Skill_KnockBack(float length) {
        Transform skillUser = SkillController.GetSkillUser();

        onSkillMoving = true;
        SkillSetMoving((skillUser != null) ? (transform.position - skillUser.position).normalized * length : -transform.forward * length);
    }

    private void DisableDrone() {
        gameObject.layer = default_layer;
        StartCoroutine(DisableDroneWhenNotOnSkill());
    }

    private IEnumerator DisableDroneWhenNotOnSkill() {
        yield return new WaitWhile(() => onSkill);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = false;
        }
        StartCoroutine(RespawnAfterTime(2));
        OnMechEnabled(false);
    }

    private void EnableDrone() {
        OnMechEnabled(true);
        EffectController.RespawnEffect();
        gameObject.layer = player_layer;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = true;
        }
        CurrentHP = MAX_HP;
    }

    private void OnSkill(bool b) {
        onSkill = b;
    }

    private IEnumerator RespawnAfterTime(int time) {
        yield return new WaitForSeconds(time);
        EnableDrone();
    }

    private bool CheckIsSwordByStr(string name) {
        return name.Contains("SHL");
    }
    private bool CheckIsSpearByStr(string name) {
        return name.Contains("ADR");
    }
}