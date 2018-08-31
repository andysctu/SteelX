using UnityEngine;

public abstract class RangedWeaponData : WeaponData {
    public GameObject bulletPrefab;
    public AudioClip shotSound, reload_sound;
}