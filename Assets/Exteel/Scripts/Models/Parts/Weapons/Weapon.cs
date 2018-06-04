using UnityEngine;

public abstract class Weapon : ScriptableObject {
    [Tooltip("Special weapon types")]
    public string weaponType;//APS , LMG , Rocket , Cannon , Shotgun , ...
    public GameObject weaponPrefab;
    public GameObject[] Grip = new GameObject[2];//L&R , only set the rotation , the position is adjusted by hand offset
    public int damage = 0;

    [Tooltip("The range of ranged weapons")]
    [Range(0, 1200)]
    public int Range = 0;

    [Range(0, 5)]
    public float Rate = 0;//if it is melee weapon , then it's the time from slash start to the start of receiving the button

    [Tooltip("Crosshair size")]
    [Range(0, 15)]
    public float radius = 0;

    [Range(0,200)]
    public int heat_increase_amount;

    [Tooltip("Does this weapon slow down targets?")]
    public bool slowDown;
    public bool twoHanded;

    public abstract void SwitchAnimationClips(Animator weaponAniamtor);

    public AnimationClip[] skillAnimations;

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
}


