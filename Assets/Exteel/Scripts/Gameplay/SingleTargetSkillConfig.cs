using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SingleTarget")]
public class SingleTargetSkillConfig : SkillConfig {
    [Header("Skill Special")]
    [SerializeField] private AnimationClip targetAnimation;

    public override void AddComponent(GameObject player) {
        BuildMech bm = player.GetComponent<BuildMech>();

        //Add behaviour
        if((behaviour = player.GetComponent<SingleTargetSkillBehaviour>()) == null) {
            behaviour = player.AddComponent<SingleTargetSkillBehaviour>();
        }
        //((SingleTargetSkillBehaviour)behaviour).SetConfig(this);
        
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
                if (bm.weaponScripts[0] != null && bm.weaponScripts[0].weaponType == weaponTypeL && bm.weaponScripts[1].weaponType == weaponTypeR) {
                    GameObject g = Instantiate(p, bm.weapons[0].transform);
                    g.transform.localPosition = Vector3.zero;
                }
                if (bm.weaponScripts[2] != null && bm.weaponScripts[2].weaponType == weaponTypeL && bm.weaponScripts[3].weaponType == weaponTypeR) {
                    GameObject g = Instantiate(p, bm.weapons[2].transform);
                    g.transform.localPosition = Vector3.zero;
                }
            }
        }

        foreach (GameObject p in weaponREffects) {
            if (weaponTypeR != null) {
                if (bm.weaponScripts[1] != null && bm.weaponScripts[1].weaponType == weaponTypeR && bm.weaponScripts[0].weaponType == weaponTypeL) {
                    GameObject g = Instantiate(p, bm.weapons[1].transform);
                    g.transform.localPosition = Vector3.zero;
                }
                if (bm.weaponScripts[3] != null && bm.weaponScripts[3].weaponType == weaponTypeR && bm.weaponScripts[2].weaponType == weaponTypeL) {
                    GameObject g = Instantiate(p, bm.weapons[3].transform);
                    g.transform.localPosition = Vector3.zero;
                }
            }
        }
    }

    public AnimationClip GetTargetAnimation() {
        return targetAnimation;
    }
}