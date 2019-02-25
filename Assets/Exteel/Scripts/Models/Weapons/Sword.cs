using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XftWeapon;

namespace Weapons
{
    public class Sword : MeleeWeapon
    {
        protected IDamageable[] TargetsInCollider;
        protected readonly List<IDamageable> _targets = new List<IDamageable>();
        private XWeaponTrail _weaponTrail;
        private AudioClip[] _slashSounds = new AudioClip[4];

        private bool _getMouseButtonUp;
        private bool _receiveNextSlash, _isAnotherWeaponSword, _prepareToAttack;
        //_receiveNextSlash : Is waiting for the next combo (button)
        //_prepareToAttack : process next combo when current one end (exceed combo end time)

        private int _curCombo;
        private float _curComboEndTime; //AttackEnd is called when time exceeds curComboEndTime
        private const float ReceiveNextAttackThreshold = 0.1f; //receiving the input after attacking start time + this ,and before comboEndTime - this
        private const float MinInstantMoveDistance = 5; //there are different movements if too close to target or not
        private float _instantMoveDistanceInAir = 22, _instantMoveDistanceOnGround = 17;

        private float[] _attackAnimationLengths;

        private enum SlashType
        {
            FirstAttack,
            SecondAttack,
            ThirdAttack,
            MultiAttack,
            AirAttack
        };

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat cbt, Animator animator){
            base.Init(data, pos, handTransform, cbt, animator);

            InitComponents();
            CheckIsAnotherWeaponSword();
        }

        protected override void InitDataRelatedVars(WeaponData data){
            base.InitDataRelatedVars(data);
            _attackAnimationLengths = (float[]) ((SwordData) data).AttackAnimationLengths.Clone();
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
            if (Mctrl.Grounded) Cbt.CanMeleeAttack = true;

            if (_prepareToAttack){
                if (Time.time > _curComboEndTime){
                    _prepareToAttack = false;
                    AttackStartAction();
                }
            } else if (IsFiring && Time.time > _curComboEndTime){
                AttackEndAction();
            }

            if (!_getMouseButtonUp && !cmd.buttons[(int)MouseButton]){
                _getMouseButtonUp = true;
            }

            if (!cmd.buttons[(int) MouseButton] || IsOverHeat() || !_receiveNextSlash || !Cbt.CanMeleeAttack || !_getMouseButtonUp) return;
            if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

            if (Time.time - TimeOfLastUse >= ReceiveNextAttackThreshold && !_prepareToAttack && (!IsFiring || Time.time < _curComboEndTime - ReceiveNextAttackThreshold)){
                _getMouseButtonUp = false;

                //waiting for next combo ?
                if (_curCombo < 4){
                    if (_curCombo == 3 && !_isAnotherWeaponSword){
                        //combo 3 is only available for two swords
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
                IsFiring = true;
                _prepareToAttack = true;

                //IncreaseHeat(data.HeatIncreaseAmount);//TODO : check master sync this 
            }
        }

        public override void AttackRpc(Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields){
            if (Mctrl.GetOwner().IsLocal) return;

            PhotonView[] targetPvs = new PhotonView[targetPvIDs.Length];
            IDamageable[] targets = new IDamageable[targetPvIDs.Length];

            for (int i = 0; i < targetPvs.Length; i++){
                IDamageableManager iDamageableManager;
                if ((iDamageableManager = targetPvs[i].GetComponent(typeof(IDamageableManager)) as IDamageableManager) != null){
                    IDamageable c = iDamageableManager.FindDamageableComponent(specIDs[i]);
                    targets[i] = c;
                }
            }

            PlaySlashEffect(damage, targets, additionalFields[0]);
        }

        protected virtual void AttackStartAction(){
            IsFiring = true;
            TimeOfLastUse = Time.time;

            IDamageable[] targets = DetectTargets();

            //play effect locally
            PlaySlashEffect(data.damage, targets, _curCombo);

            //Apply slowing down todo : implement this
            //if (data.Slowdown) {
            //    MechController mctrl = target.GetComponent<MechController>();
            //    if (mctrl != null) {
            //        mctrl.SlowDown();
            //    }
            //}

            InstantMove(_curCombo, targets.Length == 0 ? null : targets[0].GetTransform());

            if (PhotonNetwork.isMasterClient){
                //transform targets to ids
                int[] targetPvIDs = new int[targets.Length];
                int[] specIDs = new int[targets.Length];

                for (int i = 0; i < targets.Length; i++){
                    targetPvIDs[i] = targets[i].GetPhotonView().viewID;
                    specIDs[i] = targets[i].GetSpecID();
                }

                //rpc other to play effect
                Cbt.Attack(WeapPos, Mctrl.GetForwardVector(), data.damage, targetPvIDs, specIDs, new int[]{_curCombo});
            }

            Cbt.CanMeleeAttack = Mctrl.Grounded;
            Mctrl.ResetCurBoostingSpeed();
            Cbt.IncreaseSP(data.SpIncreaseAmount * targets.Length);

            _curComboEndTime = Time.time + (!Mctrl.Grounded ? _attackAnimationLengths[(int) SlashType.AirAttack] : _attackAnimationLengths[_curCombo]);
            _curCombo++;
        }

        protected virtual void AttackEndAction(){
            //This is being called once after combo end
            _curCombo = 0;
            _receiveNextSlash = true;
            IsFiring = false;

            Mctrl.LockMechRot(false);
        }

        protected virtual void PlaySlashEffect(int damage, IDamageable[] targets, int combo){
            PlaySlashAnimation(Hand);
            if (_slashSounds != null && _slashSounds[combo] != null) WeaponAudioSource.PlayOneShot(_slashSounds[combo]);

            //apply hits
            foreach (var target in targets){
                target.OnHit(data.damage, PlayerPv, this);
                OnHitTargetAction(target);
            }
        }

        protected override void ResetMeleeVars(){
            //this is called when on skill or init
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
                _curCombo = 0;
                IsFiring = false;
                _prepareToAttack = false;
                _receiveNextSlash = false;
            } else{
                _receiveNextSlash = true;
                Cbt.CanMeleeAttack = true;
            }
        }

        public void OnHitTargetAction(IDamageable target){
            //todo : check this
            ParticleSystem p = Object.Instantiate(HitEffectPrefab, target.GetTransform());
            p.transform.position = target.GetPosition();

            /*
            //display effect todo : implement this
            if (target.GetCurrentHP() <= 0) {
            //target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, Cbt.GetCamera());
            } else {
            //if (isHitShield)
            //    target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, Cbt.GetCamera());
            //else
            //target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, Cbt.GetCamera());
            }*/
        }

        public void PlaySlashAnimation(int hand){
            MechAnimator.SetBool(hand == LEFT_HAND ? AnimatorHashVars.SlashLHash : AnimatorHashVars.SlashRHash, true);
        }

        public virtual IDamageable[] DetectTargets(){
            _targets.Clear();

            if ((TargetsInCollider = SlashDetector.GetCurrentTargets()).Length != 0){
                foreach (IDamageable target in TargetsInCollider){
                    if (target == null) continue;

                    bool isTerrainBlocksTheWay = false;

                    var hitPoints = Physics.RaycastAll(Cbt.transform.position + new Vector3(0, 5, 0), target.GetPosition() - Cbt.transform.position, (target.GetPosition() - Cbt.transform.position).magnitude, PlayerAndTerrainMask).OrderBy(h => h.distance).ToArray();

                    foreach (RaycastHit hit in hitPoints){
                        if (hit.transform.gameObject.layer == TerrainLayer){
                            //Terrain blocks the way
                            isTerrainBlocksTheWay = true;
                            break;
                        }
                    }

                    if (isTerrainBlocksTheWay) continue;

                    //check duplicated
                    bool duplicated = false;
                    foreach (var t in _targets){
                        if (t.GetPhotonView() == target.GetPhotonView()){
                            duplicated = true;
                            break;
                        }
                    }

                    if (duplicated) continue;

                    _targets.Add(target);
                }
            }

            return _targets.ToArray();
        }

        protected virtual void InstantMove(int combo, Transform closestTarget){
            if (closestTarget != null){
                if (!Mctrl.Grounded){
                    //as usual
                    Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), _instantMoveDistanceInAir, _attackAnimationLengths[(int) SlashType.AirAttack] * 0.8f);//can move in air attack earlier(while animation playing
                } else{
                    //move closer to target
                    if ((closestTarget.position - Mctrl.transform.position).magnitude > MinInstantMoveDistance){
                        Vector3 dir = Mctrl.Grounded ? closestTarget.position - Mctrl.transform.position - new Vector3(0, (closestTarget.position - Mctrl.transform.position).y, 0) : closestTarget.position - Mctrl.transform.position;

                        Mctrl.SetInstantMoving(dir, (closestTarget.position - Mctrl.transform.position).magnitude / 2, _attackAnimationLengths[_curCombo]);
                    } else{
                        Mctrl.SetInstantMoving(closestTarget.position - Mctrl.transform.position, 0, _attackAnimationLengths[_curCombo]);
                    }
                }
            } else{
                //move forward
                Vector3 dir = Mctrl.Grounded ? Mctrl.GetForwardVector() - new Vector3(0, Mctrl.GetForwardVector().y, 0) : Mctrl.GetForwardVector();
                Mctrl.SetInstantMoving(dir, !Mctrl.Grounded ? _instantMoveDistanceInAir : _instantMoveDistanceOnGround, !Mctrl.Grounded ? _attackAnimationLengths[(int) SlashType.AirAttack] * 0.8f : _attackAnimationLengths[_curCombo]);
            }
        }
    }
}