using UnityEngine;

public abstract class SkillConfig : ScriptableObject {
    [Header("Skill General")]
    public string weaponTypeL;//if two handed , put it on type 1
    public string weaponTypeR;
    [Tooltip("Animation 1 must match the order of the  types ; Animation 2 is the reverse order")]
    [SerializeField] protected AnimationClip playerAnimation1, playerAnimation2;
    [SerializeField] protected GameObject[] playerEffects, targetEffects, weaponLEffects, weaponREffects;
    [SerializeField] protected AudioClip skill_sound;
    public int EnergyCost, damage;

    public GeneralSkillParam g = new GeneralSkillParam();

    public abstract void AddComponent(SkillController SkillController, GameObject gameObjectTOattachTo);

    public abstract void Use(SkillController SkillController, int skill_num);
    
    public AnimationClip GetPlayerAniamtion(int num) {
        return (num==1)? playerAnimation1 : playerAnimation2;
    }

    public AudioClip GetSkillSound() {
        return skill_sound;
    }

    public GameObject[] GetTargetEffects() {
        return targetEffects;
    }
}

[System.Serializable]
public struct GeneralSkillParam {
    public int testInt;
}

public interface ISkill {
    void SetConfig(int skill_num);
    void Use(int skill_num);
}