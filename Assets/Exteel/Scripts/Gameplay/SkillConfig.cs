using UnityEngine;

public abstract class SkillConfig : ScriptableObject {
    [Header("Skill General")]
    public string weaponTypeL, weaponTypeR;//if two handed , put it on type 1
    [Tooltip("animation 1 must match the order ; animation 2 is the different order")]
    [SerializeField] protected AnimationClip playerAnimation1, playerAnimation2;
    [SerializeField] protected GameObject[] playerEffects, weaponLEffects, weaponREffects;
    [SerializeField] protected AudioClip skill_sound;
    public int EnergyCost, damage, crosshairRadius, detectRange, distance;
    protected ISkill behaviour;
    private int skill_num;
    //camera curve

    public abstract void AddComponent(GameObject gameObjectTOattachTo);

    public void Use() {
        behaviour.SetConfig(this);
        behaviour.Use();
    }
    
    public AnimationClip GetPlayerAniamtion() {
        return playerAnimation1;
    }

    public void AssignSkillNum(int num) {
        skill_num = num;
    }

    public int GetSkillNum() {
        return skill_num;
    }

    public AudioClip GetSkillSound() {
        return skill_sound;
    }
}

public interface ISkill {
    void SetConfig(SkillConfig config);
    void Use();
}