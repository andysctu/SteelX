using UnityEngine;

namespace Weapons
{
    public abstract class Weapon
    {
        protected GameObject weapon;
        protected WeaponData data;
        
        //Components
        protected Transform WeaponTransform;
        protected Combat Cbt;
        protected MechController Mctrl;
        protected PhotonView PlayerPv;
        protected HeatBar HeatBar;
        protected Animator MechAnimator, WeaponAnimator;
        protected AudioSource WeaponAudioSource;

        //Another weapon
        protected Weapon AnotherWeapon;
        protected WeaponData AnotherWeaponData;

        //Weapon infos
        public string WeaponName;
        public bool AllowBothWeaponUsing, IsFiring = false; //Whether the Mech Atk animation is playing or not
        protected int Hand, WeapPos; //Two-handed weapon's Hand is 0
        protected KeyCode MouseButton;
        protected float TimeOfLastUse, Rate;
        protected const int LEFT_HAND = 0, RIGHT_HAND = 1;
        protected int TerrainLayer = 10, TerrainLayerMask, PlayerLayerMask, PlayerAndTerrainMask;

        public enum AttackType
        {
            Melee,
            Ranged,
            Cannon,
            Rocket,
            None
        };

        protected AttackType attackType;

        public virtual void Init(WeaponData data, int pos, Transform WeapPos, Combat Cbt, Animator MechAnimator){
            InitDataRelatedVars(data);
            this.Cbt = Cbt;
            this.MechAnimator = MechAnimator;
            this.WeaponTransform = WeapPos;
            this.Hand = pos % 2;
            this.WeapPos = pos;
            Mctrl = this.Cbt.GetComponent<MechController>();

            MouseButton = (Hand == LEFT_HAND) ? KeyCode.Mouse0 : KeyCode.Mouse1;

            InstantiateWeapon(data);
            InitComponents();
            InitAnotherWeaponInfo();
            InitLayerMask();
            LoadSoundClips();
            SwitchWeaponAnimationClips(WeaponAnimator);
        }

        protected virtual void InitDataRelatedVars(WeaponData data){
            this.data = data;
            attackType = data.GetAttackType();
            AllowBothWeaponUsing = data.AllowBothWeaponUsing;
            Rate = data.Rate;
            WeaponName = data.weaponName;
        }

        protected virtual void InstantiateWeapon(WeaponData data){
            weapon = Object.Instantiate(data.GetWeaponPrefab(Hand % 2), Vector3.zero, Quaternion.identity) as GameObject;
            WeaponAnimator = weapon.GetComponent<Animator>();

            AdjustScale(weapon);
            SetWeaponParent(weapon);
        }

        private void InitComponents(){
            PlayerPv = Cbt.GetComponent<PhotonView>();
            HeatBar = Cbt.GetComponentInChildren<HeatBar>(true);
            AddAudioSource(weapon);
        }

        private void InitAnotherWeaponInfo(){
            int weaponOffset = WeapPos - 2 >= 0 ? 2 : 0;
            AnotherWeapon = Cbt.GetWeapon(weaponOffset + (Hand + 1) % 2);
            AnotherWeaponData = Cbt.GetWeaponData(weaponOffset + (Hand + 1) % 2);
        }

        private void InitLayerMask(){
            TerrainLayerMask = LayerMask.GetMask("Terrain");
            PlayerLayerMask = LayerMask.GetMask("PlayerLayer");
            PlayerAndTerrainMask = TerrainLayerMask | PlayerLayerMask;
        }

        protected virtual void AddAudioSource(GameObject weapon){
            WeaponAudioSource = weapon.AddComponent<AudioSource>();

            //Init AudioSource
            WeaponAudioSource.spatialBlend = 1;
            WeaponAudioSource.dopplerLevel = 0;
            WeaponAudioSource.volume = 0.8f;
            WeaponAudioSource.playOnAwake = false;
            WeaponAudioSource.minDistance = 20;
            WeaponAudioSource.maxDistance = 250;
        }

        protected abstract void LoadSoundClips();

        protected virtual void SwitchWeaponAnimationClips(Animator WeaponAnimator){
            if (WeaponAnimator == null || WeaponAnimator.runtimeAnimatorController == null) return;

            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(WeaponAnimator.runtimeAnimatorController);
            WeaponAnimator.runtimeAnimatorController = animatorOverrideController;

            data.SwitchAnimationClips(WeaponAnimator);
        }

        public abstract void HandleCombat(usercmd cmd); //Process Input
        public virtual void HandleAnimation(){
        }

        public virtual void OnSkillAction(bool enter){
        }

        public virtual void OnWeaponSwitchedAction(bool b){
        }

        public virtual void OnOverHeatAction(bool b){
        }

        public virtual bool IsOverHeat(){
            if (HeatBar == null) return false;

            return HeatBar.IsOverHeat(WeapPos);
        }

        public virtual void SetOverHeat(bool b){
            HeatBar.SetOverHeat(WeapPos, b);

            if (b){
                OnOverHeatAction(b);
            }
        }

        public virtual void IncreaseHeat(int amount){
            if (HeatBar != null) HeatBar.IncreaseHeat(WeapPos, amount);
        }

        public virtual void ActivateWeapon(bool b){
            //Not using setActive because if weapons have their own animations & are playing , disabling causes weapon animators to rebind the wrong rotation & position
            Renderer[] renderers = weapon.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers){
                renderer.enabled = b;
            }
        }

        protected virtual void AdjustScale(GameObject weapon){
            float newscale = Cbt.transform.root.localScale.x * Cbt.transform.localScale.x;
            weapon.transform.localScale = new Vector3(weapon.transform.localScale.x * newscale, weapon.transform.localScale.y * newscale, weapon.transform.localScale.z * newscale);
        }

        protected virtual void SetWeaponParent(GameObject weapon){
            weapon.transform.SetParent(WeaponTransform);
            weapon.transform.localRotation = Quaternion.Euler(90, 0, 0);
            weapon.transform.localPosition = Vector3.zero;
        }

        //for other players to play effect
        public virtual void AttackRpc(Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields) {
        }

        public virtual void OnDestroy(){
            if (weapon != null) Object.Destroy(weapon);
        }

        public GameObject GetWeapon(){
            return weapon;
        }

        public AttackType GetWeaponAttackType(){
            return attackType;
        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        }
    }
}