using UnityEngine;
using System.Collections.Generic;

namespace Weapons
{
    public abstract class MeleeWeapon : Weapon
    {
        protected SlashDetector SlashDetector;
        protected ParticleSystem HitEffectPrefab;

        public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, hand, handTransform, Cbt, Animator);
            InitComponents();
            ResetMeleeVars();

            EnableDetector(Cbt.GetOwner().IsLocal || PhotonNetwork.isMasterClient);
        }

        private void InitComponents(){
            SlashDetector = Cbt.GetComponentInChildren<SlashDetector>();
            HitEffectPrefab = ((MeleeWeaponData) data).hitEffect;
        }

        protected virtual void ResetMeleeVars(){
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
    }
}