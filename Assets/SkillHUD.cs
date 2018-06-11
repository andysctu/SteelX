using UnityEngine;
using UnityEngine.UI;

public class SkillHUD : MonoBehaviour {
    [SerializeField] private Image[] skillIcons;
    [SerializeField] private Image[] skillCooldowns;
    [SerializeField] private Text[] energyCosts;
    private SkillConfig[] skills;
    private float[] curCooldowns, curMaxCooldowns;

    private const float MARGIN_OF_ERROR = 0.05f;

    public void InitSkills(SkillConfig[] skills) {//this is called in skill controller
        this.skills = skills;
        AssignSkillImages();
        InitFillAmount();
        UpdateSkillText();
        InitSkillsCoolDown();
    }

    private void InitSkillsCoolDown() {
        curCooldowns = new float[skills.Length];
        curMaxCooldowns = new float[skills.Length];
        for (int i = 0; i < skills.Length; i++) {
            curCooldowns[i] = 0;
        }
    }

    private void AssignSkillImages() {
        for (int i = 0; i < skills.Length; i++) {
            skillCooldowns[i].type = Image.Type.Filled;
            skillIcons[i].sprite = skills[i].icon;
            skillCooldowns[i].sprite = skills[i].icon_grey;
        }
    }

    private void InitFillAmount() {
        for (int i = 0; i < skills.Length; i++) {
            if (skills[i] == null)
                continue;
            skillCooldowns[i].fillAmount = 0;
        }
    }

    private void UpdateSkillText() {
        for (int i = 0; i < skills.Length; i++) {
            energyCosts[i].text = (skills[i] == null) ? "0" : skills[i].GeneralSkillParams.energyCost.ToString(); ;
        }
    }

    private void Update() {
        for (int i = 0; i < skills.Length; i++) {
            if (curCooldowns[i] > 0) {
                if (curCooldowns[i] < MARGIN_OF_ERROR) {
                    curCooldowns[i] = 0;
                } else {
                    curCooldowns[i] -= Time.deltaTime;
                }
                skillCooldowns[i].fillAmount = curCooldowns[i] / curMaxCooldowns[i];
            }
        }
    }

    public void SetSkillCooldown(int skill_num, float MaxCooldown) {
        curCooldowns[skill_num] = MaxCooldown;
        curMaxCooldowns[skill_num] = MaxCooldown;
    }
}