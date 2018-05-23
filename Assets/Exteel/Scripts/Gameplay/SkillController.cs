using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour {
    [SerializeField]private BuildMech bm;
    [SerializeField]private MechCombat mechcombat;
    [SerializeField]private Animator skillAnimtor, mainAnimator;
    [SerializeField]private Camera cam, skillcam;
    [SerializeField]private SkillConfig[] skill = new SkillConfig[4];
    [SerializeField]private Sounds Sounds;
    [SerializeField]private AudioClip sorry;

    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;
    private Animator[] WeaponAnimators = new Animator[4];
    private int weaponOffset = 0, curSkillNum = 0;
    private bool[] skill_usable = new bool[4];
    private const string Target_Animation_Name = "skill_target";

    public delegate void OnSkillAction(bool b);
    public OnSkillAction OnSkill;

    public int SP = 0;

    //debug
    public bool isDrone = false;

    private void Awake() {
        RegisterOnSkill();
        RegisterOnWeaponBuilt();
        RegisterOnWeaponSwitched();
    }

    private void RegisterOnWeaponSwitched() {
        if (mechcombat != null)mechcombat.OnWeaponSwitched += OnWeaponSwitched;
    }

    private void RegisterOnWeaponBuilt() {
        if(bm!=null)bm.OnWeaponBuilt += UpdateWeaponAnimators;
    }

    private void RegisterOnSkill() {
        OnSkill += SwitchToSkillAnimator;
    }

    private void Start () {
        //TODO : load skill in buildMech 

        initSkillAnimatorControllers();
        InitSkill();
    }

    private void InitSkill() {
        for(int i = 0; i < skill.Length; i++) {
            if (skill[i] != null) {
                skill[i].AssignSkillNum(i);
                skill[i].AddComponent(gameObject);
                LoadSkillAnimation(i, skill[i].GetPlayerAniamtion());
            }
        }
    }

    private void initSkillAnimatorControllers() {
        animatorOverrideController = new AnimatorOverrideController(skillAnimtor.runtimeAnimatorController);
        skillAnimtor.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
    }

    public void LoadSkillAnimation(int num, AnimationClip animationClip) {
        clipOverrides["skill_" + num] = animationClip;
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public void PlayWeaponAnimation(string skill_name) {        
        if (WeaponAnimators[weaponOffset] != null) {
            Debug.Log(skill_name);
            WeaponAnimators[weaponOffset].Play(skill_name);
        }
        if (WeaponAnimators[weaponOffset+1] != null) {
            WeaponAnimators[weaponOffset+1].Play(skill_name);
        }
    }

    public void CallUseSkill(int num) {
        if (skill_usable[num] && CheckIfEnergyEnough() && !mechcombat.IsSwitchingWeapon()) {
            skill[num].Use();
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

    private void OnWeaponSwitched() {
        weaponOffset = mechcombat.GetCurrentWeaponOffset();
        CheckIfSkillsUsable();
    }

    private void SwitchToSkillAnimator(bool b) {
        skillAnimtor.enabled = b;
        mainAnimator.enabled = !b;
        if (b) {
            mainAnimator.Rebind();
        }
    }

    private void UpdateWeaponAnimators() {
        for (int i = 0; i < 4; i++) {
            if (bm.weapons[i] == null) WeaponAnimators[i] = null;
            else WeaponAnimators[i] = bm.weapons[i].GetComponent<Animator>();
        }
    }

    public void PlaySkill(int skill_num) {//state name : skill_1 , skill_2 , ... 
        SwitchToSkillAnimator(true);
        StartCoroutine(ReturnDefaultStateWhenEnd("skill_" + skill_num));
        SwitchToSkillCam(true);
        OnSkill(true);

        skillAnimtor.Play("skill_" + skill_num);
    }

    public void TargetOnSkill(AnimationClip skill_target) {
        //override target on skill animation
        clipOverrides[Target_Animation_Name] = skill_target;
        animatorOverrideController.ApplyOverrides(clipOverrides);

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
                bool L = (skill[i].weaponTypeL == string.Empty || skill[i].weaponTypeL == bm.weaponScripts[weaponOffset].GetType().ToString());
                bool R = (skill[i].weaponTypeR == string.Empty || skill[i].weaponTypeR == bm.weaponScripts[weaponOffset+1].GetType().ToString());
                skill_usable[i] = (L && R);
            }
        }
    }

    IEnumerator ReturnDefaultStateWhenEnd(string stateToWait) {//TODO : improve this
        yield return new WaitForSeconds(0.2f);//TODO : remake this logic

        yield return new WaitWhile(() => skillAnimtor.GetCurrentAnimatorStateInfo(0).IsName(stateToWait));
        OnSkill(false);
        if(!isDrone)SwitchToSkillCam(false);
    }

    public void PlayCancelSkill() {
        SwitchToSkillAnimator(true);
        StartCoroutine(ReturnDefaultStateWhenEnd("Skill_Cancel_01"));
        SwitchToSkillCam(true);
        OnSkill(true);

        skillAnimtor.Play("Skill_Cancel_01");
        PlaySkillSound(sorry);
    }

    public void PlaySkillSound(AudioClip audioClip) {
        Sounds.PlayClip(audioClip);
    }

    private void SwitchToSkillCam(bool b) {
        skillcam.enabled = b;
        cam.enabled = !b;
    }


}
