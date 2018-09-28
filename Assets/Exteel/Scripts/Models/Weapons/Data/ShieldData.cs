﻿using UnityEngine;
using Weapons;
using Weapons.Crosshairs;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Shield", order = 3)]
public class ShieldData : WeaponData {
    public ParticleSystem OnHitEffect, OverheatEffect;
    public AudioClip OnHitSound;

    [Range(0, 1)]
    public float defend_melee_efficiency;//final dmg = dmg * efficiency
    [Range(0, 1)]
    public float defend_ranged_efficiency;

    ShieldData() {
        WeaponType = typeof(Shield);
        attackType = Weapon.AttackType.None;
        AllowBothWeaponUsing = false;
        damage = 0;
        Slowdown = false;
        IsTwoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        
    }

    public override Crosshair GetCrosshair() {
        return null;
    }

    public override object GetWeaponObject() {
        return new Shield();
    }
}
