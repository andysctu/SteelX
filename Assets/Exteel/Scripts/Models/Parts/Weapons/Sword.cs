﻿using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Sword", order = 0)]
public class Sword : MeleeWeapon {
    [Tooltip("Normalized time")]
    [Range(0.75f, 1)]
    public float threshold = 1;

    public AudioClip[] slash_sound = new AudioClip[4]; 
    public AudioClip slash_hit_sound = new AudioClip();

    Sword() {
        weaponType = "Sword";
        slowDown = true;
        twoHanded = false;
    }
}

