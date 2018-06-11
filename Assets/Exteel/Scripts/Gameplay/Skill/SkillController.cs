﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillController : MonoBehaviour {
    [SerializeField] private BuildMech bm;
    [SerializeField] private MechCombat mechcombat;
    [SerializeField] private MechController mechController;
    [SerializeField] private Animator skillAnimtor, mainAnimator;
    [SerializeField] private Camera cam, skillcam;
    [SerializeField] private SkillConfig[] skills = new SkillConfig[4];
    [SerializeField] private Sounds Sounds;
    [SerializeField] private AudioClip sorry;
    [SerializeField] private PhotonView photonView;

    private AnimatorOverrideController animatorOverrideController = null;
    private AnimationClipOverrides clipOverrides;
    private Animator[] WeaponAnimators = new Animator[4];
    private Animator boosterAnimator;
    private Transform SkillPanel;
    private Transform skillUser;
    private SkillHUD SkillHUD;

    private int weaponOffset = 0, curSkillNum = 0;
    private bool[] skill_usable = new bool[4];
    private float[] skill_length = new float[4];//for skill cam
    private float[] curCooldowns, MaxCooldowns; // curMaxCooldown = MIN_COOLDOWN || MaxCooldown
    private const string Target_Animation_Name = "skill_target";
    private const float MIN_COOLDOWN = 3;

    public delegate void OnSkillAction(bool b);
    public event OnSkillAction OnSkill;

    public List<RequireSkillInfo>[] RequireInfoSkills;

    private int SP = 0, maxSP = 2000;
    private Slider SPBar;
    private Image SPBar_fill;
    private Text SPBartext;

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
        }
    }

    private void RegisterOnWeaponBuilt() {
        if (bm != null) {
            bm.OnWeaponBuilt += OnWeaponBuilt;
        }
    }

    private void OnWeaponSwitched() {
        weaponOffset = mechcombat.GetCurrentWeaponOffset();
        CheckIfSkillsMatchWeaponTypes();
        LoadPlayerSkillAnimations();
    }

    private void OnWeaponBuilt() {
        InitEffectsList();
        InitSkill();
        InitSkillsCooldown();

        UpdateBoosterAnimator();
        UpdateWeaponAnimators();
        LoadWeaponSkillAnimations();
        LoadBoosterSkillAnimations();
        InitHUD();
    }

    private void Start() {
        InitComponents();
        InitSkillHUD();
        
        if(tag == "Drone")
            enabled = false;
    }

    private void InitSkillHUD() {
        if (!photonView.isMine || tag == "Drone")
            return;
        Transform PanelCanvas = FindObjectOfType<RespawnPanel>().transform;
        SkillHUD = PanelCanvas.Find("SkillPanel").GetComponent<SkillHUD>();

        SkillHUD.enabled = true;
        SkillHUD.InitSkills(skills);
    }

    public void SetSkills(SkillConfig[] skills) {//this gets called in buildMech
        this.skills = skills;
    }

    private void InitComponents() {
        photonView = GetComponent<PhotonView>();
    }

    private void InitHUD() {
        if (!photonView.isMine)
            return;
        InitSPBar();
    }

    private void InitSPBar() {
        Slider[] sliders = GameObject.Find("PanelCanvas").GetComponentsInChildren<Slider>();
        if (sliders.Length > 0) {
            SPBar = sliders[2];
            SPBar.value = 0;
            SP = 0;
            SPBartext = SPBar.GetComponentInChildren<Text>();
        }
        updateHUD();
    }

    private void RegisterOnSkill() {
        OnSkill += SwitchToSkillAnimator;
    }

    private void InitSkill() {
        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] != null) {
                skill_length[i] = 0;
                skills[i].AddComponent(gameObject, i);
            }
        }
    }

    private void InitEffectsList() {
        RequireInfoSkills = new List<RequireSkillInfo>[skills.Length];
        for (int i = 0; i < skills.Length; i++) {
            RequireInfoSkills[i] = new List<RequireSkillInfo>();
        }
    }

    private void InitSkillsCooldown() {
        MaxCooldowns = new float[skills.Length];
        curCooldowns = new float[skills.Length];

        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] == null)
                continue;
            curCooldowns[i] = 0;
            MaxCooldowns[i] = skills[i].GeneralSkillParams.cooldown;
        }
    }

    //the skill aniamtions may need to be switched when using different weapons ( in case of different order )
    private void LoadPlayerSkillAnimations() {
        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] == null) {
                clipOverrides["sk" + i] = null;
            } else {
                clipOverrides["sk" + i] = skills[i].GetPlayerAniamtion(CheckIfWeaponOrderReverse(i));
            }

            skill_length[i] = (clipOverrides["skill_" + i] == null) ? 0 : clipOverrides["skill_" + i].length;
            animatorOverrideController.ApplyOverrides(clipOverrides);
        }
    }

    private void LoadWeaponSkillAnimations() {
        for (int i = 0; i < 4; i++) {
            if (WeaponAnimators[i] == null) {
                continue;
            }

            //make sure buildmech overrides the first time
            AnimatorOverrideController WeaponAnimatorOverrideController = (AnimatorOverrideController)WeaponAnimators[i].runtimeAnimatorController;

            AnimationClipOverrides clipOverrides = new AnimationClipOverrides(WeaponAnimatorOverrideController.overridesCount);
            WeaponAnimatorOverrideController.GetOverrides(clipOverrides);

            for (int j = 0; j < skills.Length; j++) {
                if (skills[j] == null || !skills[j].hasWeaponAnimation)
                    continue;

                string weaponTypeL = (bm.weaponScripts[(i >= 2) ? 2 : 0] == null) ? "" : bm.weaponScripts[(i >= 2) ? 2 : 0].GetType().ToString(),
                    weaponTypeR = (bm.weaponScripts[(i >= 2) ? 3 : 1] == null) ? "" : bm.weaponScripts[(i >= 2) ? 3 : 1].GetType().ToString();
                if (!CheckIfWeaponMatch(skills[j], weaponTypeL, weaponTypeR)) {
                    continue;
                }

                if (skills[j] == null || !skills[j].hasWeaponAnimation) {
                    clipOverrides["sk" + j] = null;
                } else {
                    AnimationClip clip = skills[j].GetWeaponAnimation(i % 2, CheckIfWeaponOrderReverse(j));

                    if (clip != null) {
                        //Debug.Log("clip name : " + clip.name + " on weapon : " + i);
                        clipOverrides["sk" + j] = clip;
                    } else {
                        clipOverrides["sk" + j] = bm.weaponScripts[i].FindSkillAnimationClip(bm.weaponScripts[i].name + "_" + skills[j].name);

                        if (bm.weaponScripts[i].FindSkillAnimationClip(bm.weaponScripts[i].name + "_" + skills[j].name) == null)
                            Debug.Log("Can't find the skill animation : " + bm.weaponScripts[i].name + "_" + skills[j].name + " on weapon and there is no default animation.");
                    }
                }
            }

            WeaponAnimatorOverrideController.ApplyOverrides(clipOverrides);
        }
    }

    private void LoadBoosterSkillAnimations() {
        if (boosterAnimator == null)
            return;

        //Make sure buildmech overrides the AnimatorController first
        AnimatorOverrideController BoosterAnimatorOverrideController = (AnimatorOverrideController)boosterAnimator.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(BoosterAnimatorOverrideController.overridesCount);
        BoosterAnimatorOverrideController.GetOverrides(clipOverrides);

        for (int j = 0; j < skills.Length; j++) {
            if (skills[j] == null)
                continue;

            if (skills[j].hasBoosterAnimation) {
                clipOverrides["sk" + j] = skills[j].GetBoosterAnimation();
            }
        }
        BoosterAnimatorOverrideController.ApplyOverrides(clipOverrides);
    }

    private bool CheckIfWeaponOrderReverse(int skill_num) {
        if (((skills[skill_num].weaponTypeL == "" && bm.weaponScripts[weaponOffset] == null) || (bm.weaponScripts[weaponOffset] != null && skills[skill_num].weaponTypeL == bm.weaponScripts[weaponOffset].GetType().ToString())) &&
            ((skills[skill_num].weaponTypeR == "" && bm.weaponScripts[weaponOffset + 1] == null) || (bm.weaponScripts[weaponOffset + 1] != null && skills[skill_num].weaponTypeR == bm.weaponScripts[weaponOffset + 1].GetType().ToString()))) {
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
            WeaponAnimators[weaponOffset].Play("sk" + skill_num);
        }
        if (WeaponAnimators[weaponOffset + 1] != null) {
            WeaponAnimators[weaponOffset + 1].Play("sk" + skill_num);
        }
    }

    public void CallUseSkill(int skill_num) {
        if (CheckIfSkillUsable(skill_num)) {
            if (skills[skill_num].Use(this, skill_num)) {
                IncreaseSP(-skills[skill_num].GeneralSkillParams.energyCost);
                CooldownSkill(skill_num);
            }
        }
    }

    private bool CheckIfSkillUsable(int skill_num) {
        return skill_usable[skill_num] && CheckIfSkillHasCooldown(skill_num) && CheckIfEnergyEnough(skills[skill_num].GeneralSkillParams.energyCost) && !mechcombat.IsSwitchingWeapon() && mechController.grounded && !mainAnimator.GetBool("OnMelee");
    }

    private bool CheckIfEnergyEnough(int energyCost) {
        if (SP < energyCost)
            Debug.Log("Energy not enough : " + SP + "<" + energyCost);

        return SP >= energyCost;
    }

    public Camera GetCamera() {
        return cam;
    }

    public string GetSkillName(int skill_num) {
        return skills[skill_num].name;
    }

    public SkillConfig GetSkillConfig(int skill_num) {
        return skills[skill_num];
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

    private void UpdateBoosterAnimator() {
        Transform CurrentMech = transform.Find("CurrentMech");

        if (CurrentMech != null)
            boosterAnimator = CurrentMech.GetComponentInChildren<BoosterController>().GetComponent<Animator>();
    }

    public void PlayPlayerAnimation(int skill_num) {//state name : skill_1 , skill_2 , ...
        SwitchToSkillAnimator(true);
        StartCoroutine(ReturnDefaultStateWhenEnd("sk" + skill_num));
        SwitchToSkillCam(true);
        OnSkill(true);

        skillAnimtor.Play("sk" + skill_num);
    }

    public void PlayerBoosterAnimation(int skill_num) {
        if (boosterAnimator != null)
            boosterAnimator.Play("sk" + skill_num);
    }

    public void PlayPlayerEffects(int skill_num) {
        throw new NotImplementedException();
    }

    public void TargetOnSkill(AnimationClip skill_target) {//TODO : generalize this
        OnSkill(true);
        //override target on skill animation
        clipOverrides[Target_Animation_Name] = skill_target;
        animatorOverrideController.ApplyOverrides(clipOverrides);
        if (skillcam != null) SwitchToSkillCam(true);
        SwitchToSkillAnimator(true);
        skillAnimtor.Play(Target_Animation_Name);
        StartCoroutine(ReturnDefaultStateWhenEnd(Target_Animation_Name));
    }

    public void SetSkillUser(Transform user) {
        skillUser = user;
    }

    public Transform GetSkillUser() {
        return skillUser;
    }

    private bool CheckIfSkillHasCooldown(int skill_num) {
        return curCooldowns[skill_num] <= 0;
    }

    //do the weapons match the skill requires
    private void CheckIfSkillsMatchWeaponTypes() {
        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] == null) {
                skill_usable[i] = false;
                continue;
            }
            skill_usable[i] = CheckIfWeaponMatch(skills[i], (bm.weaponScripts[weaponOffset] == null) ? "" : bm.weaponScripts[weaponOffset].GetType().ToString(), (bm.weaponScripts[weaponOffset + 1] == null) ? "" : bm.weaponScripts[weaponOffset + 1].GetType().ToString());
        }
    }

    private bool CheckIfWeaponMatch(SkillConfig skill, string weaponTypeL, string weaponTypeR) {
        if (weaponTypeL == "" || weaponTypeR == "") {
            if (weaponTypeL == "") {
                bool L = (skill.weaponTypeL == string.Empty);
                bool R = (skill.weaponTypeR == string.Empty || skill.weaponTypeR == weaponTypeR);
                return (L && R);
            } else {
                bool L = (skill.weaponTypeL == string.Empty || skill.weaponTypeL == weaponTypeL);
                bool R = (skill.weaponTypeR == string.Empty);
                return (L && R);
            }
        } else {
            bool b = ((skill.weaponTypeL == "" || (weaponTypeL != "" && skill.weaponTypeL == weaponTypeL)) &&
            (skill.weaponTypeR == "" || (weaponTypeR != "" && skill.weaponTypeR == weaponTypeR)));

            bool b2 = ((skill.weaponTypeL == "" || (weaponTypeR != "" && skill.weaponTypeL == weaponTypeR)) &&
            (skill.weaponTypeR == "" || (weaponTypeL != "" && skill.weaponTypeR == weaponTypeL)));

            return b || b2;
        }
    }

    private IEnumerator ReturnDefaultStateWhenEnd(string stateToWait) {//TODO : improve this so not using string
        yield return new WaitForSeconds(0.2f);//TODO : remake this logic
        yield return new WaitWhile(() => skillAnimtor.GetCurrentAnimatorStateInfo(0).IsName(stateToWait));
        OnSkill(false);
        if (!isDrone) SwitchToSkillCam(false);
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
        if (skills[skill_num].GetSkillSound() != null)
            Sounds.PlayClip(skills[skill_num].GetSkillSound());
    }

    private void SwitchToSkillCam(bool b) {
        if (!photonView.isMine) return;
        skillcam.GetComponent<SkillCam>().enabled = b;
        skillcam.enabled = b;
        cam.enabled = !b;
    }

    public void IncreaseSP(int amount) {
        SP = (SP + amount > maxSP) ? maxSP : SP + amount;
        updateHUD();
    }

    private void updateHUD() {
        if (SPBar == null) return;//drone;
        SPBar.value = SP / (float)maxSP;
        SPBartext.text = BarValueToString(SP, maxSP);
    }

    private string BarValueToString(int curvalue, int maxvalue) {
        string curvalueStr = curvalue.ToString();
        string maxvalueStr = maxvalue.ToString();

        string finalStr = string.Empty;
        for (int i = 0; i < 4 - curvalueStr.Length; i++) {
            finalStr += "0 ";
        }

        for (int i = 0; i < curvalueStr.Length; i++) {
            finalStr += (curvalueStr[i] + " ");
        }
        finalStr += "/ ";
        for (int i = 0; i < 3; i++) {
            finalStr += (maxvalueStr[i] + " ");
        }
        finalStr += maxvalueStr[3];

        return finalStr;
    }

    private void CooldownSkill(int skill_num) {
        for (int i = 0; i < skills.Length; i++) {
            if (curCooldowns[i] <= MIN_COOLDOWN) {
                curCooldowns[i] = MIN_COOLDOWN;
                SkillHUD.SetSkillCooldown(i, MIN_COOLDOWN);
            }
        }
        curCooldowns[skill_num] = MaxCooldowns[skill_num];
        SkillHUD.SetSkillCooldown(skill_num, MaxCooldowns[skill_num]);
    }

    private void Update() {
        for (int i = 0; i < skills.Length; i++) {
            if (curCooldowns[i] > 0) {
                curCooldowns[i] -= Time.deltaTime;
            }
        }
    }
}