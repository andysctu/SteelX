using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapons
{
    public class Spear : MeleeWeapon
    {
        protected IDamageable[] TargetsInCollider;
        private readonly List<IDamageable> _targets = new List<IDamageable>();
        private AudioClip _smashSound;
        private bool _prepareToAttack;//process next combo when current one end (exceed combo end time)

        private bool _getMouseButtonUp;
        private float _curComboEndTime;//AttackEnd is called when time exceeds curComboEndTime
        private const float MinInstantMoveDistance = 5;//there are different movements if too close to target or not
        private float _instantMoveDistanceInAir = 25, _instantMoveDistanceOnGround = 19;

        private float[] _attackAnimationLengths;
        private enum SmashType { NormalAttack, AirAttack };

        protected override void InitDataRelatedVars(WeaponData data){
            base.InitDataRelatedVars(data);

            _attackAnimationLengths = (float[])((SpearData)data).AnimationLengths.Clone(); ;
        }

        protected override void LoadSoundClips(){
            _smashSound = ((SpearData) data).SmashSound;
        }

        public override void HandleCombat(usercmd cmd) {
            if (Mctrl.Grounded) Cbt.CanMeleeAttack = true;

            if (_prepareToAttack) {
                if (cmd.timeStamp > _curComboEndTime) {
                    _prepareToAttack = false;
                    AttackStartAction(cmd.timeStamp);
                }
            } else if (IsFiring && cmd.timeStamp > _curComboEndTime) {
                AttackEndAction();
            }

            if (!_getMouseButtonUp && !cmd.buttons[(int)MouseButton]) {
                _getMouseButtonUp = true;
            }

            if (!cmd.buttons[(int)MouseButton] || IsOverHeat() || !Cbt.CanMeleeAttack) return;
            if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

            if (cmd.timeStamp > _curComboEndTime){
                _getMouseButtonUp = false;
                _prepareToAttack = true;
                IsFiring = true;
                //IncreaseHeat(data.HeatIncreaseAmount); //todo : implement this
            }
        }

        public override void AttackRpc(Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields) {
            //if (Mctrl.GetOwner().IsLocal) return;

            //PhotonView[] targetPvs = new PhotonView[targetPvIDs.Length];
            IDamageable[] targets = new IDamageable[targetPvIDs.Length];

            //for (int i = 0; i < targetPvs.Length; i++) {
            //    IDamageableManager iDamageableManager;
            //    if ((iDamageableManager = targetPvs[i].GetComponent(typeof(IDamageableManager)) as IDamageableManager) != null) {
            //        IDamageable c = iDamageableManager.FindDamageableComponent(specIDs[i]);
            //        targets[i] = c;
            //    }
            //}

            PlaySmashEffect(damage, targets);
        }

        protected virtual void AttackStartAction(float startTime) {
            IsFiring = true;
            TimeOfLastUse = startTime;
            _curComboEndTime = startTime + (!Mctrl.Grounded ? _attackAnimationLengths[(int)SmashType.AirAttack] : _attackAnimationLengths[(int)SmashType.NormalAttack]);

            IDamageable[] targets = DetectTargets();
            PlaySmashEffect(data.damage, targets);

            InstantMove(targets.Length == 0 ? null : targets[0].GetTransform());

            //if (PhotonNetwork.isMasterClient){
            //    //transform targets to ids
            //    int[] targetPvIDs = new int[targets.Length];
            //    int[] specIDs = new int[targets.Length];
			//
            //    for (int i = 0; i < targets.Length; i++) {
            //        targetPvIDs[i] = targets[i].GetPhotonView().viewID;
            //        specIDs[i] = targets[i].GetSpecID();
            //    }
			//
            //    Cbt.Attack(WeapPos, Mctrl.GetForwardVector(), data.damage, targetPvIDs, specIDs);
            //}

            Cbt.CanMeleeAttack = Mctrl.Grounded;
            Mctrl.ResetCurBoostingSpeed();
            Cbt.IncreaseSP(data.SpIncreaseAmount * targets.Length);
        }

        protected virtual void AttackEndAction() {//This is being called once after combo end
            IsFiring = false;
            Mctrl.LockMechRot(false);
        }

        protected virtual void PlaySmashEffect(int damage, IDamageable[] targets) {
            PlaySmashAnimation(Hand);
            if (_smashSound != null) WeaponAudioSource.PlayOneShot(_smashSound);

            //apply hits
            foreach (var target in targets) {
                //target.OnHit(data.damage, PlayerPv, this);
                OnHitTargetAction(target);
            }
        }

        protected override void ResetMeleeVars(){
            base.ResetMeleeVars();

            MechAnimator.SetBool("SmashL", false);
            MechAnimator.SetBool("SmashR", false);
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
        }

        public void OnHitTargetAction(IDamageable target){
            ParticleSystem p = Object.Instantiate(HitEffectPrefab, target.GetTransform());
            p.transform.position = target.GetPosition();
        }

        public void PlaySmashAnimation(int hand) {
            MechAnimator.SetBool(hand == LEFT_HAND ? AnimatorHashVars.SmashLHash : AnimatorHashVars.SmashRHash, true);
        }

        public virtual IDamageable[] DetectTargets() {
            _targets.Clear();

            if ((TargetsInCollider = SlashDetector.GetCurrentTargets()).Length != 0) {
                foreach (IDamageable target in TargetsInCollider) {
                    if (target == null) continue;

                    bool isTerrainBlocksTheWay = false;

                    var hitPoints = Physics.RaycastAll(Cbt.transform.position + new Vector3(0, 5, 0),
                        target.GetPosition() - Cbt.transform.position, (target.GetPosition() - Cbt.transform.position).magnitude, PlayerAndTerrainMask).OrderBy(h => h.distance).ToArray();

                    foreach (RaycastHit hit in hitPoints) {
                        if (hit.transform.gameObject.layer == TerrainLayer) {
                            //Terrain blocks the way
                            isTerrainBlocksTheWay = true;
                            break;
                        }
                    }

                    if (isTerrainBlocksTheWay) continue;

                    //check duplicated
                    bool duplicated = false;
                    foreach (var t in _targets) {
                        //if (t.GetPhotonView() == target.GetPhotonView()) {
                        //    duplicated = true;
                        //    break;
                        //}
                    }

                    if (duplicated) continue;

                    _targets.Add(target);
                }
            }

            return _targets.ToArray();
        }

        protected virtual void InstantMove(Transform target) {
            if (target != null) {
                if (!Mctrl.Grounded) {//as usual
                    Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), _instantMoveDistanceInAir, _attackAnimationLengths[(int)SmashType.AirAttack] * 0.8f); //can move in air attack earlier(while animation playing
                } else {//move closer to target
                    if ((target.position - Mctrl.transform.position).magnitude > MinInstantMoveDistance){
                        Vector3 dir = Mctrl.Grounded ? target.position - Mctrl.transform.position - new Vector3(0, (target.position - Mctrl.transform.position).y, 0) :
                            target.position - Mctrl.transform.position;

                        Mctrl.SetInstantMoving(dir, (target.position - Mctrl.transform.position).magnitude / 2, _attackAnimationLengths[(int) SmashType.NormalAttack]);
                    }else{
                        Mctrl.SetInstantMoving(target.position - Mctrl.transform.position, 0, _attackAnimationLengths[(int)SmashType.NormalAttack]);
                    }
                }
            } else {//move forward
                Vector3 dir = Mctrl.Grounded ? Mctrl.GetForwardVector() - new Vector3(0, Mctrl.GetForwardVector().y, 0) : Mctrl.GetForwardVector();
                Mctrl.SetInstantMoving(dir, !Mctrl.Grounded ? _instantMoveDistanceInAir : _instantMoveDistanceOnGround, !Mctrl.Grounded ? _attackAnimationLengths[(int)SmashType.AirAttack] * 0.8f : _attackAnimationLengths[(int)SmashType.NormalAttack]);
            }
        }
    }
}