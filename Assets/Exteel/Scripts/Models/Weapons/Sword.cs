using System.Linq;
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
        private const float ReceiveNextAttackThreshold = 0.1f;//receiving the input after attacking start time + this && before start time + last combo length - this
        private const int DetectShieldMaxDistance = 50; //the ray which checks if hitting shield max distance
        private const float MinInstantMoveDistance = 5;//the min distance to target
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

            if (Time.time - TimeOfLastUse >= ReceiveNextAttackThreshold && !_prepareToAttack && (!IsFiring || Time.time < TimeOfLastUse + _attackAnimationLengths[_curCombo - 1] - ReceiveNextAttackThreshold)) {
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

        public override void AttackRpc(int[] additionalFields){
            if(Mctrl.GetOwner().IsLocal)return;

            PlaySlashEffect(additionalFields[0]);
        }

        protected virtual void AttackStartAction(){
            IsFiring = true;
            TimeOfLastUse = Time.time;

            PlaySlashEffect(_curCombo);
            DetectTarget();
            if(PhotonNetwork.isMasterClient)Cbt.Attack(WeapPos, new int[]{_curCombo});

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

        protected virtual void PlaySlashEffect(int combo){
            PlaySlashAnimation(Hand);
            if (_slashSounds != null && _slashSounds[combo] != null) WeaponAudioSource.PlayOneShot(_slashSounds[combo]);
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

        public virtual void DetectTarget() {//TODO : check this
            Transform closestTarget = null;

            if ((TargetsInCollider = SlashDetector.getCurrentTargets()).Count != 0) {
                int damage = data.damage;

                foreach (Transform target in TargetsInCollider) {
                    if (target == null) continue;

                    //cast a ray to check if hitting shield
                    bool isHitShield = false, isTerrainBlocksTheWay = false;
                    Transform t = target;

                    var hitPoints = Physics.RaycastAll(Cbt.transform.position + new Vector3(0, 5, 0),
                        target.transform.root.position - Cbt.transform.position, DetectShieldMaxDistance, PlayerAndTerrainMask).OrderBy(h => h.distance).ToArray();

                    foreach (RaycastHit hit in hitPoints) {
                        if (hit.transform.root == target) {
                            if (hit.collider.transform.tag[0] == 'S') {//todo : improve this
                                isHitShield = true;
                                t = hit.collider.transform;
                            }

                            break;
                        } else if (hit.transform.gameObject.layer == TerrainLayer) {
                            //Terrain blocks the way
                            isTerrainBlocksTheWay = true;
                            break;
                        }
                    }

                    if (isTerrainBlocksTheWay) continue;

                    if (isHitShield) {
                        ShieldActionReceiver ShieldActionReceiver = t.transform.parent.GetComponent<ShieldActionReceiver>();
                        int shieldPos = ShieldActionReceiver.GetPos(); //which hand holds the shield?
                        target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PlayerPv.viewID, WeapPos, shieldPos);
                    } else {
                        target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PlayerPv.viewID, WeapPos, -1);
                    }

                    if (target.GetComponent<Combat>().CurrentHP <= 0) {
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, Cbt.GetCamera());
                    } else {
                        if (isHitShield)
                            target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, Cbt.GetCamera());
                        else
                            target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, Cbt.GetCamera());
                    }
                    if(closestTarget == null) closestTarget = target;

                    Cbt.IncreaseSP(data.SpIncreaseAmount);
                }
            }

            InstantMove(_curCombo, closestTarget);
        }

        protected virtual void InstantMove(int combo, Transform target){
            if (target != null) {
                if (Mctrl.IsJumping) {//as usual
                    Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), _instantMoveDistanceInAir, _attackAnimationLengths[(int)SlashType.AirAttack] * 0.8f);
                } else {//move closer to target
                    if((target.position - Mctrl.transform.position).magnitude > MinInstantMoveDistance)
                        Mctrl.SetInstantMoving(target.position - Mctrl.transform.position, (target.position - Mctrl.transform.position).magnitude / 2, _attackAnimationLengths[_curCombo]);
                    else{
                        Mctrl.SetInstantMoving(target.position - Mctrl.transform.position, 0, _attackAnimationLengths[_curCombo]);
                    }
                }
            } else {//move forward
                Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), (Mctrl.IsJumping) ? _instantMoveDistanceInAir : _instantMoveDistanceOnGround, Mctrl.IsJumping ? _attackAnimationLengths[(int)SlashType.AirAttack] * 0.8f : _attackAnimationLengths[_curCombo]);
            }
        }
    }
}