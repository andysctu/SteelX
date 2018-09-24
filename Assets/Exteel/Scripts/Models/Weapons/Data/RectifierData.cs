using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Rectifier", order = 4)]
public class RectifierData : RangedWeaponData {

    RectifierData() {
        WeaponType = typeof(Rectifier);
        attackType = Weapon.AttackType.Ranged;
        AllowBothWeaponUsing = true;
        Slowdown = false;
        IsTwoHanded = false;
    }

    public override object GetWeaponObject() {
        return new Rectifier();
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        
    }
}