using UnityEngine;

[CreateAssetMenu(menuName = "Skill/MultiTarget")]
public class MultiTargetSkillConfig : SkillConfig {
    [Header("Skill Special")]
    [SerializeField] private GameObject[] boosterEffects;
    public MultiTargetSkillParams MultiTargetSkillParams = new MultiTargetSkillParams();

    public override void AddComponent(GameObject player, int skill_num) {
        BuildMech bm = player.GetComponent<BuildMech>();

        GameObject player_booster = player.GetComponentInChildren<BoosterController>().gameObject;

        //Check if booster match
        if (BoosterName != "" && !player_booster.name.Contains(BoosterName))
            return;

        if (bm.GetComponent<MultiTargetSkillBehaviour>() == null) {
            bm.gameObject.AddComponent<MultiTargetSkillBehaviour>();
        }

        AttachEffectsOnBooster(player_booster, skill_num);

        if ((weaponTypeL == "" || (bm.WeaponDatas[0] != null && weaponTypeL == bm.WeaponDatas[0].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.WeaponDatas[1] != null && weaponTypeR == bm.WeaponDatas[1].GetType().ToString()))) {
            AttachEffectsOnWeapons(player, 0, 1, skill_num);
        }

        if ((weaponTypeL == "" || (bm.WeaponDatas[2] != null && weaponTypeL == bm.WeaponDatas[2].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.WeaponDatas[3] != null && weaponTypeR == bm.WeaponDatas[3].GetType().ToString()))) {
            AttachEffectsOnWeapons(player, 2, 3, skill_num);
        }
    }

    private void AttachEffectsOnBooster(GameObject booster, int skill_num) {
        SkillController SkillController = booster.transform.root.GetComponent<SkillController>();

        foreach (GameObject effect in boosterEffects) {
            GameObject g;

            if ((g = FindDuplicatedEffect(booster.transform, effect.name)) == null) {
                g = Instantiate(effect, booster.transform);
                g.transform.localPosition = Vector3.zero;
                g.name = effect.name;//name must match when playing animation
            }

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                SkillController.RequireInfoSkills[skill_num].Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));
            }
        }
    }

    private void AttachEffectsOnWeapons(GameObject player, int L, int R, int skill_num) {
        BuildMech bm = player.GetComponent<BuildMech>();
        SkillController SkillController = player.GetComponent<SkillController>();

        foreach (GameObject effect in weaponLEffects) {
            GameObject g;

            if ((g = FindDuplicatedEffect(bm.Weapons[L].GetWeapon().transform, effect.name)) == null) {
                g = Instantiate(effect, bm.Weapons[L].GetWeapon().transform);
                g.transform.localPosition = Vector3.zero;
                g.name = effect.name;//name must match when playing animation
            }

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                SkillController.RequireInfoSkills[skill_num].Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                //set info
                ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetWeapPos(L % 2, (L >= 2) ? 2 : 0);
            }
        }
        foreach (GameObject effect in weaponREffects) {
            GameObject g;

            if ((g = FindDuplicatedEffect(bm.Weapons[R].GetWeapon().transform, effect.name)) == null) {
                g = Instantiate(effect, bm.Weapons[R].GetWeapon().transform);
                g.transform.localPosition = Vector3.zero;
                g.name = effect.name;//name must match when playing animation
            }

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                SkillController.RequireInfoSkills[skill_num].Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));
                ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetWeapPos(R % 2, (L >= 2) ? 2 : 0);
            }
        }
    }

    public string GetBoosterName() {
        return BoosterName;
    }

    public GameObject[] GetboosterEffects() {
        return boosterEffects;
    }

    public override bool Use(SkillController SkillController, int skill_num) {
        MultiTargetSkillBehaviour behaviour = SkillController.GetComponent<MultiTargetSkillBehaviour>();
        return behaviour.Use(skill_num);
    }

    private GameObject FindDuplicatedEffect(Transform t, string effect_name) {
        Transform[] childs = t.GetComponentsInChildren<Transform>();
        foreach (Transform child in childs) {
            if (child.name == effect_name)
                return child.gameObject;
        }
        return null;
    }
}

[System.Serializable]
public struct MultiTargetSkillParams {
    public int crosshairRadius, detectRange, max_target;
}