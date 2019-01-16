using UnityEngine;

namespace Weapons
{
    public abstract class RangedWeapon : Weapon
    {
        protected GameObject BulletPrefab, MuzzlePrefab;
        protected ParticleSystem Muzzle;

        protected Transform EffectEnd;
        protected Camera MechCam;
        protected CrosshairController CrosshairController;

        //IK
        protected GameObject CurTarget = null;
        protected Transform Hips, Clavicle, Spine1;
        protected Vector3 UpperArm;
        protected float IdealRot;
        protected bool IsIkOn = false;

        protected int AtkAnimHash;
        protected bool atkAnimationIsPlaying = false;
        protected float startShootTime;

        public enum StateCallBackType
        {
            AttackStateEnter,
            AttackStateUpdate,
            AttackStateExit,
            ReloadStateEnter,
            ReloadStateExit,
            PoseStateEnter,
            PoseStateExit
        }

        public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, hand, handTransform, Cbt, Animator);

            InitComponents();
            InitAtkAnimHash();
        }

        protected override void InitDataRelatedVars(WeaponData data){
            base.InitDataRelatedVars(data);

            BulletPrefab = ((RangedWeaponData) data).bulletPrefab;
            MuzzlePrefab = ((RangedWeaponData) data).muzzlePrefab;
        }

        private void InitComponents(){
            MechCam = Cbt.GetCamera();
            InitIkTransforms();
            FindEffectEnd();

            AttachMuzzle(EffectEnd);

            if (MechCam != null) CrosshairController = MechCam.GetComponent<CrosshairController>();
        }

        protected virtual void InitIkTransforms(){
            if (MechAnimator == null) return;
            Hips = MechAnimator.transform.Find("Bip01/Bip01_Pelvis");

            Clavicle = (Hand == 0) ? MechAnimator.transform.Find("Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_L_Clavicle") : MechAnimator.transform.Find("Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_R_Clavicle");

            Spine1 = MechAnimator.transform.Find("Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1");
        }

        protected virtual void InitAtkAnimHash(){
            AtkAnimHash = (Hand == 0) ? Animator.StringToHash("AtkL") : Animator.StringToHash("AtkR");
        }

        protected virtual void FindEffectEnd(){
            EffectEnd = TransformExtension.FindDeepChild(weapon.transform, "EffectEnd");
            if (EffectEnd == null){
                Debug.LogError("Can't find EffectEnd on this weapon : " + data.weaponName);
            }
        }

        protected virtual void AttachMuzzle(Transform Effect_End){
            Muzzle = Object.Instantiate(MuzzlePrefab, Effect_End).GetComponent<ParticleSystem>();
            TransformExtension.SetLocalTransform(Muzzle.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
        }

        public override void HandleCombat(usercmd cmd){
            if (!(Hand == LEFT_HAND ? cmd.buttons[(int)UserButton.LeftMouse] : cmd.buttons[(int)UserButton.RightMouse]) || IsOverHeat()) return;

            if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

            if (Time.time - TimeOfLastUse >= 1 / Rate){
                //IncreaseHeat(data.HeatIncreaseAmount);

                TimeOfLastUse = Time.time;

                AttackStartAction();
            }
        }

        public override void HandleAnimation(){
            if (IsFiring){
                if (Time.time - startShootTime >= 1 / Rate){
                    if (atkAnimationIsPlaying){
                        atkAnimationIsPlaying = false;
                        MechAnimator.SetBool(AtkAnimHash, false);
                    }
                } else{
                    if (!atkAnimationIsPlaying){
                        MechAnimator.SetBool(AtkAnimHash, true);
                        atkAnimationIsPlaying = true;
                    }
                }
            } else{
                if (atkAnimationIsPlaying){
                    MechAnimator.SetBool(AtkAnimHash, false);
                    atkAnimationIsPlaying = false;
                }
            }

            if (IsIkOn){
                UpdateIk();
            }
        }

        protected virtual void AttackStartAction(){
            FireRaycast(MechCam.transform.TransformPoint(0, 0, CrosshairController.CamDistanceToMech), MechCam.transform.forward, Hand);//todo : check this


        }

        protected virtual void UpdateIk(){
            //if(MechCam == null)return;

            //IdealRot = Vector3.SignedAngle(MechCam.transform.forward, Cbt.transform.forward, Cbt.transform.right);
            //IdealRot = Mathf.Clamp(IdealRot, -50, 40);
            //IdealRot += (Hand==0)? 180 - Hips.localRotation.eulerAngles.z : -(180 - Hips.localRotation.eulerAngles.z);

            //UpperArm = Clavicle.localRotation.eulerAngles;
            //Clavicle.localRotation = Quaternion.Euler(UpperArm + new Vector3(IdealRot, 0, 0));
        }

        public override void OnWeaponSwitchedAction(bool isThisWeaponActivated){
            if (isThisWeaponActivated){
                UpdateMechArmState();
            } else{
                IsIkOn = false;
            }
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            if (!enter){
                UpdateMechArmState();
                IsIkOn = false;
            }
        }

        protected abstract void UpdateMechArmState();

        protected virtual void FireRaycast(Vector3 start, Vector3 direction, int hand){
            Transform target = ((hand == 0) ? CrosshairController.GetCurrentTargetL() : CrosshairController.GetCurrentTargetR());

            if (target != null){
                PhotonView targetPv = target.transform.root.GetComponent<PhotonView>();

                if (target.tag != "Shield"){
                    PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetPv.viewID, -1);
                } else{
                    //check what hand is it //todo : improve this
                    ShieldActionReceiver shieldActionReceiver = target.parent.GetComponent<ShieldActionReceiver>();
                    int targetShieldPos = shieldActionReceiver.GetPos();

                    PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetPv.viewID, targetShieldPos);
                }
            } else{
                PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, -1, -1);
            }
        }

        public virtual void Shoot(Vector3 direction, int targetPvId, int targetWeapPos){
            if (PlayerPv.isMine){
                if (CrosshairController != null) CrosshairController.OnShootAction(WeapPos);
            }

            MechAnimator.SetBool(AtkAnimHash, true);
            WeaponAnimator.SetTrigger("Atk");

            IsFiring = true;
            startShootTime = Time.time;

            GameObject target = null;
            PhotonView targetPv = PhotonView.Find(targetPvId);
            if (targetPv != null) target = targetPv.gameObject;

            CurTarget = target;

            if (target != null){
                Combat targetCbt = target.GetComponent<Combat>();
                targetCbt.OnHit(data.damage, PlayerPv.viewID, WeapPos, targetWeapPos);

                DisplayBullet(direction, target, (targetWeapPos == -1) ? null : targetCbt.GetWeapon(targetWeapPos));

                Cbt.IncreaseSP(data.SpIncreaseAmount); //TODO : check this
            } else{
                DisplayBullet(direction, null, null);
            }
        }

        protected abstract void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon);

        protected virtual void OnAnimatorIKCallBack(){
        }
    }
}