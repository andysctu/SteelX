using UnityEngine;

public abstract class SkillConfig : ScriptableObject {
    [Header("Skill General")]
    [SerializeField] protected Weapon weaponL, weaponR;//if two handed , put it on L , and leave R null
    [SerializeField] protected int EnergyCost, damage, crosshairRadius, detectRange, distance;
    [SerializeField] protected AnimationClip playerAnimation;
    [SerializeField] protected GameObject[] playerEffects, weaponLEffects, weaponREffects;
    protected ISkill behaviour;
    private int skill_num;
    //camera curve

    public abstract void AddComponent(GameObject gameObjectTOattachTo);

    public void Use() {
        behaviour.Use();
    }

    public struct SkillParams {
        public int EnergyCost, damage, distance;
        public int crosshairRadius;
        public int detectRange;
        public AnimationClip playerAnimation;

        public SkillParams(int EnergyCost, int damage, int crosshairRadius, int detectRange, int distance, AnimationClip playerAnimation) {
            this.EnergyCost = EnergyCost;
            this.damage = damage;
            this.crosshairRadius = crosshairRadius;
            this.detectRange = detectRange;
            this.distance = distance;
            this.playerAnimation = playerAnimation;
        }
    }

    public AnimationClip GetPlayerAniamtion() {
        return playerAnimation;
    }

    public void AssignSkillNum(int num) {
        skill_num = num;
    }

    public int GetSkillNum() {
        return skill_num;
    }
}

public interface ISkill {
    void Use();
}
