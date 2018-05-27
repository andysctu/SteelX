using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour {
    [SerializeField]private BuildMech bm;
    [SerializeField]private MechCombat mechcombat;
    [SerializeField]private MechController mechController;
    [SerializeField]private Animator skillAnimtor, mainAnimator;
    [SerializeField]private Camera cam, skillcam;
    [SerializeField]private SkillConfig[] skill = new SkillConfig[4];
    [SerializeField]private Sounds Sounds;
    [SerializeField]private AudioClip sorry;

    private AnimatorOverrideController animatorOverrideController = null;
    private AnimationClipOverrides clipOverrides;
    private Animator[] WeaponAnimators = new Animator[4];
    private int weaponOffset = 0, curSkillNum = 0;
    private bool[] skill_usable = new bool[4];
    private float[] skill_length = new float[4];//for skill cam
    private const string Target_Animation_Name = "skill_target";

    public delegate void OnSkillAction(bool b);
    public event OnSkillAction OnSkill;

    public List<RequireSkillInfo> weaponEffects_1, weaponEffects_2;//weaponEffects_1 corresponds to the weapons 0 & 1

    private int SP = 0;

    //debug
    public bool isDrone = false;

    private void Awake() {
        InitSkillAnimatorControllers();
        RegisterOnSkill();
        RegisterOnWeaponBuilt();
        RegisterOnWeaponSwitched();
    }

    private void RegisterOnWeaponSwitched() {
        if (mechcombat != null) {
            mechcombat.OnWeaponSwitched += OnWeaponSwitched;
            mechcombat.OnWeaponSwitched += LoadSkillAnimations;
        }
    }

    private void RegisterOnWeaponBuilt() {
        if (bm != null) {
            bm.OnWeaponBuilt += UpdateWeaponAnimators;
            bm.OnWeaponBuilt += InitSkill;
        }
    }

    private void RegisterOnSkill() {
        OnSkill += SwitchToSkillAnimator;
    }

    private void InitSkill() {
        InitEffectsList();

        for (int i = 0; i < skill.Length; i++) {
            if (skill[i] != null) {
                skill_length[i] = 0;
                skill[i].AddComponent(gameObject);
            }
        }
    }

    private void InitEffectsList() {
        weaponEffects_1 = new List<RequireSkillInfo>();
        weaponEffects_2 = new List<RequireSkillInfo>();
    }

    //the skill aniamtions may need to be switched when using different weapons ( different order )
    private void LoadSkillAnimations() {
        for(int i = 0; i < skill.Length; i++) {
            clipOverrides["skill_" + i] = (CheckIfWeaponOrderReverse(i)) ? skill[i].GetPlayerAniamtion(2) : skill[i].GetPlayerAniamtion(1);
            skill_length[i] = (clipOverrides["skill_" + i] == null) ? 0 : clipOverrides["skill_" + i].length;
            //Debug.Log(i + " length is : " + skill_length[i]);
            animatorOverrideController.ApplyOverrides(clipOverrides);
        }
    }

    public void LoadSkillAnimation(int num, AnimationClip animationClip) {
        clipOverrides["skill_" + num] = animationClip;
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    private bool CheckIfWeaponOrderReverse(int skill_num) {
        if (((skill[skill_num].weaponTypeL == "" && bm.weaponScripts[weaponOffset] == null) || (bm.weaponScripts[0] != null && skill[skill_num].weaponTypeL == bm.weaponScripts[weaponOffset].GetType().ToString())) &&
            ((skill[skill_num].weaponTypeR == "" && bm.weaponScripts[weaponOffset + 1] == null) || (bm.weaponScripts[1] != null && skill[skill_num].weaponTypeR == bm.weaponScripts[weaponOffset + 1].GetType().ToString()))) {
            return false;
        } else {
            return true;
        }
    }

    private void InitSkillAnimatorControllers() {
        animatorOverrideController = new AnimatorOverrideController(skillAnimtor.runtimeAnimatorController);
        skillAnimtor.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
    }

    public void PlayWeaponAnimation(int skill_num) {
        if (WeaponAnimators[weaponOffset] != null) {            
            WeaponAnimators[weaponOffset].Play(skill[skill_num].name);
        }
        if (WeaponAnimators[weaponOffset+1] != null) {
            WeaponAnimators[weaponOffset+1].Play(skill[skill_num].name);
        }
    }

    public void CallUseSkill(int num) {
        if (skill_usable[num] && CheckIfEnergyEnough() && !mechcombat.IsSwitchingWeapon() && mechController.grounded && !mainAnimator.GetBool("OnMelee")) {
            skill[num].Use(this, num);
        }
    }

    public int RequireCurSkillNum() {
        return curSkillNum++;
    }

    private bool CheckIfEnergyEnough() {
        return true;
    }

    public Camera GetCamera() {
        return cam;
    }

    public string GetSkillName(int skill_num) {
        return skill[skill_num].name;
    }

    public SkillConfig GetSkillConfig(int skill_num) {
        return skill[skill_num];
    }

    private void OnWeaponSwitched() {
        weaponOffset = mechcombat.GetCurrentWeaponOffset();
        CheckIfSkillsUsable();
    }
    
    private void SwitchToSkillAnimator(bool b) {
        ResetMainAnimatorState();

        if (!b) {//only call update when the skill end , in case shoot animation event gets called
            mainAnimator.Update(0);
        }

        skillAnimtor.enabled = b;
        mainAnimator.enabled = !b;
    }

    private void ResetMainAnimatorState() {
        mainAnimator.SetBool("Boost", false);
        mainAnimator.SetFloat("Speed", 0);
        mainAnimator.SetFloat("Direction", 0);

        mainAnimator.Play("Walk", 0);
        mainAnimator.Play("Idle", 1);
        mainAnimator.Play("Idle", 2);
    }

    private void UpdateWeaponAnimators() {
        for (int i = 0; i < 4; i++) {
            if (bm.weapons[i] == null) WeaponAnimators[i] = null;
            else WeaponAnimators[i] = bm.weapons[i].GetComponent<Animator>();
        }
    }

    public void PlayPlayerAnimation(int skill_num) {//state name : skill_1 , skill_2 , ... 
        SwitchToSkillAnimator(true);
        StartCoroutine(ReturnDefaultStateWhenEnd("skill_" + skill_num));
        SwitchToSkillCam(true);
        OnSkill(true);

        skillAnimtor.Play("skill_" + skill_num);
    }

    public void TargetOnSkill(AnimationClip skill_target) {//TODO : generalize this
        OnSkill(true);
        //override target on skill animation
        clipOverrides[Target_Animation_Name] = skill_target;
        animatorOverrideController.ApplyOverrides(clipOverrides);
        if(skillcam!=null)SwitchToSkillCam(true);
        SwitchToSkillAnimator(true);
        skillAnimtor.Play(Target_Animation_Name);
        StartCoroutine(ReturnDefaultStateWhenEnd(Target_Animation_Name));
    }

    //do the weapons match the skill requires
    private void CheckIfSkillsUsable() {
        for (int i = 0; i < skill.Length; i++) {
            if (skill[i] == null) {
                skill_usable[i] = false;
                continue;
            }

            if(bm.weaponScripts[weaponOffset] == null || bm.weaponScripts[weaponOffset+1] == null) {
                if(bm.weaponScripts[weaponOffset] == null) {
                    bool L = (skill[i].weaponTypeL == string.Empty);
                    bool R = (skill[i].weaponTypeR == string.Empty || skill[i].weaponTypeR == bm.weaponScripts[weaponOffset + 1].GetType().ToString());
                    skill_usable[i] = (L && R);
                } else {
                    bool L = (skill[i].weaponTypeL == string.Empty || skill[i].weaponTypeL == bm.weaponScripts[weaponOffset].GetType().ToString());
                    bool R = (skill[i].weaponTypeR == string.Empty);
                    skill_usable[i] = (L && R);
                }
            } else {
                bool b = ((skill[i].weaponTypeL == "" || (bm.weaponScripts[weaponOffset] != null && skill[i].weaponTypeL == bm.weaponScripts[weaponOffset].GetType().ToString())) &&
                (skill[i].weaponTypeR == "" || (bm.weaponScripts[weaponOffset + 1] != null && skill[i].weaponTypeR == bm.weaponScripts[weaponOffset + 1].GetType().ToString())));

                bool b2 = ((skill[i].weaponTypeL == "" || (bm.weaponScripts[weaponOffset+1] != null && skill[i].weaponTypeL == bm.weaponScripts[weaponOffset+1].GetType().ToString())) &&
                (skill[i].weaponTypeR == "" || (bm.weaponScripts[weaponOffset] != null && skill[i].weaponTypeR == bm.weaponScripts[weaponOffset].GetType().ToString())));

                skill_usable[i] = b || b2;
            }
        }
    }

    IEnumerator ReturnDefaultStateWhenEnd(string stateToWait) {//TODO : improve this so not using string
        yield return new WaitForSeconds(0.2f);//TODO : remake this logic
        yield return new WaitWhile(() => skillAnimtor.GetCurrentAnimatorStateInfo(0).IsName(stateToWait));
        OnSkill(false);
        if (!isDrone) SwitchToSkillCam(false);
        Debug.Log("has returned to default state");
    }

    public void PlayCancelSkill() {
        SwitchToSkillAnimator(true);
        StartCoroutine(ReturnDefaultStateWhenEnd("Skill_Cancel_01"));
        SwitchToSkillCam(true);
        OnSkill(true);

        skillAnimtor.Play("Skill_Cancel_01");
        Sounds.PlayClip(sorry);
    }

    public void PlaySkillSound(int skill_num) {
        Sounds.PlayClip(skill[skill_num].GetSkillSound());
    }

    private void SwitchToSkillCam(bool b) {
        if (!GetComponent<PhotonView>().isMine) return;
        skillcam.GetComponent<SkillCam>().enabled = b;
        skillcam.enabled = b;
        cam.enabled = !b;
    }

    
}
