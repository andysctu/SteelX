using UnityEngine;
using XftWeapon;

namespace Weapons
{
    public class Sword : MeleeWeapon
    {
        private XWeaponTrail _weaponTrail;
        private AudioClip[] _slashSounds = new AudioClip[4];

        private bool _receiveNextSlash, _isAnotherWeaponSword;
        private int _curCombo = 0;

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, pos, handTransform, Cbt, Animator);
            InitComponents();
            Threshold = ((SwordData) data).threshold;

            CheckIsAnotherWeaponSword();

            UpdateSlashAnimationThreshold();
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

        public override void HandleCombat(){
            if (!Input.GetKeyDown(BUTTON) || IsOverHeat()){
                return;
            }

            if (Time.time - TimeOfLastUse >= 1 / Rate){
                if (!_receiveNextSlash || !Cbt.CanMeleeAttack){
                    return;
                }

                if (_curCombo == 3 && !_isAnotherWeaponSword) return;

                if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

                Cbt.CanMeleeAttack = false;
                _receiveNextSlash = false;
                TimeOfLastUse = Time.time;

                IncreaseHeat(data.HeatIncreaseAmount);

                //Play Animation
                AnimationEventController.Slash(Hand, _curCombo);
            }
        }

        public override void HandleAnimation(){
        }

        protected override void ResetMeleeVars(){
            //this is called when on skill or init
            base.ResetMeleeVars();
            _curCombo = 0;

            if (!Cbt.photonView.isMine) return;

            _receiveNextSlash = true;

            Cbt.CanMeleeAttack = true;
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
            ((SlashState) state).SetThreshold(Threshold); //the state is confirmed SlashState in mechCombat        

            //Play slash sound
            if (_slashSounds != null && _slashSounds[_curCombo] != null) AudioSource.PlayClipAtPoint(_slashSounds[_curCombo], weapon.transform.position);

            if (PlayerPv != null && PlayerPv.isMine){
                //TODO : master check this
                IsFiring = true;

                //If not final slash
                if (!MechAnimator.GetBool(AnimatorVars.FinalSlashHash)) _receiveNextSlash = true;

                MeleeAttack(Hand);
            }

            _curCombo++;
        }

        private void OnAttackStateMachineExit(MechStateMachineBehaviour state){
            IsFiring = false;
            _receiveNextSlash = true;
            _curCombo = 0;
        }

        private void OnAttackStateExit(MechStateMachineBehaviour state){
            if (((SlashState) state).IsInAir()){
                IsFiring = false;
                _receiveNextSlash = true;
                _curCombo = 0;
            }
        }

        private void UpdateSlashAnimationThreshold(){
            Threshold = ((SwordData) data).threshold;
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