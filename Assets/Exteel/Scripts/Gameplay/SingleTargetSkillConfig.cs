using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SingleTarget")]
public class SingleTargetSkillConfig : SkillConfig {
    [Header("Skill Special")]
    [SerializeField] private AnimationClip targetAnimation;
    public SkillParams skillParams;
    //[SerializeField] private int range, 

    public override void AddComponent(GameObject player) {
        BuildMech bm = player.GetComponent<BuildMech>();

        //add behaviour
        behaviour = player.AddComponent<SingleTargetSkillBehaviour>();
        

        skillParams = new SkillParams(EnergyCost, damage, crosshairRadius, detectRange, distance, playerAnimation);
        ((SingleTargetSkillBehaviour)behaviour).SetConfig(this);

        //attach animation : consider do this in Skill.cs using override method
        Transform currentMech = player.transform.Find("CurrentMech");
        if(currentMech == null) {
            Debug.LogError("can't find currentMech");
            return;
        }
        //currentMech.GetComponent<Animator>().
        

        //attach effects on player
        foreach(GameObject p in playerEffects) {
            GameObject g = Instantiate(p, currentMech);
            g.transform.localPosition = Vector3.zero;
        }

        //attach effects on weapons
        foreach (GameObject p in weaponLEffects) {
            if (weaponL != null) {
                if (bm.weaponScripts[0] != null && bm.weaponScripts[0].weaponType == weaponL.weaponType) {
                    GameObject g = Instantiate(p, bm.weapons[0].transform);
                    g.transform.localPosition = Vector3.zero;
                }
                if (bm.weaponScripts[2] != null && bm.weaponScripts[2].weaponType == weaponL.weaponType) {
                    GameObject g = Instantiate(p, bm.weapons[2].transform);
                    g.transform.localPosition = Vector3.zero;
                }
            }
        }
        foreach (GameObject p in weaponREffects) {
            if (weaponR != null) {
                if (bm.weaponScripts[1] != null && bm.weaponScripts[1].weaponType == weaponR.weaponType) {
                    GameObject g = Instantiate(p, bm.weapons[1].transform);
                    g.transform.localPosition = Vector3.zero;
                }
                if (bm.weaponScripts[3] != null && bm.weaponScripts[3].weaponType == weaponR.weaponType) {
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