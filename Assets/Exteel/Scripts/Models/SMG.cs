using UnityEngine;


public class SMG : RangedWeapon {
    public override void AttackTarget(GameObject target, bool isShield) {
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
    protected override void LoadSoundClips() {
    }
}