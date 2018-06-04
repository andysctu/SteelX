using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Shotgun", order = 3)]
public class Shotgun : RangedWeapon {
    Shotgun() {
        weaponType = "Shotgun";
        slowDown = true;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        throw new System.NotImplementedException();
    }
}