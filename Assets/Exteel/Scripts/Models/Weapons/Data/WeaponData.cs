﻿using UnityEngine;
using Weapons.Crosshairs;

public abstract class WeaponData : ScriptableObject {
    [SerializeField] private GameObject weaponPrefab_lft, weaponPrefab_rt;

    protected System.Type WeaponType;
    protected Weapons.Weapon.AttackType attackType;
    public string weaponName, displayName;//displayName will be displayed in hangar    
    public GameObject[] Grip = new GameObject[2];//0&1 -> L&R. Grips only set the rotation , the position is adjusted by hand offset
    public int damage = 0, weight = 0;
    public bool AllowBothWeaponUsing = false;
    [Tooltip("The range of ranged weapons")]
    [Range(0, 1200)]public int Range = 0, MinRange = 0;
    [Range(0, 5)]public float Rate = 0;//if it is melee weapon , then it's the time from slash start to the start of receiving the button
    [Tooltip("Crosshair size")][Range(0, 15)]public float Radius = 0;
    [Range(0,200)]public int HeatIncreaseAmount;

    public bool Slowdown, IsTwoHanded, IsTargetAlly = false;
    public int SpIncreaseAmount = 0;

    public virtual void SwitchAnimationClips(Animator weaponAniamtor){
    }

    public AnimationClip[] skillAnimations;

    public string GetWeaponName() {
        return weaponName;
    }

    public GameObject GetWeaponPrefab(int hand = 0) {
        return (hand==0 || weaponPrefab_rt==null) ? weaponPrefab_lft : weaponPrefab_rt;
    }

    public AnimationClip FindSkillAnimationClip(string name) {
        if(skillAnimations != null) {
            foreach(AnimationClip clip in skillAnimations) {
                if(clip.name == name) {
                    return clip;
                }
            }
        }
        return null;
    }

    public abstract object GetWeaponObject();

    public virtual Crosshair GetCrosshair(){
        return null;
    }
    
    public System.Type GetWeaponType() {
        return WeaponType;
    }

    public Weapons.Weapon.AttackType GetAttackType(){
        return attackType;
    }
}


