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
        protected IDamageable CurTarget;
        protected Transform Hips, Clavicle, Spine1;
        protected Vector3 UpperArm;
        protected float IdealRot;
        protected bool IsIkOn = false;

        protected int AtkAnimHash;
        protected float AtkAnimationLength;

        public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, hand, handTransform, Cbt, Animator);

            AtkAnimationLength = ((RangedWeaponData) data).AtkAnimationLength;
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
            Clavicle = (Hand == 0) ? MechAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder) : MechAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
            Spine1 = MechAnimator.GetBoneTransform(HumanBodyBones.Chest);
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

        protected virtual void AttackStartAction(){
            IDamageable target = CrosshairController.GetCurrentTarget(Hand);
            CurTarget = target;
            IsFiring = true;

            if (Cbt.GetOwner().IsLocal) {//crosshair effect
                if (CrosshairController != null) CrosshairController.OnShootAction(WeapPos);
            }

            PlayShootEffect(MechCam.transform.forward, target);

            if (target != null) {
                if (PhotonNetwork.isMasterClient) Cbt.Attack(WeapPos, MechCam.transform.forward, data.damage, new int[] { target.GetPhotonView().viewID }, new int[] { target.GetSpecID() });

                Cbt.IncreaseSP(data.SpIncreaseAmount);
            } else {
                if (PhotonNetwork.isMasterClient) Cbt.Attack(WeapPos, MechCam.transform.forward, data.damage, null, null);
            }

            //if (data.Slowdown && !isShield) {
            //    //target.GetComponent<MechController>().SlowDown();
            //}
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

        public override void AttackRpc(Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields) { 
            if(Cbt.GetOwner().IsLocal)return;

            TimeOfLastUse = Time.time;
            IsFiring = true;

            if (targetPvIDs == null || targetPvIDs.Length < 1){
                PlayShootEffect(direction, null);
            } else{
                PhotonView targetPv = PhotonView.Find(targetPvIDs[0]);
                IDamageableManager manager = targetPv.GetComponent(typeof(IDamageableManager)) as IDamageableManager;

                if(manager != null){
                    PlayShootEffect(direction, manager.FindDamageableComponent(specIDs[0]));
                } else{
                    Debug.LogError("Can't find IDamageableManager on : "+targetPv.gameObject.name);
                    PlayShootEffect(direction, null);
                }
            }
        }

        protected virtual void PlayShootEffect(Vector3 direction, IDamageable target) {
            MechAnimator.SetBool(AtkAnimHash, true);
            MechAnimator.Update(0);
            WeaponAnimator.SetTrigger("Atk");

            DisplayBullet(MechCam.transform.forward, target);

            Muzzle.Play();
            //apply damage
            if(target != null)target.OnHit(data.damage, PlayerPv, this);
        }

        protected virtual void DisplayBullet(Vector3 direction, IDamageable target){
        }
    }
}