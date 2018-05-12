using UnityEngine;

public class RangedWeapon : Weapon {
    public GameObject bulletPrefab;
    public AudioClip shoot_sound = new AudioClip(),reload_sound = new AudioClip();
}