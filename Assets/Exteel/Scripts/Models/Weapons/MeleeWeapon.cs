using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class MeleeWeapon : Weapon {
    protected SlashDetector SlashDetector;    
    protected List<Transform> targets_in_collider;
    protected ParticleSystem HitEffectPrefab;

    private const int DetectShieldMaxDistance = 30;//the ray which checks if hitting shield max distance
    public float threshold;

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);
        InitComponents();
        ResetMeleeVars();

        EnableDetector(mcbt.photonView.isMine);
    }

    protected override void InitComponents() {
        base.InitComponents();
        SlashDetector = mcbt.GetComponentInChildren<SlashDetector>();
        HitEffectPrefab = ((MeleeWeaponData)data).hitEffect;
    }

    protected virtual void ResetMeleeVars() {
        if (!mcbt.photonView.isMine) return;

        MechAnimator.SetBool("OnMelee", false);        
    }

    //Enable  Detector
    protected virtual void EnableDetector(bool b) {
        if(SlashDetector != null)SlashDetector.EnableDetector(b);
    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);

        if(enter)
            ResetMeleeVars();
    }

    public override void OnDestroy() {        
        base.OnDestroy();
        ResetMeleeVars();
    }

    public override void OnSwitchedWeaponAction() {
        ResetMeleeVars();
    }

    public virtual void MeleeAttack(int hand) {
        if ((targets_in_collider = SlashDetector.getCurrentTargets()).Count != 0) {
            int damage = data.damage;
            string weaponName = data.weaponName;

            foreach (Transform target in targets_in_collider) {
                if (target == null) {continue;}

                //cast a ray to check if hitting shield
                bool isHitShield = false, isTerrainBlocksTheWay = false;
                RaycastHit[] hitpoints;
                Transform t = target;

                hitpoints = Physics.RaycastAll(mcbt.transform.position + new Vector3(0, 5, 0), target.transform.root.position - mcbt.transform.position, DetectShieldMaxDistance, PlayerAndTerrainMask).OrderBy(h => h.distance).ToArray();
                foreach (RaycastHit hit in hitpoints) {
                    if (hit.transform.root == target) {
                        if (hit.collider.transform.tag == "Shield") {
                            isHitShield = true;
                            t = hit.collider.transform;
                        }
                        break;
                    }else if(hit.transform.gameObject.layer == TerrainLayer) {//Terrain blocks the way
                        isTerrainBlocksTheWay = true;
                        break;
                    }
                }

                if (isTerrainBlocksTheWay) {
                    continue;
                }

                if (isHitShield) {
                    ShieldActionReceiver ShieldActionReceiver = t.transform.parent.GetComponent<ShieldActionReceiver>();
                    int shieldPos = ShieldActionReceiver.GetPos();//which hand holds the shield?
                    target.GetComponent<PhotonView>().RPC("ShieldOnHit", PhotonTargets.All, damage, PhotonNetwork.player, pos, shieldPos, (int)Shield.DefendType.Melee);
                } else {
                    target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PhotonNetwork.player, pos);
                }

                if (target.GetComponent<Combat>().CurrentHP <= 0) {
                    target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, mcbt.GetCamera());
                } else {
                    if (isHitShield)
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, mcbt.GetCamera());
                    else
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, mcbt.GetCamera());
                }

                //increase SP
                //SkillController.IncreaseSP(data.SPincreaseAmount);
            }
        }
    }
}