using UnityEngine;

[CreateAssetMenu]
public class SkillManager : ScriptableObject {
    [SerializeField] private SkillConfig[] skills;

    public SkillConfig FindData(int skill_id) {//now is using index as ID
        if (skills == null || skill_id >= skills.Length) {
            Debug.LogError("skill_id > skills.Length");
            return null;
        } else {
            return skills[skill_id];
        }
    }

    public SkillConfig[] GetAllSkills() {
        for (int i = 0; i < skills.Length; i++) {
            skills[i].SetID(i);
        }

        return skills;
    }

    public SkillConfig GetSkillConfig(int ID) {
        if (ID < skills.Length && ID != -1)
            return skills[ID];
        else return null;
    }
}