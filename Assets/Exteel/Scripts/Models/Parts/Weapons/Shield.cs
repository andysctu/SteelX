using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Shield", order = 3)]
public class Shield : Weapon {

    [Range(0, 1)]
    public float defend_melee_efficiency;//final dmg = dmg * efficiency
    [Range(0, 1)]
    public float defend_ranged_efficiency;

    Shield() {
        weaponType = "Shield";
        damage = 0;
        slowDown = false;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        throw new System.NotImplementedException();
    }
}
