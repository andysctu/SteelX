using UnityEngine;

public abstract class WeaponData : ScriptableObject {
    [SerializeField] private GameObject weaponPrefab_lft, weaponPrefab_rt;

    protected System.Type WeaponType;
    public string weaponName, displayName;//this will be displayed in hangar    
    public GameObject[] Grip = new GameObject[2];//L&R , only set the rotation , the position is adjusted by hand offset
    public int damage = 0, weight = 0;

    [Tooltip("The range of ranged weapons")]
    [Range(0, 1200)]
    public int Range = 0, minRange = 0;

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

    public int SPincreaseAmount = 0;

    public abstract void SwitchAnimationClips(Animator weaponAniamtor);

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

    public System.Type GetWeaponType() {
        return WeaponType;
    }
}


