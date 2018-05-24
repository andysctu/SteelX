using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "Skill/SingleTarget")]
public class SingleTargetSkillConfig : SkillConfig {
    [Header("Skill Special")]
    [SerializeField] private AnimationClip targetAnimation;
    private BuildMech bm;
    public List<RequireSkillInfo> weaponEffects_1, weaponEffects_2;//weaponEffects_1 corresponds to the weapons 0 & 1

    public override void AddComponent(GameObject player) {
        InitEffectsList();

        bm = player.GetComponent<BuildMech>();

        //Add behaviour
        if((behaviour = player.GetComponent<SingleTargetSkillBehaviour>()) == null) {
            behaviour = player.AddComponent<SingleTargetSkillBehaviour>();
        }
        
        Transform currentMech = player.transform.Find("CurrentMech");
        if(currentMech == null) {Debug.LogError("can't find currentMech");return;}

        //Attach effects on player
        foreach(GameObject p in playerEffects) {
            GameObject g = Instantiate(p, currentMech);
            g.transform.localPosition = Vector3.zero;
        }

        if ( (weaponTypeL==""||(bm.weaponScripts[0]!=null && weaponTypeL == bm.weaponScripts[0].GetType().ToString())) && 
            (weaponTypeR == "" || (bm.weaponScripts[1] != null && weaponTypeR == bm.weaponScripts[1].GetType().ToString())) ) {
            AttachEffectsOnWeapons(0, 1);

            //reverse order
        } else if( (weaponTypeL == "" || (bm.weaponScripts[1] != null && weaponTypeL == bm.weaponScripts[1].GetType().ToString())) && 
            (weaponTypeR == "" || (bm.weaponScripts[0] != null && weaponTypeR == bm.weaponScripts[0].GetType().ToString())) ) {
            AttachEffectsOnWeapons(1, 0);
        }

        if ((weaponTypeL == "" || (bm.weaponScripts[2] != null && weaponTypeL == bm.weaponScripts[2].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.weaponScripts[3] != null && weaponTypeR == bm.weaponScripts[3].GetType().ToString())) ) {
            AttachEffectsOnWeapons(2, 3);

            //reverse order
        } else if ( (weaponTypeL == ""|| (bm.weaponScripts[3] != null && weaponTypeL == bm.weaponScripts[3].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.weaponScripts[2] != null && weaponTypeR == bm.weaponScripts[2].GetType().ToString())) ) {
            AttachEffectsOnWeapons(3, 2);
        }

        for (int i = 0; i < 4; i++) {
            if(bm.weapons[i]!=null && bm.weapons[i].GetComponent<Animator>()!=null)
                bm.weapons[i].GetComponent<Animator>().Rebind();
        }
    }

    private void InitEffectsList() {
        weaponEffects_1 = new List<RequireSkillInfo>();
        weaponEffects_2 = new List<RequireSkillInfo>();
    }

    private void AttachEffectsOnWeapons(int L, int R) {//left weapon effects are attached to "L"
        foreach (GameObject p in weaponLEffects) {
            GameObject g = Instantiate(p, bm.weapons[L].transform);
            g.transform.localPosition = Vector3.zero;

            //name must match when playing animation
            g.name = p.name;

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                if(L < 2)weaponEffects_1.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));
                else weaponEffects_2.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

            //set info
            ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetHand(L%2);
            }        
        }
        foreach (GameObject p in weaponREffects) {
            GameObject g = Instantiate(p, bm.weapons[R].transform);
            g.transform.localPosition = Vector3.zero;
            g.name = p.name;

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                if (L < 2) weaponEffects_1.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));
                else weaponEffects_2.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

            ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetHand(R%2);
            }         
        }
    }

    public AnimationClip GetTargetAnimation() {
        return targetAnimation;
    }
}