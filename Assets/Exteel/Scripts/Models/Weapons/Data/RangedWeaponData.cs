using UnityEngine;

public abstract class RangedWeaponData : WeaponData {
    public GameObject bulletPrefab,muzzlePrefab;
    public AudioClip shotSound, reload_sound;
}