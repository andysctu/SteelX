using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HangarManager : MonoBehaviour {
    [SerializeField] private GameObject[] Tabs;
    [SerializeField] private GameObject UIPart, UIWeap, UISkill;
    [SerializeField] private GameObject Mech;
    [SerializeField] private Sprite buttonTexture;
    [SerializeField] private Button displaybutton1, displaybutton2;
    [SerializeField] private Image[] skill_slots;
    private WeaponManager WeaponManager;
    private SkillManager SkillManager;
    private MechPartManager MechPartManager;
    private Transform[] contents;
    private int activeTab;
    //private Dictionary<string, string> equipped;
    private string MechHandlerURL = "https://afternoon-temple-1885.herokuapp.com/mech";
    public int Mech_Num = 0;

    private void Start() {
        WeaponManager = Resources.Load<WeaponManager>("WeaponManager");
        SkillManager = Resources.Load<SkillManager>("SkillManager");
        MechPartManager = Resources.Load<MechPartManager>("MechPartManager");

        //Mech m = UserData.myData.Mech[0];

        displaybutton1.onClick.AddListener(() => Mech.GetComponent<BuildMech>().DisplayFirstWeapons());
        displaybutton2.onClick.AddListener(() => Mech.GetComponent<BuildMech>().DisplaySecondWeapons());

        Button[] buttons = GameObject.FindObjectsOfType<Button>();
        foreach (Button b in buttons) {
            b.image.overrideSprite = buttonTexture;
            b.GetComponentInChildren<Text>().color = Color.white;
        }

        activeTab = 0;
        contents = new Transform[Tabs.Length];
        for (int i = 0; i < Tabs.Length; i++) {
            Transform tabTransform = Tabs[i].transform;
            GameObject smallTab = tabTransform.Find("Tab").gameObject;
            Color c = new Color(255, 255, 255, 63);
            smallTab.GetComponent<Image>().color = c;
            int index = i;
            smallTab.GetComponent<Button>().onClick.AddListener(() => activateTab(index));
            GameObject pane = tabTransform.Find("Pane").gameObject;
            pane.SetActive(i == 0);
            contents[i] = pane.transform.Find("Viewport/Content");
        }

        // Debug, take out
        Part[] Parts = MechPartManager.GetAllParts();
        foreach (Part part in Parts) {
            //foreach (string part in UserData.myData.Owns) {
            int parent = -1;
            switch (part.name[0]) {
                case 'H':
                parent = 0;
                break;
                case 'C':
                parent = 1;
                break;
                case 'A':
                parent = 2;
                break;
                case 'L':
                parent = 3;
                break;
                case 'P':
                parent = 4;
                break;
                default:
                Debug.LogError("Can not catagorize " + part);
                break;
            }
            GameObject uiPart;
            uiPart = Instantiate(UIPart, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            uiPart.transform.SetParent(contents[parent]);
            uiPart.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            uiPart.GetComponent<RectTransform>().position = new Vector3(0, 0, 0);
            Sprite s = Resources.Load<Sprite>(part.name);
            if (s == null) Debug.Log(part + "'s sprite is missing");
            uiPart.GetComponentsInChildren<Image>()[1].sprite = s;
            uiPart.GetComponentInChildren<Text>().text = part.displayName;
            uiPart.GetComponentInChildren<Button>().onClick.AddListener(() => EquipMechPart(part.name));
        }

        LoadWeapons();
        LoadSkills();
        DisplayPlayerSkills();
    }

    private void LoadWeapons() {
        foreach (Weapon weapon in WeaponManager.GetAllWeaponss()) {
            string weaponName = weapon.weaponPrefab.name;
            GameObject uiPart = Instantiate(UIWeap, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            uiPart.transform.SetParent(contents[5]);
            uiPart.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            uiPart.GetComponent<RectTransform>().position = new Vector3(0, 0, 0);
            Sprite s = Resources.Load<Sprite>(weaponName);
            if (s == null) Debug.Log(weapon + "'s sprite is missing");
            uiPart.GetComponentsInChildren<Image>()[1].sprite = s;
            uiPart.GetComponentInChildren<Text>().text = weapon.displayName == "" ? weaponName : weapon.displayName;

            Button[] btns = uiPart.GetComponentsInChildren<Button>();
            for (int i = 0; i < btns.Length; i++) {
                if ((weapon.twoHanded) && (i == 1 || i == 3)) {//if two handed , turn off equip on right hand
                    uiPart.transform.Find("Equip1r").gameObject.SetActive(false);
                    uiPart.transform.Find("Equip2r").gameObject.SetActive(false);

                    btns[i].image.enabled = false;
                    continue;
                }
                int n = i;
                btns[i].onClick.AddListener(() => EquipWeapon(weaponName, n));
            }
        }
    }

    private void LoadSkills() {
        foreach (SkillConfig skill in SkillManager.GetAllSkills()) {
            string skillName = skill.name;//TODO : don't use .name

            GameObject uiPart = Instantiate(UISkill, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            uiPart.transform.SetParent(contents[6]);
            uiPart.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            uiPart.GetComponent<RectTransform>().position = new Vector3(0, 0, 0);
            Sprite s = skill.icon;
            if (s == null) Debug.Log(skill.name + "'s sprite is missing");
            uiPart.GetComponentsInChildren<Image>()[1].sprite = s;
            uiPart.transform.Find("skillname").GetComponent<Text>().text = skillName;
            uiPart.transform.Find("weaponType").GetComponent<Text>().text = "L : " + skill.weaponTypeL + " / R : " + skill.weaponTypeR;

            Button[] btns = uiPart.GetComponentsInChildren<Button>();
            for (int i = 0; i < btns.Length; i++) {
                int n = i;
                btns[i].onClick.AddListener(() => EquipSkill(skill.GetID(), n));
            }
        }
    }

    private void DisplayPlayerSkills() {
        if (UserData.myData.Mech[Mech_Num].skillIDs != null) {
            for (int i = 0; i < UserData.myData.Mech[Mech_Num].skillIDs.Length; i++) {
                if (UserData.myData.Mech[Mech_Num].skillIDs[i] != -1) {
                    EquipSkill(UserData.myData.Mech[Mech_Num].skillIDs[i], i);
                } else {
                    EquipSkill(-1, i);
                }
            }
        }
    }

    private void activateTab(int index) {
        for (int i = 0; i < Tabs.Length; i++) {
            Transform tabTransform = Tabs[i].transform;
            GameObject smallTab = tabTransform.Find("Tab").gameObject;
            Color c = new Color(255, 255, 255, 63);
            smallTab.GetComponent<Image>().color = c;
            tabTransform.Find("Pane").gameObject.SetActive(i == index);
        }
        if ((index == 4 && activeTab != 4) || (index != 4 && activeTab == 4)) {
            Debug.Log("rotating");

            //create the rotation we need to be in to look at the target
            Quaternion newRot = Quaternion.Euler(new Vector3(Mech.transform.rotation.eulerAngles.x, Mech.transform.rotation.eulerAngles.y + 180, Mech.transform.rotation.eulerAngles.z));

            Vector3 rot = Mech.transform.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y + 180, rot.z);

            //			Debug.Log ("newRot: " + newRot.x + ", " + newRot.y + ", " + newRot.z);
            //			Debug.Log ("rot: " + Mech.transform.rotation.x + ", " + Mech.transform.rotation.y + ", " + Mech.transform.rotation.z);
            //			Debug.Log ("localRot: " + Mech.transform.localRotation.x + ", " + Mech.transform.localRotation.y + ", " + Mech.transform.localRotation.z);
            //rotate towards a direction, but not immediately (rotate a little every frame)
            Mech.transform.rotation = Quaternion.Slerp(Mech.transform.rotation, Quaternion.Euler(rot), Time.deltaTime * 0.5f);
        }
        activeTab = index;
    }

    public void Back() {
        // Save mech
        //WWWForm form = new WWWForm();

        /*form.AddField("uid", UserData.myData.User.Uid);
		foreach (KeyValuePair<string, string> entry in equipped) {
			form.AddField(entry.Key, entry.Value);
		}*/

        /*
		WWW www = new WWW(MechHandlerURL, form);
		while (!www.isDone) {}*/
        /*
		if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK") {
			string json = www.text;
			Mech m = JsonUtility.FromJson<Mech>(json);
			UserData.myData.Mech = m;
			UserData.myData.Mech.PopulateParts();
		}*/

        // Return to lobby
        SceneManager.LoadScene("Lobby");
    }

    public void EquipMechPart(string part_name) {
        Part part = MechPartManager.FindData(part_name);

        if (part != null) {
            GameObject partPrefab = part.GetPartPrefab();

            if (partPrefab == null) {
                Debug.Log("Null part prefab");
                return;
            }

            string mechPartToReplace = "";
            int parent = -1;
            if (part_name[0] != 'P') {
                SkinnedMeshRenderer newSMR = (partPrefab == null) ? null : partPrefab.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
                SkinnedMeshRenderer[] curSMR = Mech.GetComponentsInChildren<SkinnedMeshRenderer>();
                Material material = Resources.Load("MechPartMaterials/"+part_name + "mat", typeof(Material)) as Material;

                switch (part_name[0]) {
                    case 'C':
                    parent = 0;
                    mechPartToReplace = UserData.myData.Mech[Mech_Num].Core;
                    UserData.myData.Mech[Mech_Num].Core = part_name;
                    break;
                    case 'A':
                    parent = 1;
                    mechPartToReplace = UserData.myData.Mech[Mech_Num].Arms;
                    UserData.myData.Mech[Mech_Num].Arms = part_name;
                    break;
                    case 'L':
                    parent = 2;
                    mechPartToReplace = UserData.myData.Mech[Mech_Num].Legs;
                    UserData.myData.Mech[Mech_Num].Legs = part_name;                       
                    break;
                    case 'H':
                    parent = 3;
                    mechPartToReplace = UserData.myData.Mech[Mech_Num].Head;
                    UserData.myData.Mech[Mech_Num].Head = part_name;
                    break;
                    default:
                        Debug.Log("Can't catorize this : "+ part_name);
                    break;
                }
                curSMR[parent].sharedMesh = newSMR.sharedMesh;
                curSMR[parent].material = material;

            } else {//Booster
                parent = 4;
                mechPartToReplace = UserData.myData.Mech[Mech_Num].Booster;
                UserData.myData.Mech[Mech_Num].Booster = part_name;

                Transform boosterbone = Mech.transform.Find("CurrentMech/metarig/hips/spine/chest/neck/boosterBone");
                if (boosterbone != null) {
                    GameObject booster = (boosterbone.childCount == 0) ? null : boosterbone.GetChild(0).gameObject;
                    if (booster != null) {//destroy previous 
                        DestroyImmediate(booster);
                    }                   

                    GameObject newBooster = Instantiate(partPrefab, boosterbone);
                    newBooster.transform.localPosition = Vector3.zero;
                    newBooster.transform.localRotation = Quaternion.Euler(90, 0, 0);
                }
            }
            Mech.GetComponent<BuildMech>().ReplaceMechPart(mechPartToReplace, part_name);

        }
    }

    public void EquipWeapon(string weapon_name, int weap) {
        switch (weap) {
            case 0:
            UserData.myData.Mech[Mech_Num].Weapon1L = weapon_name;
            break;
            case 1:
            UserData.myData.Mech[Mech_Num].Weapon1R = weapon_name;
            break;
            case 2:
            UserData.myData.Mech[Mech_Num].Weapon2L = weapon_name;
            break;
            case 3:
            UserData.myData.Mech[Mech_Num].Weapon2R = weapon_name;
            break;
            default:
            Debug.Log("Should not get here");
            break;
        }
        Mech.GetComponent<BuildMech>().EquipWeapon(weapon_name, weap);
    }

    public void EquipSkill(int skill_id, int skill_pos) {
        UserData.myData.Mech[Mech_Num].skillIDs[skill_pos] = skill_id;
        Debug.Log("equip skill : " + skill_id + " on skill_pos :" + skill_pos);

        if (skill_id == -1) {
            skill_slots[skill_pos].sprite = null;
            skill_slots[skill_pos].gameObject.SetActive(false);
        } else {
            skill_slots[skill_pos].sprite = SkillManager.GetSkillConfig(skill_id).icon;
            skill_slots[skill_pos].gameObject.SetActive(true);
        }
    }

    public void ChangeDisplayMech(int Num) {
        BuildMech bm = Mech.GetComponent<BuildMech>();
        if (bm == null)
            return;
        Mech_Num = Num;
        Mech m = UserData.myData.Mech[Num];
        bm.Mech_Num = Num;
        bm.buildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
        bm.DisplayFirstWeapons();
        bm.CheckAnimatorState();

        DisplayPlayerSkills();
    }
}