using UnityEngine;

public abstract class RangedWeapon : WeaponData {
    public GameObject bulletPrefab;
    public AudioClip shoot_sound, reload_sound;
}