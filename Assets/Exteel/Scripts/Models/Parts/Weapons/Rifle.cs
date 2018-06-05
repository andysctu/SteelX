using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Rifle", order = 3)]
public class Rifle : RangedWeapon {
    Rifle() {
        weaponType = "Rifle";
        slowDown = false;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        
    }
}