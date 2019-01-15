using StateMachine;
using UnityEngine;

namespace Weapons
{
    public class Shield : Weapon
    {
        private Transform _effectEnd;
        private ShieldActionReceiver _shieldActionReceiver;
        private AudioClip _onHitSound;
        private ParticleSystem _onHitEffect, _overheatEffect;
        private int AtkAnimHash;

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, pos, handTransform, Cbt, Animator);
            InitComponents();
            InitAtkAnimHash();
            AddShieldActionReceiver();
            AttachEffects();
            ResetAnimationVars();
        }

        private void InitComponents(){
            _effectEnd = weapon.transform.Find("EffectEnd");
        }

        private void InitAtkAnimHash(){
            AtkAnimHash = (Hand == 0) ? Animator.StringToHash("AtkL") : Animator.StringToHash("AtkR");
        }

        private void AddShieldActionReceiver(){
            _shieldActionReceiver = weapon.AddComponent<ShieldActionReceiver>();
            _shieldActionReceiver.SetPos(WeapPos);
        }

        private void AttachEffects(){
            //OnHitEffect
            _onHitEffect = Object.Instantiate(((ShieldData) data).OnHitEffect, _effectEnd);
            TransformExtension.SetLocalTransform(_onHitEffect.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

            //OverheatEffect
            _overheatEffect = Object.Instantiate(((ShieldData) data).OverheatEffect, weapon.transform);
            TransformExtension.SetLocalTransform(_overheatEffect.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
        }

        private void ResetAnimationVars(){
            IsFiring = false;
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            ResetAnimationVars();
        }

        public override void OnWeaponSwitchedAction(bool isThisWeaponActivated){
            ResetAnimationVars();
            if (isThisWeaponActivated){
                UpdateMechArmState();
            }
        }

        private void UpdateMechArmState(){
            MechAnimator.Play("SHS", 1 + Hand);
        }

        public override void HandleCombat(usercmd cmd) {
            if (!Input.GetKey(BUTTON) || IsOverHeat()){
                IsFiring = false;
                return;
            }

            if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

            IsFiring = true;
        }

        public override void HandleAnimation(){
            if (IsFiring){
                if (!MechAnimator.GetBool(AtkAnimHash)){
                    MechAnimator.SetBool(AtkAnimHash, true);
                }
            } else{
                if (MechAnimator.GetBool(AtkAnimHash)){
                    MechAnimator.SetBool(AtkAnimHash, false);
                }
            }
        }

        protected override void LoadSoundClips(){
            _onHitSound = ((ShieldData) data).OnHitSound;
        }

        public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield){
        }

        public override void OnHitAction(Combat shooter, Weapon shooterWeapon){
            IncreaseHeat(shooterWeapon.GetRawDamage() / 10); //TODO : improve this
        }

        public override void PlayOnHitEffect(){
            WeaponAudioSource.PlayOneShot(_onHitSound);

            _onHitEffect.Play();
        }

        public override void OnOverHeatAction(bool isOverHeat){
            if (_overheatEffect == null) return;

            if (isOverHeat){
                var main = _overheatEffect.main;
                main.duration = HeatBar.GetCooldownLength(WeapPos);

                _overheatEffect.Play();
            } else{
                _overheatEffect.Stop();
            }
        }

        public override int ProcessDamage(int damage, AttackType attackType){
            int newDmg = damage;
            float efficiencyCoeff = (IsOverHeat()) ? 1.5f : 1, efficiency = 1;

            switch (attackType){
                case AttackType.Melee:
                    efficiency = Mathf.Clamp(((ShieldData) data).defend_melee_efficiency * efficiencyCoeff, 0, 1);
                    newDmg = (int) (newDmg * efficiency);
                    break;
                case AttackType.Ranged:
                case AttackType.Cannon:
                case AttackType.Rocket:
                    efficiency = Mathf.Clamp(((ShieldData) data).defend_ranged_efficiency * efficiencyCoeff, 0, 1);
                    newDmg = (int) (newDmg * efficiency);
                    break;
                case AttackType.None:
                    break;
                default:
                    break;
            }

            return newDmg;
        }

        public override bool IsShield(){
            return true;
        }

        public override void OnStateCallBack(int type, MechStateMachineBehaviour state){
        }
    }
}