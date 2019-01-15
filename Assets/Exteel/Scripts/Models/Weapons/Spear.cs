using UnityEngine;

namespace Weapons
{
    public class Spear : MeleeWeapon
    {
        private AudioClip _smashSound;
        private bool _receiveNextSMash, _prepareToAttack;
        //_receiveNextSlash : Is waiting for the next combo (button)
        //_prepareToAttack : process next combo when current one end

        private float _curComboEndTime;//AttackEnd is called when time exceeds curComboEndTime
        private float _instantMoveDistanceInAir = 25, _instantMoveDistanceOnGround = 19;

        private float[] _attackAnimationLengths;
        private enum SmashType { NormalAttack, AirAttack };

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat cbt, Animator animator){
            base.Init(data, pos, handTransform, cbt, animator);
            InitComponents();

            _attackAnimationLengths = (float[])((SpearData) data).AnimationLengths.Clone(); ;
        }

        private void InitComponents(){
            //FindTrail(weapon);
        }

        protected override void LoadSoundClips(){
            _smashSound = ((SpearData) data).SmashSound;
        }

        //private void FindTrail(GameObject weapon) {
        //    WeaponTrail = weapon.GetComponentInChildren<XWeaponTrail>(true);
        //    if (WeaponTrail != null) WeaponTrail.Deactivate();
        //}

        public override void HandleCombat(usercmd cmd) {
            if (Mctrl.Grounded) Cbt.CanMeleeAttack = true;

            if (_prepareToAttack) {
                if (Time.time > _curComboEndTime) {
                    _prepareToAttack = false;
                    AttackStartAction();
                }
            } else if (IsFiring && Time.time > _curComboEndTime) {
                AttackEndAction();
            }

            if (!(Hand == LEFT_HAND ? cmd.buttons[(int)UserButton.LeftMouse] : cmd.buttons[(int)UserButton.RightMouse]) || IsOverHeat()) {
                return;
            }

            if (Time.time - TimeOfLastUse >= 1 / Rate){
                if (!Cbt.CanMeleeAttack)return;

                if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

                _prepareToAttack = true;
                //IncreaseHeat(data.HeatIncreaseAmount);
            }
        }

        protected virtual void AttackStartAction() {
            IsFiring = true;
            TimeOfLastUse = Time.time;
            _curComboEndTime = Time.time + ((Mctrl.IsJumping) ? _attackAnimationLengths[(int)SmashType.AirAttack] : _attackAnimationLengths[(int)SmashType.NormalAttack]);

            Smash(Hand);
            MeleeAttack(Hand);//todo : check this
            Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), (Mctrl.IsJumping) ? _instantMoveDistanceInAir : _instantMoveDistanceOnGround, Mctrl.IsJumping ? _attackAnimationLengths[(int)SmashType.AirAttack] * 0.8f : _attackAnimationLengths[(int)SmashType.NormalAttack]);

            if (_smashSound != null) WeaponAudioSource.PlayOneShot(_smashSound);

            Cbt.CanMeleeAttack = Mctrl.Grounded;
            Mctrl.ResetCurBoostingSpeed();
        }

        protected virtual void AttackEndAction() {//This is being called once after combo end
            IsFiring = false;
            Mctrl.LockMechRot(false);
        }

        protected override void ResetMeleeVars(){
            //this is called when on skill or init
            base.ResetMeleeVars();

            Cbt.CanMeleeAttack = true;
            MechAnimator.SetBool("SmashL", false);
            MechAnimator.SetBool("SmashR", false);
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

        public void Smash(int hand) {
            MechAnimator.SetBool(hand == LEFT_HAND ? AnimatorVars.SmashLHash : AnimatorVars.SmashRHash, true);
        }
    }
}