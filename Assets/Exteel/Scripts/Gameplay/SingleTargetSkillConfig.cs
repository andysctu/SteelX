using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Skill/SingleTarget")]
public class SingleTargetSkillConfig : SkillConfig {
    [Header("Skill Special")]
    [SerializeField] private AnimationClip targetAnimation;
    public List<RequireSkillInfo> weaponEffects_1 = new List<RequireSkillInfo>(), weaponEffects_2 = new List<RequireSkillInfo>();//TODO : improve this

    public override void AddComponent(GameObject player) {
        BuildMech bm = player.GetComponent<BuildMech>();

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

        //Attach effects on weapons
        foreach (GameObject p in weaponLEffects) {
            if (weaponTypeL != null) {
                //Check if both weapons match the types
                if (bm.weaponScripts[0] != null && bm.weaponScripts[0].GetType().ToString() == weaponTypeL && bm.weaponScripts[1].GetType().ToString() == weaponTypeR) {
                    GameObject g = Instantiate(p, bm.weapons[0].transform);
                    g.transform.localPosition = Vector3.zero;
                    g.name = p.name;

                    if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                        weaponEffects_1.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                        ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetHand(0);
                    }
                }
                if (bm.weaponScripts[2] != null && bm.weaponScripts[2].GetType().ToString() == weaponTypeL && bm.weaponScripts[3].GetType().ToString() == weaponTypeR) {
                    GameObject g = Instantiate(p, bm.weapons[2].transform);
                    g.transform.localPosition = Vector3.zero;
                    g.name = p.name;

                    if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                        weaponEffects_2.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                        ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetHand(0);
                    }

                }
            }
        }
        

        foreach (GameObject p in weaponREffects) {
            if (weaponTypeR != null) {
                if (bm.weaponScripts[1] != null && bm.weaponScripts[1].GetType().ToString() == weaponTypeR && bm.weaponScripts[0].GetType().ToString() == weaponTypeL) {
                    GameObject g = Instantiate(p, bm.weapons[1].transform);
                    g.transform.localPosition = Vector3.zero;
                    g.name = p.name;
                    if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                        weaponEffects_1.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                        ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetHand(1);
                    }
                }
                if (bm.weaponScripts[3] != null && bm.weaponScripts[3].GetType().ToString() == weaponTypeR && bm.weaponScripts[2].GetType().ToString() == weaponTypeL) {
                    GameObject g = Instantiate(p, bm.weapons[3].transform);
                    g.transform.localPosition = Vector3.zero;
                    g.name = p.name;
                    if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                        weaponEffects_2.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                        ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetHand(1);
                    }
                }
            }
        }

        for (int i = 0; i < 4; i++) {
            if(bm.weapons[i]!=null && bm.weapons[i].GetComponent<Animator>()!=null)
                bm.weapons[i].GetComponent<Animator>().Rebind();
        }

    }

    public AnimationClip GetTargetAnimation() {
        return targetAnimation;
    }
}