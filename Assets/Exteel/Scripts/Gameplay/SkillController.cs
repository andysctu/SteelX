using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour {
    [SerializeField]private BuildMech bm;
    [SerializeField]private MechCombat mechcombat;
    [SerializeField]private Animator skillAnimtor, mainAnimator;
    [SerializeField]private Camera cam;
    [SerializeField]private SkillConfig[] skill = new SkillConfig[4];

    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;
    private Animator[] WeaponAnimators = new Animator[4];
    private int weaponOffset = 0, curSkillNum = 0;
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
        if (mechcombat != null)
            mechcombat.OnWeaponSwitched += OnWeaponSwitched;
    }

    private void RegisterOnWeaponBuilt() {
        if(bm!=null)
            bm.OnWeaponBuilt += UpdateWeaponAnimators;
    }

    private void RegisterOnSkill() {
        OnSkill += SwitchToSkillAnimator;
    }

    private void Start () {
        //TODO : load skill in buildMech 

        //implement
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

    private void Update () {
        if (isDrone) return;
        //TODO : move this to mechCombat
        //check input
        if (Input.GetKeyDown(KeyCode.Alpha1)) { 
            if (CheckIfEnergyEnough() && !mechcombat.IsSwitchingWeapon()) {
                if (skill[0] != null) skill[0].Use();
            }
        }else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            if (CheckIfEnergyEnough() && !mechcombat.IsSwitchingWeapon()) {
                if (skill[1] != null) skill[1].Use();
            }
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            if (CheckIfEnergyEnough() && !mechcombat.IsSwitchingWeapon()) {
                if (skill[2] != null) skill[2].Use();
            }
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            if (CheckIfEnergyEnough() && !mechcombat.IsSwitchingWeapon()) {
                if (skill[3] != null) skill[3].Use();
            }
        }
    }

    public void PlayWeaponAnimation(string skill_name) {        
        if (WeaponAnimators[weaponOffset] != null) {
            WeaponAnimators[weaponOffset].Play(skill_name);
        }
        if (WeaponAnimators[weaponOffset+1] != null) {
            WeaponAnimators[weaponOffset+1].Play(skill_name);
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

    public void TargetOnSkill(AnimationClip skill_target) {
        clipOverrides["skill_target"] = skill_target;
        animatorOverrideController.ApplyOverrides(clipOverrides);

        SwitchToSkillAnimator(true);
        skillAnimtor.Play("skill_target");
    }

    private void OnWeaponSwitched() {
        weaponOffset = mechcombat.GetCurrentWeaponOffset();
    }


}
