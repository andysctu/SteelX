using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SingleTarget")]
public class SingleTargetSkillConfig : SkillConfig {
    [Header("Skill Special")]
    [SerializeField] private AnimationClip targetAnimation_front;
    [SerializeField] private AnimationClip targetAnimation_back;
    public SingleTargetSkillParams SingleTargetSkillParams = new SingleTargetSkillParams();

    public override void AddComponent(GameObject player) {
        BuildMech bm = player.GetComponent<BuildMech>();

        if(bm.GetComponent<SingleTargetSkillBehaviour>() == null) {
            bm.gameObject.AddComponent<SingleTargetSkillBehaviour>();
        }
        
        //Transform hips = player.transform.Find("CurrentMech/metarig/hips");//TODO : consider put effects on hips
        //if(hips == null) {Debug.LogError("can't find hips");return;}

        //Attach effects on player
        /*foreach(GameObject p in playerEffects) {
            GameObject g = Instantiate(p, player.transform);
            g.transform.localPosition = Vector3.zero;
            g.name = p.name;
        }*/

        if ( (weaponTypeL==""||(bm.weaponScripts[0]!=null && weaponTypeL == bm.weaponScripts[0].GetType().ToString())) && 
            (weaponTypeR == "" || (bm.weaponScripts[1] != null && weaponTypeR == bm.weaponScripts[1].GetType().ToString())) ) {
            AttachEffectsOnWeapons(player, 0, 1);

            //reverse order
        } else if( (weaponTypeL == "" || (bm.weaponScripts[1] != null && weaponTypeL == bm.weaponScripts[1].GetType().ToString())) && 
            (weaponTypeR == "" || (bm.weaponScripts[0] != null && weaponTypeR == bm.weaponScripts[0].GetType().ToString())) ) {
            AttachEffectsOnWeapons(player, 1, 0);
        }

        if ((weaponTypeL == "" || (bm.weaponScripts[2] != null && weaponTypeL == bm.weaponScripts[2].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.weaponScripts[3] != null && weaponTypeR == bm.weaponScripts[3].GetType().ToString())) ) {
            AttachEffectsOnWeapons(player, 2, 3);

            //reverse order
        } else if ( (weaponTypeL == ""|| (bm.weaponScripts[3] != null && weaponTypeL == bm.weaponScripts[3].GetType().ToString())) &&
            (weaponTypeR == "" || (bm.weaponScripts[2] != null && weaponTypeR == bm.weaponScripts[2].GetType().ToString())) ) {
            AttachEffectsOnWeapons(player, 3, 2);
        }
    }

    private void AttachEffectsOnWeapons(GameObject player, int L, int R) {//left weapon effects are attached to "L"
        BuildMech bm = player.GetComponent<BuildMech>();
        SkillController SkillController = player.GetComponent<SkillController>();
        
        foreach (GameObject p in weaponLEffects) {
            GameObject g = Instantiate(p, bm.weapons[L].transform);
            g.transform.localPosition = Vector3.zero;

            //name must match when playing animation
            g.name = p.name;

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                if(L < 2) SkillController.weaponEffects_1.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));
                else SkillController.weaponEffects_2.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                //set info
                ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetWeapPos(L % 2, (L >= 2) ? 2 : 0);
                g.SetActive(true);//note that some skills should not be active ( trail )
            }        
        }
        foreach (GameObject p in weaponREffects) {
            GameObject g = Instantiate(p, bm.weapons[R].transform);
            g.transform.localPosition = Vector3.zero;
            g.name = p.name;

            if (g.GetComponent(typeof(RequireSkillInfo)) != null) {
                if (L < 2) SkillController.weaponEffects_1.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));
                else SkillController.weaponEffects_2.Add((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo)));

                ((RequireSkillInfo)g.GetComponent(typeof(RequireSkillInfo))).SetWeapPos(R % 2, (L >= 2) ? 2 : 0);
                g.SetActive(true);
            }
        }
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
    [Tooltip("The distance between player and target at the skill start")]
    public int crosshairRadius;
    public int detectRange, distance;
}