using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/SMG", order = 3)]
public class SMG : RangedWeapon {
    [SerializeField]private AnimationClip Atk, Reload;

    SMG() {
        slowDown = false;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        
    }
}