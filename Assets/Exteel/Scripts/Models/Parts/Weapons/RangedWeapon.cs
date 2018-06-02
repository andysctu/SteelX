using UnityEngine;

public abstract class RangedWeapon : Weapon {
    public GameObject bulletPrefab;
    public AudioClip shoot_sound, reload_sound;
}