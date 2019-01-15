using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Weapons
{
    public abstract class MeleeWeapon : Weapon
    {
        protected SlashDetector SlashDetector;
        protected List<Transform> TargetsInCollider;
        protected ParticleSystem HitEffectPrefab;

        private const int DetectShieldMaxDistance = 50; //the ray which checks if hitting shield max distance

        public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, hand, handTransform, Cbt, Animator);
            InitComponents();
            ResetMeleeVars();

            EnableDetector(Cbt.photonView.isMine);
        }

        private void InitComponents(){
            SlashDetector = Cbt.GetComponentInChildren<SlashDetector>();
            HitEffectPrefab = ((MeleeWeaponData) data).hitEffect;
        }

        protected virtual void ResetMeleeVars(){
            if (!Cbt.photonView.isMine) return;

            MechAnimator.SetBool("OnMelee", false);
        }

        protected virtual void ResetArmAnimatorState(){
            MechAnimator.Play("Idle", 1 + Hand);
        }

        //Enable  Detector
        protected virtual void EnableDetector(bool b){
            if (SlashDetector != null) SlashDetector.EnableDetector(b);
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);

            if (enter) ResetMeleeVars();
        }

        public override void OnDestroy(){
            ResetMeleeVars();
            base.OnDestroy();
        }

        public override void OnWeaponSwitchedAction(bool isThisWeaponActivated){
            if (isThisWeaponActivated){
                ResetMeleeVars();
                ResetArmAnimatorState();
            }
        }

        public virtual void MeleeAttack(int hand){//TODO : check this
            if ((TargetsInCollider = SlashDetector.getCurrentTargets()).Count != 0){
                int damage = data.damage;

                foreach (Transform target in TargetsInCollider){
                    if (target == null)continue;

                    //cast a ray to check if hitting shield
                    bool isHitShield = false, isTerrainBlocksTheWay = false;
                    Transform t = target;

                    var hitpoints = Physics.RaycastAll(Cbt.transform.position + new Vector3(0, 5, 0), 
                        target.transform.root.position - Cbt.transform.position, DetectShieldMaxDistance, PlayerAndTerrainMask).OrderBy(h => h.distance).ToArray();

                    foreach (RaycastHit hit in hitpoints){
                        if (hit.transform.root == target){
                            if (hit.collider.transform.tag[0] == 'S'){//todo : improve this
                                isHitShield = true;
                                t = hit.collider.transform;
                            }

                            break;
                        } else if (hit.transform.gameObject.layer == TerrainLayer){
                            //Terrain blocks the way
                            isTerrainBlocksTheWay = true;
                            break;
                        }
                    }

                    if (isTerrainBlocksTheWay){
                        continue;
                    }

                    if (isHitShield){
                        ShieldActionReceiver ShieldActionReceiver = t.transform.parent.GetComponent<ShieldActionReceiver>();
                        int shieldPos = ShieldActionReceiver.GetPos(); //which hand holds the shield?
                        target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PlayerPv.viewID, WeapPos, shieldPos);
                    } else{
                        target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PlayerPv.viewID, WeapPos, -1);
                    }

                    if (target.GetComponent<Combat>().CurrentHP <= 0){
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, Cbt.GetCamera());
                    } else{
                        if (isHitShield)
                            target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, Cbt.GetCamera());
                        else
                            target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, Cbt.GetCamera());
                    }

                    //TODO : improve this
                    Cbt.IncreaseSP(data.SpIncreaseAmount);
                }
            }
        }
    }
}