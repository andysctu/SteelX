using UnityEngine;

namespace Weapons
{
    public class Spear : MeleeWeapon
    {
        private AudioClip smashSound;

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, pos, handTransform, Cbt, Animator);
            InitComponents();
            //threshold = ((SpearData)data).threshold;

            //UpdateSmashAnimationThreshold();
        }

        private void InitComponents(){
            //FindTrail(weapon);
        }

        protected override void LoadSoundClips(){
            smashSound = ((SpearData) data).smash_sound;
        }

        //private void FindTrail(GameObject weapon) {
        //    WeaponTrail = weapon.GetComponentInChildren<XWeaponTrail>(true);
        //    if (WeaponTrail != null) WeaponTrail.Deactivate();
        //}

        public override void HandleCombat(){
            if (!Input.GetKeyDown(BUTTON) || IsOverHeat()){
                return;
            }

            if (Time.time - TimeOfLastUse >= 1 / Rate){
                if (!Cbt.CanMeleeAttack){
                    return;
                }

                if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

                Cbt.CanMeleeAttack = false;
                TimeOfLastUse = Time.time;

                IncreaseHeat(data.HeatIncreaseAmount);

                //Play Animation
                AnimationEventController.Smash(Hand);
            }
        }

        public override void HandleAnimation(){
        }

        protected override void ResetMeleeVars(){
            //this is called when on skill or init
            base.ResetMeleeVars();

            if (!Cbt.photonView.isMine) return;

            Cbt.CanMeleeAttack = true;
            Cbt.SetMeleePlaying(false);
        }

        //public void EnableWeaponTrail(bool b) {
        //    if (WeaponTrail == null) return;

        //    if (b) {
        //        WeaponTrail.Activate();
        //    } else {
        //        WeaponTrail.StopSmoothly(0.1f);
        //    }
        //}

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);

            if (enter){
            } else{
                Cbt.CanMeleeAttack = true;
            }
        }

        public override void OnStateCallBack(int type, MechStateMachineBehaviour state){
            switch ((StateCallBackType) type){
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

        private void OnAttackStateEnter(MechStateMachineBehaviour state){
            //other player will also execute this
            //((SmashState)state).SetThreshold(threshold);//the state is confirmed SmashState in mechCombat      

            WeaponAnimator.SetTrigger("Atk");

            //Play slash sound
            if (smashSound != null) AudioSource.PlayClipAtPoint(smashSound, weapon.transform.position);

            if (PlayerPv != null && PlayerPv.isMine){
                //TODO : master check this
                IsFiring = true;

                MeleeAttack(Hand);
            }
        }

        private void OnAttackStateMachineExit(MechStateMachineBehaviour state){
            IsFiring = false;
        }

        private void OnAttackStateExit(MechStateMachineBehaviour state){
            if (((SmashState) state).IsInAir()){
                IsFiring = false;
            }
        }

        private void UpdateSlashAnimationThreshold(){
            //threshold = ((SpearData)data).threshold;
        }

        public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield){

            if (isShield){
                if (targetWeapon != null) targetWeapon.PlayOnHitEffect();
            } else{
                //Apply slowing down effect
                if (data.Slowdown){
                    MechController mctrl = target.GetComponent<MechController>();
                    if (mctrl != null){
                        mctrl.SlowDown();
                    }
                }

                ParticleSystem p = Object.Instantiate(HitEffectPrefab, target.transform);
                TransformExtension.SetLocalTransform(p.transform, new Vector3(0, 5, 0));
            }

        }
    }
}