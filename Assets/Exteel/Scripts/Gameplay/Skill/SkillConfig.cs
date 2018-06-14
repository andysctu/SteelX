using UnityEngine;

public abstract class SkillConfig : ScriptableObject {
    [Header("Skill General")]
    private int ID;//now is using the index in SkillManager
    public Sprite icon, icon_grey;

    public string weaponTypeL;
    public string weaponTypeR;//If two-handed , put it on type L

    [SerializeField] protected AnimationClip playerAnimation;
    [SerializeField] protected AnimationClip cameraAnimation, target_CamAnimation;

    public bool hasWeaponAnimation = true, hasBoosterAnimation = false;
    //if weaponAnimation L or R is null & hasWeaponAnimation == true, then it is supposed to have their own animation
    [SerializeField] protected AnimationClip weaponAnimationL, weaponAnimationR, boosterAnimation;

    [SerializeField] protected GameObject[] playerEffects, targetEffects, weaponLEffects, weaponREffects;
    [SerializeField] protected AudioClip skill_sound, mech_sound;
    public GeneralSkillParams GeneralSkillParams = new GeneralSkillParams();

    public abstract void AddComponent(GameObject gameObjectToAttachTo, int skill_num);

    public abstract bool Use(SkillController SkillController, int skill_num);

    public AnimationClip GetPlayerAniamtion() {
        return playerAnimation;
    }

    public AnimationClip GetWeaponAnimation(int hand) {
        if (hand == 0) {
            return weaponAnimationL;
        } else {
            return weaponAnimationR;
        }
    }

    public AnimationClip GetBoosterAnimation() {
        return boosterAnimation;
    }

    public AnimationClip GetCamAnimation() {
        return cameraAnimation;
    }

    public GameObject[] GetPlayerEffects() {
        return playerEffects;
    }

    public AudioClip GetSkillSound() {
        return skill_sound;
    }

    public AudioClip GetMechSound() {
        return mech_sound;
    }

    public void SetID(int id) {//TODO : improve this
        ID = id;
    }
    public int GetID() {
        return ID;
    }
}

[System.Serializable]
public struct GeneralSkillParams {
    public bool IsDamageLessWhenUsing;
    public int energyCost, damage, cooldown;
}

public interface ISkill {
    bool Use(int skill_num);
}