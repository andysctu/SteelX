using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillController : MonoBehaviour {
    [SerializeField] private BuildMech bm;
    [SerializeField] private MechCombat mechcombat;
    [SerializeField] private MechController mechController;
    [SerializeField] private Animator skillAnimator, mainAnimator, skillcamAnimator;
    [SerializeField] private Camera mainCam, skillCam;
    [SerializeField] private SkillConfig[] skills = new SkillConfig[4];
    [SerializeField] private Sounds Sounds;
    [SerializeField] private AudioClip sorry;
    [SerializeField] private PhotonView photonView;

    private AnimatorOverrideController animatorOverrideController = null, skillcamAnimator_OC = null;
    private AnimationClipOverrides clipOverrides, skillcam_clipOverrides;
    private Animator[] WeaponAnimators = new Animator[4];
    private Animator boosterAnimator;
    private Transform skillUser;
    private SkillHUD SkillHUD;

    private string boosterName;
    private int weaponOffset = 0;
    private bool[] skill_isMatchRequirements = new bool[4];
    private float[] curCooldowns, MaxCooldowns; // curMaxCooldown = MIN_COOLDOWN or MaxCooldown
    private const string Target_Animation_Name = "skill_target";
    private const float MIN_COOLDOWN = 3;

    public delegate void OnSkillAction(bool b);
    public event OnSkillAction OnSkill;

    public List<RequireSkillInfo>[] RequireInfoSkills;

    private int MPU = 4;//TODO : implement this
    private int SP = 0, MAX_SP = 2000;
    private Slider SPBar;
    private Text SPBartext;

    public bool isDrone = false;

    private void Awake() {
        InitSkillAnimatorControllers();
        RegisterOnSkill();//TODO : remake this
        RegisterOnMechBuilt();
        RegisterOnWeaponBuilt();
        RegisterOnWeaponSwitched();
        InitSkillHUD();
    }

    private void RegisterOnMechBuilt() {
        if (bm == null) return;

        bm.OnMechBuilt += OnMechBuilt;
    }

    private void OnMechBuilt() {
        if(bm == null)return;

        LoadMechProperties();
    }

    private void RegisterOnWeaponSwitched() {
        if(mechcombat == null)return;

        mechcombat.OnWeaponSwitched += OnWeaponSwitched;        
    }

    private void RegisterOnWeaponBuilt() {
        if (bm == null) return;

        bm.OnMechBuilt += OnWeaponBuilt;
        bm.OnMechBuilt += LoadPlayerSkillAnimations;
        bm.OnMechBuilt += LoadSkillCamAnimations;
        
    }

    private void OnWeaponSwitched() {
        weaponOffset = mechcombat.GetCurrentWeaponOffset();
        CheckIfSkillsMatchRequirements();
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
        if (photonView.isMine) SkillHUD.InitSkills(skills);
    }

    public void LoadMechProperties() {
        MAX_SP = bm.MechProperty.SP;
        MPU = bm.MechProperty.MPU;
    }

    private void Start() {
        InitComponents();

        if (tag == "Drone")
            enabled = false;
    }

    private void InitSkillHUD() {
        if (!photonView.isMine || tag == "Drone")
            return;
        Transform PanelCanvas = GameObject.Find("PanelCanvas").transform;//TODO : remake this
        SkillHUD = PanelCanvas.Find("SkillPanel").GetComponent<SkillHUD>();

        SkillHUD.enabled = true;
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

    private void LoadPlayerSkillAnimations() {
        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] == null) {
                clipOverrides["sk" + i] = null;
            } else {
                clipOverrides["sk" + i] = skills[i].GetPlayerAniamtion();
            }

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
                    AnimationClip clip = skills[j].GetWeaponAnimation(i % 2);

                    if (clip != null) {
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

    private void LoadSkillCamAnimations() {
        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] == null) {
                skillcam_clipOverrides["sk" + i] = null;
            } else {
                skillcam_clipOverrides["sk" + i] = skills[i].GetCamAnimation();
            }
            skillcamAnimator_OC.ApplyOverrides(skillcam_clipOverrides);
        }
    }

    private void InitSkillAnimatorControllers() {
        animatorOverrideController = new AnimatorOverrideController(skillAnimator.runtimeAnimatorController);
        skillAnimator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        //override skill cam animator
        if (skillcamAnimator == null)
            return;
        skillcamAnimator_OC = new AnimatorOverrideController(skillcamAnimator.runtimeAnimatorController);
        skillcamAnimator.runtimeAnimatorController = skillcamAnimator_OC;

        skillcam_clipOverrides = new AnimationClipOverrides(skillcamAnimator_OC.overridesCount);
        skillcamAnimator_OC.GetOverrides(skillcam_clipOverrides);
    }

    private void Update() {
        for (int i = 0; i < skills.Length; i++) {
            if (curCooldowns[i] > 0) {
                curCooldowns[i] -= Time.deltaTime;
            }
        }
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
        if(skills[skill_num] == null)
            return false;

        bool hasPlayerAnimation = (skills[skill_num].GetPlayerAniamtion() != null);

        return skill_isMatchRequirements[skill_num] && CheckIfSkillHasCooldown(skill_num) && CheckIfEnergyEnough(skills[skill_num].GeneralSkillParams.energyCost) && !mechcombat.isSwitchingWeapon && (!hasPlayerAnimation || mechController.grounded) && !mainAnimator.GetBool("OnMelee");
    }

    private bool CheckIfEnergyEnough(int energyCost) {
        if (SP < energyCost)
            Debug.Log("Energy not enough : " + SP + "<" + energyCost);

        return SP >= energyCost;
    }

    public Camera GetCamera() {
        return mainCam;
    }

    public Camera GetSkillCamera() {
        return skillCam;
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

        skillAnimator.enabled = b;
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

        if (CurrentMech != null) {
            boosterAnimator = CurrentMech.GetComponentInChildren<BoosterController>().GetComponent<Animator>();
            boosterName = boosterAnimator.name;
        } else {
            boosterName = "";
            boosterAnimator = null;
        }
    }

    public void PlayPlayerAnimation(int skill_num) {//state name : skill_1 , skill_2 , ...
        SwitchToSkillAnimator(true);
        StartCoroutine(ReturnDefaultStateWhenEnd("sk" + skill_num));
        SwitchToSkillCam(true);
        PlaySkillCamAnimation(skill_num);
        OnSkill(true);

        skillAnimator.Play("sk" + skill_num);
    }

    public void PlayerBoosterAnimation(int skill_num) {
        if (boosterAnimator != null)
            boosterAnimator.Play("sk" + skill_num);
    }

    public void PlayPlayerEffects(int skill_num) {
        throw new NotImplementedException();
    }

    public void TargetOnSkill(AnimationClip skill_target, AnimationClip skillcam_target, Vector3 skill_user_pos) {//TODO : generalize this
        if (OnSkill != null) OnSkill(true);
        //override target on skill animation
        clipOverrides[Target_Animation_Name] = skill_target;
        animatorOverrideController.ApplyOverrides(clipOverrides);
        if (skillCam != null) {
            //rotate skill cam to face the target so the skill_cam animation is correct
            skillCam.transform.localRotation = Quaternion.identity;
            skillCam.transform.parent.LookAt(transform.position + (transform.position - skill_user_pos) * 9999);

            Debug.DrawRay(transform.position + new Vector3(0,5,0), (transform.position - skill_user_pos) * 9999);
            skillcam_clipOverrides[Target_Animation_Name] = skillcam_target;
            skillcamAnimator_OC.ApplyOverrides(skillcam_clipOverrides);

            SwitchToSkillCam(true);
            PlaySkillCamAnimation(-1);//-1 : target animation
        }
        SwitchToSkillAnimator(true);
        skillAnimator.Play(Target_Animation_Name);
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
    private void CheckIfSkillsMatchRequirements() {
        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] == null) {
                skill_isMatchRequirements[i] = false;
                continue;
            }
            bool req_1 = CheckIfWeaponMatch(skills[i], (bm.weaponScripts[weaponOffset] == null) ? "" : bm.weaponScripts[weaponOffset].GetType().ToString(), (bm.weaponScripts[weaponOffset + 1] == null) ? "" : bm.weaponScripts[weaponOffset + 1].GetType().ToString()),
                req_2 = CheckIfBoosterMatch(skills[i], boosterName);

            skill_isMatchRequirements[i] = req_1 && req_2;
            //Debug.Log("skill : " + i + " is usable ? " + skill_isMatchRequirements[i]);
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
            return (skill.weaponTypeL == string.Empty || skill.weaponTypeL == weaponTypeL) && (skill.weaponTypeR == string.Empty || skill.weaponTypeR == weaponTypeR);
        }
    }

    private bool CheckIfBoosterMatch(SkillConfig skill, string boosterName) {
        if (skill.hasBoosterAnimation) {
            return skill.BoosterName == "" || boosterName.Contains(skill.BoosterName);
        } else {
            return true;
        }
    }

    private IEnumerator ReturnDefaultStateWhenEnd(string stateToWait) {//TODO : improve this so not using string
        yield return new WaitForSeconds(0.2f);//TODO : remake this logic
        yield return new WaitWhile(() => skillAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateToWait));
        OnSkill(false);
        if (!isDrone) SwitchToSkillCam(false);
    }

    public void PlayCancelSkill() {
        SwitchToSkillAnimator(true);
        StartCoroutine(ReturnDefaultStateWhenEnd("Skill_Cancel_01"));
        SwitchToSkillCam(true);
        OnSkill(true);

        skillAnimator.Play("Skill_Cancel_01");
        Sounds.PlayClip(sorry);
    }

    public void PlaySkillSound(int skill_num) {
        if (skills[skill_num].GetSkillSound() != null)
            Sounds.PlayClip(skills[skill_num].GetSkillSound());
    }

    private void SwitchToSkillCam(bool b) {
        if (!photonView.isMine) return;
        skillCam.gameObject.SetActive(b);
        mainCam.enabled = !b;
    }

    private void PlaySkillCamAnimation(int skill_num) {//-1 : target animation
        if (!photonView.isMine) return;

        //Rotate to correct angle
        skillCam.transform.localRotation = Quaternion.identity;

        if (skill_num != -1) {
            skillCam.transform.localRotation = Quaternion.identity;
            skillCam.transform.parent.localRotation = Quaternion.identity;

            skillcamAnimator.Play("sk" + skill_num);
        } else {
            Debug.Log("call : " + Target_Animation_Name);
            skillcamAnimator.Play(Target_Animation_Name);
        }
    }

    public void IncreaseSP(int amount) {
        SP = (SP + amount > MAX_SP) ? MAX_SP : SP + amount;
        updateHUD();
    }

    private void updateHUD() {
        if (SPBar == null) return;//drone;
        SPBar.value = SP / (float)MAX_SP;
        SPBartext.text = UIExtensionMethods.BarValueToString(SP, MAX_SP);
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
}