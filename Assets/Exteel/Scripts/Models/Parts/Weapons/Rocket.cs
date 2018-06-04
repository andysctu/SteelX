using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Rocket", order = 5)]
public class Rocket : RangedWeapon {
    [Range(0,20)]
    public int impact_radius;

    [Range(150,350)]
    public int bullet_speed;

    Rocket() {
        weaponType = "Rocket";
        slowDown = true;
        twoHanded = true;
        impact_radius = 6;
        bullet_speed = 200;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        throw new System.NotImplementedException();
    }
}