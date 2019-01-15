using UnityEngine;
using Weapons;

[CreateAssetMenu(fileName = "New Sword", menuName = "WeaponDatas/Sword", order = 0)]
public class SwordData : MeleeWeaponData {
    public float[] AttackAnimationLengths = { 0.8f, 0.8f, 1.3f, 0.8f, 0.6f };// FirstAttack, SecondAttack, ThirdAttack, MultiAttack, AirAttack
    public AudioClip[] SlashSounds = new AudioClip[4]; 
    public AudioClip HitSound = new AudioClip();

    private SwordData() {
        WeaponType = typeof(Sword);
        attackType = Weapon.AttackType.Melee;
        AllowBothWeaponUsing = false;
        Slowdown = true;
        IsTwoHanded = false;
    }

    public override object GetWeaponObject() {
        return new Sword();
    }
}

