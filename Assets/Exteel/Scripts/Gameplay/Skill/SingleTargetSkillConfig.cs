﻿using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SingleTarget")]
public class SingleTargetSkillConfig : SkillConfig {
    [Header("Skill Special")]
    [SerializeField] private AnimationClip targetAnimation_front;
    [SerializeField] private AnimationClip targetAnimation_back;
    public SingleTargetSkillParams SingleTargetSkillParams = new SingleTargetSkillParams();

    public override void AddComponent(GameObject player, int skill_num) {
        BuildMech bm = player.GetComponent<BuildMech>();

        if (bm.GetComponent<SingleTargetSkillBehaviour>() == null) {
            bm.gameObject.AddComponent<SingleTargetSkillBehaviour>();
        }

        if ((weaponTypeL == "" || (bm.weaponScripts[0] != null && weaponTypeL == bm.weaponScripts[0].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.weaponScripts[1] != null && weaponTypeR == bm.weaponScripts[1].GetType().ToString()))) {
            AttachEffectsOnWeapons(player, 0, 1, skill_num);
        } else if ((weaponTypeL == "" || (bm.weaponScripts[1] != null && weaponTypeL == bm.weaponScripts[1].GetType().ToString())) && //reverse order
            (weaponTypeR == "" || (bm.weaponScripts[0] != null && weaponTypeR == bm.weaponScripts[0].GetType().ToString()))) {
            AttachEffectsOnWeapons(player, 1, 0, skill_num);
        }

        if ((weaponTypeL == "" || (bm.weaponScripts[2] != null && weaponTypeL == bm.weaponScripts[2].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.weaponScripts[3] != null && weaponTypeR == bm.weaponScripts[3].GetType().ToString()))) {
            AttachEffectsOnWeapons(player, 2, 3, skill_num);
        } else if ((weaponTypeL == "" || (bm.weaponScripts[3] != null && weaponTypeL == bm.weaponScripts[3].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.weaponScripts[2] != null && weaponTypeR == bm.weaponScripts[2].GetType().ToString()))) {
            AttachEffectsOnWeapons(player, 3, 2, skill_num);
        }
    }

    private void AttachEffectsOnWeapons(GameObject player, int L, int R, int skill_num) {//left weapon effects are attached to "L"
        BuildMech bm = player.GetComponent<BuildMech>();
        SkillController SkillController = player.GetComponent<SkillController>();

        foreach (GameObject effect in weaponLEffects) {
            GameObject g;

            if ((g = FindDuplicatedEffect(bm.weapons[L].transform, effect.name)) == null) {
                g = Instantiate(effect, bm.weapons[L].transform);
                g.transform.localPosition = Vector3.zero;
                g.name = effect.name;//name must match when playing animation
            }            

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                SkillController.RequireInfoSkills[skill_num].Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                //set info
                ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetWeapPos(L % 2, (L >= 2) ? 2 : 0);
                g.SetActive(true);//note that some skills should not be active ( trail )
            }
        }
        foreach (GameObject effect in weaponREffects) {
            GameObject g;

            if ((g = FindDuplicatedEffect(bm.weapons[R].transform, effect.name)) == null) {
                g = Instantiate(effect, bm.weapons[R].transform);
                g.transform.localPosition = Vector3.zero;
                g.name = effect.name;//name must match when playing animation
            }

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                SkillController.RequireInfoSkills[skill_num].Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetWeapPos(R % 2, (L >= 2) ? 2 : 0);
                g.SetActive(true);
            }
        }
    }

    private GameObject FindDuplicatedEffect(Transform t, string effect_name) {
        Transform[] childs = t.GetComponentsInChildren <Transform>();
        foreach (Transform child in childs) {
            if(child.name == effect_name)
                return child.gameObject;
        }
        return null;
    }

    public override void Use(SkillController SkillController, int skill_num) {
        SingleTargetSkillBehaviour behaviour = SkillController.GetComponent<SingleTargetSkillBehaviour>();
        behaviour.Use(skill_num);
    }

    public AnimationClip GetTargetFrontAnimation() {
        return targetAnimation_front;
    }

    public AnimationClip GetTargetBackAnimation() {
        return targetAnimation_back;
    }
}

[System.Serializable]
public struct SingleTargetSkillParams {
    public int crosshairRadius, detectRange;
    [Tooltip("The distance between player and target at the skill start")]
    public int distance;
}