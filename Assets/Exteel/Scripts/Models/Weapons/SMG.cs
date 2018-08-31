using UnityEngine;

public class SMG : RangedWeapon {
    private AudioClip shotSound, reloadSound;

    public SMG() {
        allowBothWeaponUsing = true;
    }

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);
    }

    protected override void InitComponents() {
        base.InitComponents();
    }

    protected override void LoadSoundClips() {
        shotSound = ((SMGData)data).shotSound;
        reloadSound = ((SMGData)data).reload_sound;
    }

    public override void HandleCombat() {
        if (!Input.GetKey(BUTTON) || IsOverHeat()) {
            return;
        }

        if (Time.time - timeOfLastUse >= 1 / data.Rate) {
            timeOfLastUse = Time.time;

            IncreaseHeat();

            //Play Animation

        }
    }

    public override void HandleAnimation() {
    }

    public override void OnTargetEffect(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }

    //private void UpdateSMGAnimationSpeed() {//Use SMG rate to adjust animation speed
    //    if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.APS) {//APS animation clip length 1.066s
    //        animator.SetFloat("rateL", (((SMGData)weaponScripts[weaponOffset]).Rate) * 1.066f);
    //    } else if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.LMG) {//LMG animation clip length 0.8s
    //        animator.SetFloat("rateL", (((SMGData)weaponScripts[weaponOffset]).Rate) * 0.8f);
    //    }

    //    if (curSpecialWeaponTypes[weaponOffset + 1] == (int)SpecialWeaponTypes.APS) {
    //        animator.SetFloat("rateR", (((SMGData)weaponScripts[weaponOffset + 1]).Rate) * 1.066f);
    //    } else if (curSpecialWeaponTypes[weaponOffset + 1] == (int)SpecialWeaponTypes.LMG) {
    //        animator.SetFloat("rateR", (((SMGData)weaponScripts[weaponOffset + 1]).Rate) * 0.8f);
    //    }
    //}
    
}