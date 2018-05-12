using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Spear", order = 0)]
public class Spear : MeleeWeapon {
    public AudioClip smash_sound = new AudioClip();
    public AudioClip smash_hit_sound = new AudioClip();

    Spear() {
        weaponType = "Spear";
        slowDown = true;
        twoHanded = false;
    }
}

