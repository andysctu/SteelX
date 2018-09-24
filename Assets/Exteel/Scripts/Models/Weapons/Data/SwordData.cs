using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Sword", order = 0)]
public class SwordData : MeleeWeaponData {
    [Tooltip("When can the slash animations exit in normalized time?")]
    [Range(0.75f, 1)]
    public float threshold = 1;

    public AudioClip[] slash_sound = new AudioClip[4]; 
    public AudioClip slash_hit_sound = new AudioClip();

    SwordData() {
        WeaponType = typeof(Sword);
        attackType = Weapon.AttackType.Melee;
        AllowBothWeaponUsing = false;
        Slowdown = true;
        IsTwoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        return;
    }

    public override object GetWeaponObject() {
        return new Sword();
    }
}

