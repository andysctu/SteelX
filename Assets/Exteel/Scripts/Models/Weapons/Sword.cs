using UnityEngine;
using XftWeapon;

namespace Weapons
{
    public class Sword : MeleeWeapon
    {
        private XWeaponTrail _weaponTrail;
        private AudioClip[] _slashSounds = new AudioClip[4];

        private bool _receiveNextSlash, _isAnotherWeaponSword, _prepareToAttack;
        //_receiveNextSlash : Is waiting for the next combo (button)
        //_prepareToAttack : process next combo when current one end

        private int _curCombo;
        private float _curComboEndTime;//AttackEnd is called when time exceeds curComboEndTime
        private const float ReceiveNextAttackThreshold = 0.2f;//receiving the input after attacking start time + this
        private float _instantMoveDistanceInAir = 22, _instantMoveDistanceOnGround = 17;

        private float[] _attackAnimationLengths;
        private enum SlashType { FirstAttack, SecondAttack, ThirdAttack, MultiAttack, AirAttack};

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat cbt, Animator animator){
            base.Init(data, pos, handTransform, cbt, animator);
            _attackAnimationLengths = (float[])((SwordData) data).AttackAnimationLengths.Clone();

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
            _slashSounds = ((SwordData) data).SlashSounds;
        }

        private void FindTrail(GameObject weapon){
            _weaponTrail = weapon.GetComponentInChildren<XWeaponTrail>(true);
            if (_weaponTrail != null) _weaponTrail.Deactivate();
        }

        public override void HandleCombat(usercmd cmd){
            if (Mctrl.Grounded)Cbt.CanMeleeAttack = true;

            if (_prepareToAttack){
                if (Time.time > _curComboEndTime) {
                    _prepareToAttack = false;
                    AttackStartAction();
                }
            }else if (IsFiring && Time.time > _curComboEndTime) {
                AttackEndAction();
            }

            if (!(Hand == LEFT_HAND ? cmd.buttons[(int)UserButton.LeftMouse] : cmd.buttons[(int)UserButton.RightMouse]) || IsOverHeat()){
                return;
            }

            if (Time.time - TimeOfLastUse >= ReceiveNextAttackThreshold && !_prepareToAttack) {
                if (!_receiveNextSlash || !Cbt.CanMeleeAttack)return;
                if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

                //waiting for next combo ?
                if (_curCombo < 4){
                    if(_curCombo == 3 && !_isAnotherWeaponSword) {//combo 3 is only available for two swords
                        _receiveNextSlash = false;
                        return;
                    } else{
                        _receiveNextSlash = true;
                    }
                } else{
                    _receiveNextSlash = false;
                    return;
                }

                TimeOfLastUse = Time.time;
                _prepareToAttack = true;

                //IncreaseHeat(data.HeatIncreaseAmount);//TODO : check master sync this 
            }
        }

        protected virtual void AttackStartAction(){
            IsFiring = true;
            TimeOfLastUse = Time.time;

            PlaySlashAnimation(Hand);
            MeleeAttack(Hand);//todo : check this
            Mctrl.SetInstantMoving(Mctrl.GetForwardVector(),  (Mctrl.IsJumping) ? _instantMoveDistanceInAir : _instantMoveDistanceOnGround, Mctrl.IsJumping ? _attackAnimationLengths[(int)SlashType.AirAttack] * 0.8f : _attackAnimationLengths[_curCombo]);

            if (_slashSounds != null && _slashSounds[_curCombo] != null) WeaponAudioSource.PlayOneShot(_slashSounds[_curCombo]);

            Cbt.CanMeleeAttack = Mctrl.Grounded;
            Mctrl.ResetCurBoostingSpeed();

            _curComboEndTime = Time.time + ((Mctrl.IsJumping) ? _attackAnimationLengths[(int) SlashType.AirAttack] : _attackAnimationLengths[_curCombo]);
            _curCombo++;
        }

        protected virtual void AttackEndAction() {//This is being called once after combo end
            _curCombo = 0;
            _receiveNextSlash = true;
            IsFiring = false;

            Mctrl.LockMechRot(false);
        }

        protected override void ResetMeleeVars(){//this is called when on skill or init
            base.ResetMeleeVars();
            _curCombo = 0;
            _receiveNextSlash = true;

            MechAnimator.SetBool("SlashL", false);
            MechAnimator.SetBool("SlashR", false);
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

        public void PlaySlashAnimation(int hand){
            MechAnimator.SetBool(hand == LEFT_HAND ? AnimatorHashVars.SlashLHash : AnimatorHashVars.SlashRHash, true);
        }

        public override void WeaponAnimationEvent(int hand, string s){
        }
    }
}