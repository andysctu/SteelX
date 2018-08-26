using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Shield", order = 3)]
public class ShieldData : WeaponData {

    [Range(0, 1)]
    public float defend_melee_efficiency;//final dmg = dmg * efficiency
    [Range(0, 1)]
    public float defend_ranged_efficiency;

    ShieldData() {
        WeaponType = typeof(Shield);
        damage = 0;
        slowDown = false;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        
    }

    public override object GetWeaponObject() {
        return new Shield();
    }
}
