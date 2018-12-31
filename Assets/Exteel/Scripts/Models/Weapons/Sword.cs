using StateMachine;
using StateMachine.Attack;
using UnityEngine;
using XftWeapon;

namespace Weapons
{
    public class Sword : MeleeWeapon
    {
        private XWeaponTrail _weaponTrail;
        private AudioClip[] _slashSounds = new AudioClip[4];

        private bool _receiveNextSlash, _isAnotherWeaponSword, _prepareToAttack, _isAttacking;
        private int _curCombo;
        private float _attackStartTime;

        private float _attackLength = 0.8f, _finalAttackLength = 1.3f, _multiSwordAttackLength = 0.8f, _airAttackLength = 0.6f;//TODO : remake this part

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, pos, handTransform, Cbt, Animator);
            InitComponents();

            CheckIsAnotherWeaponSword();
        }

        private void InitComponents(){
            FindTrail(weapon);
        }

        private void CheckIsAnotherWeaponSword(){
            _isAnotherWeaponSword = (AnotherWeaponData.GetWeaponType() == typeof(Sword));
        }

        protected override void LoadSoundClips(){
            _slashSounds = ((SwordData) data).slash_sound;
        }

        private void FindTrail(GameObject weapon){
            _weaponTrail = weapon.GetComponentInChildren<XWeaponTrail>(true);
            if (_weaponTrail != null) _weaponTrail.Deactivate();
        }

        public override void HandleCombat(usercmd cmd){
            if (Mctrl.Grounded)Cbt.CanMeleeAttack = true;

            if (_prepareToAttack){// combo attack
                if (Time.time > _attackStartTime){
                    _prepareToAttack = false;
                    AttackStartAction();
                }
            }else if (Time.time > _attackStartTime + _attackLength && _isAttacking) {//todo check this
                _isAttacking = false;
                AttackEndAction();
            }

            if (!(Hand == LEFT_HAND ? cmd.buttons[(int)UserButton.LeftMouse] : cmd.buttons[(int)UserButton.RightMouse]) || IsOverHeat()){
                return;
            }

            if (Time.time - TimeOfLastUse >= 1 / Rate && !_prepareToAttack) {
                if (!_receiveNextSlash || !Cbt.CanMeleeAttack){
                    return;
                }

                if (_curCombo == 3 && !_isAnotherWeaponSword) return;
                if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

                Cbt.CanMeleeAttack = false;
                _receiveNextSlash = false;
                TimeOfLastUse = Time.time;

                if (_curCombo >= 1) { // combo attack
                    _prepareToAttack = true;
                    _attackStartTime += _attackLength * 0.8f;//todo : check this
                } else {//first attack
                    _attackStartTime = Time.time;
                    AttackStartAction();
                }
                _isAttacking = true;

                //IncreaseHeat(data.HeatIncreaseAmount);
            }
        }

        protected virtual void AttackStartAction(){
            AnimationEventController.Slash(Hand, _curCombo);

            Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), (Mctrl.IsJumping) ? 22 : 15, (Mctrl.IsJumping) ? 0.45f : 0.75f);

            //Play slash sound
            if (_slashSounds != null && _slashSounds[_curCombo] != null) AudioSource.PlayClipAtPoint(_slashSounds[_curCombo], weapon.transform.position);

            //TODO : master check this
            IsFiring = true;

            //If not final slash
            if (_curCombo != 4) _receiveNextSlash = true;

            MeleeAttack(Hand);//todo : check this

            Cbt.CanMeleeAttack = !Mctrl.IsJumping;
            Cbt.SetMeleePlaying(true);
            Mctrl.ResetCurBoostingSpeed();

            _curCombo++;
        }

        protected virtual void AttackEndAction() {
            //state machine
            if(Mctrl.Grounded){
                IsFiring = false;
                _receiveNextSlash = true;
                _curCombo = 0;
            }

            //normal state
            if (!Mctrl.Grounded) {
                IsFiring = false;
                _receiveNextSlash = true;
                _curCombo = 0;
                Cbt.SetMeleePlaying(false);
            } else {
                Cbt.CanMeleeAttack = true;
            }

            Mctrl.CallLockMechRot(false);
        }

        public override void HandleAnimation(){
        }

        protected override void ResetMeleeVars(){
            //this is called when on skill or init
            base.ResetMeleeVars();
            _curCombo = 0;

            _receiveNextSlash = true;

            Cbt.SetMeleePlaying(false);
            MechAnimator.SetBool("Slash", false);
        }

        public void EnableWeaponTrail(bool b){
            if (_weaponTrail == null) return;

            if (b){
                _weaponTrail.Activate();
            } else{
                _weaponTrail.StopSmoothly(0.1f);
            }
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);

            if (enter){
                _receiveNextSlash = false;
            } else{
                _receiveNextSlash = true;
                Cbt.CanMeleeAttack = true;
            }
        }

        public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield){

            if (isShield){
                if (targetWeapon != null){
                    targetWeapon.PlayOnHitEffect();
                }
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