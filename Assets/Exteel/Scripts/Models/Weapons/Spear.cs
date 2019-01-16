using System.Linq;
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
        private const int DetectShieldMaxDistance = 50; //the ray which checks if hitting shield max distance
        private const float MinInstantMoveDistance = 5;//the min distance to target
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

        public override void AttackRpc(int[] additionalFields) {
            if (Mctrl.GetOwner().IsLocal) return;

            PlaySlashEffect();
        }

        protected virtual void AttackStartAction() {
            IsFiring = true;
            TimeOfLastUse = Time.time;
            _curComboEndTime = Time.time + ((Mctrl.IsJumping) ? _attackAnimationLengths[(int)SmashType.AirAttack] : _attackAnimationLengths[(int)SmashType.NormalAttack]);

            PlaySlashEffect();
            DetectTarget();//todo : check this
            if (PhotonNetwork.isMasterClient) Cbt.Attack(WeapPos);

            Cbt.CanMeleeAttack = Mctrl.Grounded;
            Mctrl.ResetCurBoostingSpeed();
        }

        protected virtual void AttackEndAction() {//This is being called once after combo end
            IsFiring = false;
            Mctrl.LockMechRot(false);
        }

        protected virtual void PlaySlashEffect() {
            PlaySmashAnimation(Hand);
            if (_smashSound != null) WeaponAudioSource.PlayOneShot(_smashSound);
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

        public void PlaySmashAnimation(int hand) {
            MechAnimator.SetBool(hand == LEFT_HAND ? AnimatorHashVars.SmashLHash : AnimatorHashVars.SmashRHash, true);
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
                    if (closestTarget == null) closestTarget = target;

                    Cbt.IncreaseSP(data.SpIncreaseAmount);
                }
            }

            InstantMove(closestTarget);
        }

        protected virtual void InstantMove(Transform target) {
            if (target != null) {
                if (Mctrl.IsJumping) {//as usual
                    Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), _instantMoveDistanceInAir, _attackAnimationLengths[(int)SmashType.AirAttack] * 0.8f);
                } else {//move closer to target
                    if ((target.position - Mctrl.transform.position).magnitude > MinInstantMoveDistance)
                        Mctrl.SetInstantMoving(target.position - Mctrl.transform.position, (target.position - Mctrl.transform.position).magnitude / 2, _attackAnimationLengths[(int)SmashType.NormalAttack]);
                    else{
                        Mctrl.SetInstantMoving(target.position - Mctrl.transform.position, 0, _attackAnimationLengths[(int)SmashType.NormalAttack]);
                    }
                }
            } else {//move forward
                Mctrl.SetInstantMoving(Mctrl.GetForwardVector(), (Mctrl.IsJumping) ? _instantMoveDistanceInAir : _instantMoveDistanceOnGround, Mctrl.IsJumping ? _attackAnimationLengths[(int)SmashType.AirAttack] * 0.8f : _attackAnimationLengths[(int)SmashType.NormalAttack]);
            }
        }
    }
}