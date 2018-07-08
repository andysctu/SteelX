using UnityEngine;
using UnityEngine.UI;

public class OperatorStatsUI : MonoBehaviour {
    [SerializeField] private Transform MechInfoStats;
    [SerializeField] private BuildMech Mech;
    [SerializeField] private MechPartManager MechPartManager;
    [SerializeField] private WeaponManager WeaponManager;
    [SerializeField] private Text playerName;

    private MechProperty curMechProperty;
    private Part[] MechParts = new Part[5];
    private Weapon[] MechWeapons = new Weapon[4];
    private Text[] stat_texts = null, stat_labels = null, stat_differences = null;
    private Color32 BLUE = new Color32(39, 67, 253, 255), RED = new Color32(248, 84, 84, 255);
    private string[] STAT_LABELS = new string[18] {
       "HP","EN","SP","MPU","Size","Weight","Move Speed","Dash Speed","EN Recovery","Min. EN Required",
        "Dash EN Drain","Jump EN Drain","Dash Accel","Dash Decel","Max Heat","Cooldown Rate","Scan Range","Marksmanship"
    };

    private void Awake() {
        stat_texts = new Text[MechInfoStats.childCount];
        stat_labels = new Text[MechInfoStats.childCount];
        stat_differences = new Text[MechInfoStats.childCount];

        for (int i = 0; i < MechInfoStats.childCount; i++) {
            stat_texts[i] = MechInfoStats.GetChild(i).Find("Stats").GetComponent<Text>();
            stat_labels[i] = MechInfoStats.GetChild(i).Find("Label").GetComponent<Text>();

            //Init labels
            stat_labels[i].text = STAT_LABELS[i];
            stat_differences[i] = MechInfoStats.GetChild(i).Find("Change/Difference").GetComponent<Text>();
            stat_differences[i].enabled = false;
        }
    }

    private void OnEnable() {
        playerName.text = PhotonNetwork.player.NickName;
    }

    public void DisplayMechProperties() {
        curMechProperty = Mech.MechProperty;
        MechParts = Mech.curMechParts;

        int[] MechPropertiesArray = TransformMechPropertiesToArray(curMechProperty, CalculateTotalWeight(MechParts, Mech.weaponScripts));
        for (int i = 0; i < MechPropertiesArray.Length; i++) {
            stat_texts[i].text = MechPropertiesArray[i].ToString();
        }
        ClearAllDiff();
    }

    public void PreviewMechProperty(string part, bool isWeapon) {
        Part[] tmpParts = (Part[])MechParts.Clone();

        if (isWeapon) {
            Weapon newWeap = WeaponManager.FindData(part);
        } else {
            Part newPart = MechPartManager.FindData(part);

            System.Type[] partTypes = new System.Type[5] { typeof(Head), typeof(Core), typeof(Arm), typeof(Leg), typeof(Booster) };

            for (int i = 0; i < 5; i++) {
                if (MechPartManager.GetPartType(part) == partTypes[i]) {//which type is this part
                    for (int j = 0; j < 5; j++) {
                        if (MechPartManager.GetPartType(MechParts[j].name) == partTypes[i]) {//find the corresponding part in MechParts
                            tmpParts[j] = MechPartManager.FindData(part);//switch
                            break;
                        }
                    }
                    break;
                }
            }
            MechProperty newMechProperty = new MechProperty();
            //Load all property info
            for (int i = 0; i < 5; i++) {
                if (tmpParts[i] != null) {
                    tmpParts[i].LoadPartInfo(ref newMechProperty);
                }
            }

            //show diff
            int[] curMechPropertiesArray = TransformMechPropertiesToArray(curMechProperty, CalculateTotalWeight(MechParts, Mech.weaponScripts));
            int[] newMechPropertiesArray = TransformMechPropertiesToArray(newMechProperty, CalculateTotalWeight(tmpParts, Mech.weaponScripts));

            for (int j = 0; j < 18; j++) {
                if (j == 4 || j == 5 || j == 9 || j == 10 || j == 11) {
                    stat_differences[j].text = (newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? "▲" : "▼") + (Mathf.Abs(newMechPropertiesArray[j] - curMechPropertiesArray[j])).ToString();
                    stat_differences[j].color = newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? RED : BLUE;
                } else {
                    stat_differences[j].text = (newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? "▲" : "▼") + (Mathf.Abs(newMechPropertiesArray[j] - curMechPropertiesArray[j])).ToString();
                    stat_differences[j].color = newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? BLUE : RED;
                }
                stat_differences[j].enabled = (newMechPropertiesArray[j] - curMechPropertiesArray[j] != 0);
            }
        }
    }

    private int[] TransformMechPropertiesToArray(MechProperty mechProperty, int totalWeight) {
        int[] PropertiesArray = new int[STAT_LABELS.Length];
        PropertiesArray[0] = mechProperty.HP;
        PropertiesArray[1] = mechProperty.EN;
        PropertiesArray[2] = mechProperty.SP;
        PropertiesArray[3] = mechProperty.MPU;
        PropertiesArray[4] = mechProperty.Size;
        PropertiesArray[5] = mechProperty.Weight;
        PropertiesArray[6] = (int)mechProperty.GetMoveSpeed(totalWeight);
        Debug.Log("speed : " + PropertiesArray[6]);
        PropertiesArray[7] = (int)mechProperty.GetDashSpeed(totalWeight);
        PropertiesArray[8] = mechProperty.ENOutputRate;
        PropertiesArray[9] = mechProperty.MinENRequired;
        PropertiesArray[10] = mechProperty.DashENDrain;
        PropertiesArray[11] = (int)mechProperty.GetJumpENDrain(totalWeight);
        PropertiesArray[12] = (int)mechProperty.GetDashAcceleration(totalWeight);
        PropertiesArray[13] = (int)mechProperty.GetDashDecelleration(totalWeight);
        PropertiesArray[14] = mechProperty.MaxHeat;
        PropertiesArray[15] = mechProperty.CooldownRate;
        PropertiesArray[16] = mechProperty.ScanRange;
        PropertiesArray[17] = mechProperty.Marksmanship;

        return PropertiesArray;
    }

    private int CalculateTotalWeight(Part[] parts, Weapon[] weapons) {
        int totalweight = 0;
        for (int i = 0; i < parts.Length; i++) {
            totalweight += parts[i].Weight;
        }

        totalweight += (weapons[Mech.GetWeaponOffset()] == null) ? 0 : weapons[Mech.GetWeaponOffset()].weight;
        totalweight += (weapons[Mech.GetWeaponOffset() + 1] == null) ? 0 : weapons[Mech.GetWeaponOffset() + 1].weight;
        return totalweight;
    }

    private void ClearAllDiff() {
        for (int i = 0; i < 18; i++) {
            stat_differences[i].enabled = false;
        }
    }
}