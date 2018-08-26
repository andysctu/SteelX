using UnityEngine;

public abstract class RangedWeaponData : WeaponData {
    public GameObject bulletPrefab;
    public AudioClip shoot_sound, reload_sound;
}