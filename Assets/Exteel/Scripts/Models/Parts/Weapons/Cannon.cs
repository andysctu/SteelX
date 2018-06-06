using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Cannon", order = 6)]
public class Cannon : RangedWeapon {
    [Tooltip("Call reload after running out of bullets")]
    public int maxBullet;

    Cannon() {
        weaponType = "Cannon";
        slowDown = true;
        twoHanded = true;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        //throw new System.NotImplementedException();
    }
}