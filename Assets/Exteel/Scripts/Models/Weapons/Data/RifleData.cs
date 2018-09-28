using UnityEngine;
using Weapons;
using Weapons.Crosshairs;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Rifle", order = 3)]
public class RifleData : RangedWeaponData {
    RifleData() {
        WeaponType = typeof(Rifle);
        attackType = Weapon.AttackType.Ranged;
        AllowBothWeaponUsing = true;
        Slowdown = false;
        IsTwoHanded = false;
    }

    public override object GetWeaponObject() {
        return new Rifle();
    }

    public override Crosshair GetCrosshair() {
        return new NCrosshair();
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        
    }
}