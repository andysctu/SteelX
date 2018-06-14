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
    private string[] testParts = { "CES301", "LTN411", "HDS003", "AES707", "AES104", "PBS000" };
    private WeaponManager WeaponManager;
    private SkillManager SkillManager;
    private Transform[] contents;
    private int activeTab;
    //private Dictionary<string, string> equipped;
    private string MechHandlerURL = "https://afternoon-temple-1885.herokuapp.com/mech";
    public int Mech_Num = 0;

    private void Start() {
        WeaponManager = Resources.Load<WeaponManager>("WeaponManager");
        SkillManager = Resources.Load<SkillManager>("SkillManager");

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
        foreach (string part in testParts) {
            //		foreach (string part in UserData.myData.Owns) {
            int parent = -1;
            switch (part[0]) {
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
            Sprite s = Resources.Load<Sprite>(part);
            if (s == null) Debug.LogError(part + "'s sprite is missing");
            uiPart.GetComponentsInChildren<Image>()[1].sprite = s;
            uiPart.GetComponentInChildren<Text>().text = part;
            uiPart.GetComponentInChildren<Button>().onClick.AddListener(() => Equip(part, -1));
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
            uiPart.GetComponentInChildren<Text>().text = weaponName;

            Button[] btns = uiPart.GetComponentsInChildren<Button>();
            for (int i = 0; i < btns.Length; i++) {
                if ((weapon.twoHanded) && (i == 1 || i == 3)) {//if two handed , turn off equip on right hand
                    uiPart.transform.Find("Equip1r").gameObject.SetActive(false);
                    uiPart.transform.Find("Equip2r").gameObject.SetActive(false);

                    btns[i].image.enabled = false;
                    continue;
                }
                int n = i;
                btns[i].onClick.AddListener(() => Equip(weaponName, n));
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
            uiPart.transform.Find("weaponType").GetComponent<Text>().text = "L : "+skill.weaponTypeL + " / R : " + skill.weaponTypeR;

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

    public void Equip(string part, int weap) {
        Debug.Log("Equipping weap: " + weap);
        GameObject partGO = Resources.Load(part, typeof(GameObject)) as GameObject;//no need to load the weapon , just call equip
        SkinnedMeshRenderer newSMR = (partGO == null) ? null : partGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
        SkinnedMeshRenderer[] curSMR = Mech.GetComponentsInChildren<SkinnedMeshRenderer>();
        Material material = Resources.Load(part + "mat", typeof(Material)) as Material;

        int parent = -1;
        switch (part[0]) {
            case 'C':
            parent = 0;
            UserData.myData.Mech[Mech_Num].Core = part;
            break;
            case 'A':
            if (part[1] == 'E') {
                parent = 1;
                UserData.myData.Mech[Mech_Num].Arms = part;
            } else {
                parent = 5;
            }
            break;
            case 'L':
            if (part[1] != 'M') {
                parent = 2;
                UserData.myData.Mech[Mech_Num].Legs = part;
            } else
                parent = 5;
            break;
            case 'H':
            parent = 3;
            UserData.myData.Mech[Mech_Num].Head = part;
            break;
            case 'P':
            parent = 4;
            UserData.myData.Mech[Mech_Num].Booster = part;
            break;
            default:
            parent = 5;
            break;
        }
        if (parent != 5) {
            curSMR[parent].sharedMesh = newSMR.sharedMesh;
            curSMR[parent].material = material;
        } else {
            switch (weap) {
                case 0:
                UserData.myData.Mech[Mech_Num].Weapon1L = part;
                break;
                case 1:
                UserData.myData.Mech[Mech_Num].Weapon1R = part;
                break;
                case 2:
                UserData.myData.Mech[Mech_Num].Weapon2L = part;
                break;
                case 3:
                UserData.myData.Mech[Mech_Num].Weapon2R = part;
                break;
                default:
                Debug.Log("Should not get here");
                break;
            }
            Mech.GetComponent<BuildMech>().EquipWeapon(part, weap);
        }
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