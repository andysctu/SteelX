using UnityEngine;

namespace Weapons
{
    public class Shield : Weapon
    {
        private Transform _effectEnd;
        private ShieldDamageable _shieldDamageable;
        private AudioClip _onHitSound;
        private ParticleSystem _onHitEffect, _overheatEffect;
        private int AtkAnimHash;

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, pos, handTransform, Cbt, Animator);
            InitComponents();
            InitAtkAnimHash();
            AddShieldDamageable();
            AttachEffects();
            ResetAnimationVars();
        }

        private void InitComponents(){
            _effectEnd = weapon.transform.Find("EffectEnd");
        }

        private void InitAtkAnimHash(){
            AtkAnimHash = (Hand == 0) ? Animator.StringToHash("AtkL") : Animator.StringToHash("AtkR");
        }

        private void AddShieldDamageable(){
            Collider c = weapon.GetComponentInChildren<Collider>();
            LayerMask _shieldLayerMask = LayerMask.NameToLayer("Shield");
            c.gameObject.layer = _shieldLayerMask.value;
            _shieldDamageable = c.gameObject.AddComponent<ShieldDamageable>();
            _shieldDamageable.Init(this, Cbt, PlayerPv, WeapPos, Hand);

            ((IDamageableManager) Cbt).RegisterDamageableComponent(_shieldDamageable);
        }

        private void AttachEffects(){
            //OnHitEffect
            _onHitEffect = Object.Instantiate(((ShieldData) data).OnHitEffect, _effectEnd);
            TransformExtension.SetLocalTransform(_onHitEffect.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

            //OverheatEffect
            _overheatEffect = Object.Instantiate(((ShieldData) data).OverheatEffect, weapon.transform);
            TransformExtension.SetLocalTransform(_overheatEffect.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
        }

        private void ResetAnimationVars(){
            IsFiring = false;
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            ResetAnimationVars();
        }

        public override void OnWeaponSwitchedAction(bool isThisWeaponActivated){
            ResetAnimationVars();
            if (isThisWeaponActivated){
                UpdateMechArmState();
            }
        }

        private void UpdateMechArmState(){
            MechAnimator.Play("SHS", 1 + Hand);
        }

        public override void HandleCombat(usercmd cmd) {
            if (!cmd.buttons[(int)MouseButton] || IsOverHeat()){
                if (IsFiring){
                    AttackEndAction();
                }
                return;
            }

            if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

            if (!IsFiring){
                AttackStartAction();
            }
        }

        public override void HandleAnimation(){
            if (IsFiring){
                if (!MechAnimator.GetBool(AtkAnimHash)){
                    MechAnimator.SetBool(AtkAnimHash, true);
                }
            } else{
                if (MechAnimator.GetBool(AtkAnimHash)){
                    MechAnimator.SetBool(AtkAnimHash, false);
                }
            }
        }

        protected virtual void AttackStartAction(){
            IsFiring = true;

            if(PhotonNetwork.isMasterClient)Cbt.Attack(WeapPos, Vector3.zero, 0, null, null, new []{0});
        }

        public override void AttackRpc(Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields){
            if(Cbt.GetOwner().IsLocal)return;

            if(additionalFields == null || additionalFields.Length < 1)return;
            IsFiring = additionalFields[0] == 0;
        }

        protected virtual void AttackEndAction() {
            IsFiring = false;

            if (PhotonNetwork.isMasterClient) Cbt.Attack(WeapPos, Vector3.zero, 0, null, null, new[] {1});
        }

        protected override void LoadSoundClips(){
            _onHitSound = ((ShieldData) data).OnHitSound;
        }

        public virtual void OnHit(int damage, PhotonView attacker, Weapon weapon){
            if (weapon == null){
                Debug.LogError("attacker passed a null weapon with pv id : " + attacker.viewID);
                return;
            }

            Cbt.OnHit(ProcessDamage(damage, weapon.GetWeaponAttackType()), attacker);
        }

        public void PlayOnHitEffect(){
            WeaponAudioSource.PlayOneShot(_onHitSound);

            _onHitEffect.Play();
        }

        public override void OnOverHeatAction(bool isOverHeat){
            if (_overheatEffect == null) return;

            if (isOverHeat){
                var main = _overheatEffect.main;
                main.duration = HeatBar.GetCooldownLength(WeapPos);

                _overheatEffect.Play();
            } else{
                _overheatEffect.Stop();
            }
        }

        public int ProcessDamage(int damage, AttackType attackType) {
            int newDmg = damage;
            float efficiencyCoeff = (IsOverHeat()) ? 1.5f : 1, efficiency = 1;

            switch (attackType) {
                case AttackType.Melee:
                    efficiency = Mathf.Clamp(((ShieldData)data).defend_melee_efficiency * efficiencyCoeff, 0, 1);
                    newDmg = (int)(newDmg * efficiency);
                break;
                default:
                    efficiency = Mathf.Clamp(((ShieldData)data).defend_ranged_efficiency * efficiencyCoeff, 0, 1);
                    newDmg = (int)(newDmg * efficiency);
                break;
            }

            return newDmg;
        }

        public PhotonPlayer GetOwner(){
            return Cbt.GetOwner();
        }

        public override void OnDestroy(){
            ((IDamageableManager)Cbt).DeregisterDamageableComponent(_shieldDamageable);

            base.OnDestroy();
        }
    }
}