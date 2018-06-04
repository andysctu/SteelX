using UnityEngine;

public abstract class SkillConfig : ScriptableObject {
    [Header("Skill General")]
    public string weaponTypeL, weaponTypeR;//if two handed , put it on type L
    [Tooltip("Animation 1 must match the order of the  types ; Animation 2 is the reverse order")]
    [SerializeField] protected AnimationClip playerAnimation1, playerAnimation2, weaponAnimationL, weaponAnimationR;//if weapon Animation is null , assume each weapon has their own animation
    [SerializeField] protected GameObject[] playerEffects, weaponLEffects, weaponREffects;
    [SerializeField] protected AudioClip skill_sound, mech_sound;
    public GeneralSkillParams GeneralSkillParams = new GeneralSkillParams();

    public abstract void AddComponent(GameObject gameObjectToAttachTo);

    public abstract void Use(SkillController SkillController, int skill_num);
    
    public AnimationClip GetPlayerAniamtion(bool isOrderReverse) {//Is left hand weapon type = weaponTypeL & right hand weapon type = weaponTypeR
        return (isOrderReverse) ? playerAnimation2 : playerAnimation1;
    }

    public AnimationClip GetWeaponAnimationL(bool isOrderReverse) {
        return (isOrderReverse)? weaponAnimationR : weaponAnimationL;
    }

    public AnimationClip GetWeaponAnimationR(bool isOrderReverse) {
        return (isOrderReverse) ? weaponAnimationL : weaponAnimationR;
    }

    public AudioClip GetSkillSound() {
        return skill_sound;
    }

    public AudioClip GetMechSound() {
        return mech_sound;
    }
}

[System.Serializable]
public struct GeneralSkillParams {
    public int energyCost, damage;
}

public interface ISkill {
    void Use(int skill_num);
}