using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class MeleeWeapon : Weapon {
    protected SlashDetector SlashDetector;    
    protected List<Transform> targets_in_collider;
    protected ParticleSystem HitEffectPrefab;

    public enum StateCallBackType { AttackStateEnter, AttackStateExit, AttackStateMachineExit }

    private const int DetectShieldMaxDistance = 50;//the ray which checks if hitting shield max distance
    public float threshold;

    public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator) {
        base.Init(data, hand, handTransform, Cbt, Animator);
        InitComponents();
        ResetMeleeVars();

        EnableDetector(Cbt.photonView.isMine);
    }

    private void InitComponents() {
        SlashDetector = Cbt.GetComponentInChildren<SlashDetector>();
        HitEffectPrefab = ((MeleeWeaponData)data).hitEffect;
    }

    protected virtual void ResetMeleeVars() {
        if (!Cbt.photonView.isMine) return;

        MechAnimator.SetBool("OnMelee", false);        
    }

    protected virtual void ResetArmAnimatorState() {
        MechAnimator.Play("Idle", 1 + hand);
    }

    //Enable  Detector
    protected virtual void EnableDetector(bool b) {
        if(SlashDetector != null)SlashDetector.EnableDetector(b);
    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);

        if(enter)ResetMeleeVars();
    }

    public override void OnDestroy() {        
        base.OnDestroy();
        ResetMeleeVars();
    }

    public override void OnSwitchedWeaponAction(bool b) {
        if (b) {
            ResetMeleeVars();
            ResetArmAnimatorState();
        }        
    }

    public virtual void MeleeAttack(int hand) {//TODO : check this again
        if ((targets_in_collider = SlashDetector.getCurrentTargets()).Count != 0) {
            int damage = data.damage;
            string weaponName = data.weaponName;

            foreach (Transform target in targets_in_collider) {
                if (target == null) {continue;}

                //cast a ray to check if hitting shield
                bool isHitShield = false, isTerrainBlocksTheWay = false;
                RaycastHit[] hitpoints;
                Transform t = target;

                hitpoints = Physics.RaycastAll(Cbt.transform.position + new Vector3(0, 5, 0), target.transform.root.position - Cbt.transform.position, DetectShieldMaxDistance, PlayerAndTerrainMask).OrderBy(h => h.distance).ToArray();
                foreach (RaycastHit hit in hitpoints) {
                    if (hit.transform.root == target) {
                        if (hit.collider.transform.tag[0] == 'S') {
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
                    target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, photonView.owner, photonView.viewID, weapPos, shieldPos);
                } else {
                    target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, photonView.owner, photonView.viewID, weapPos, -1);
                }

                if (target.GetComponent<Combat>().CurrentHP <= 0) {
                    target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, Cbt.GetCamera());
                } else {
                    if (isHitShield)
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, Cbt.GetCamera());
                    else
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, Cbt.GetCamera());
                }

                //TODO : improve this
                Cbt.IncreaseSP(data.SPincreaseAmount);
            }
        }
    }

    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        switch ((StateCallBackType)type) {
            case StateCallBackType.AttackStateEnter:
            OnAttackStateEnter(state);
            break;
            case StateCallBackType.AttackStateExit:
            OnAttackStateExit(state);
            break;
            case StateCallBackType.AttackStateMachineExit:
            OnAttackStateMachineExit(state);
            break;
        }
    }

    protected virtual void OnAttackStateEnter(MechStateMachineBehaviour state) { }
    protected virtual void OnAttackStateExit(MechStateMachineBehaviour state) { }
    protected virtual void OnAttackStateMachineExit(MechStateMachineBehaviour state) { }
}