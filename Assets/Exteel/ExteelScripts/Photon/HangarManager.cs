using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HangarManager : MonoBehaviour {

	[SerializeField] GameObject[] Tabs;
	[SerializeField] GameObject UIPart;
	[SerializeField] GameObject UIWeap;
	[SerializeField] GameObject Mech;
	[SerializeField] Sprite buttonTexture;

	private string[] testParts = { "CES301", "LTN411", "HDS003", "AES707", "AES104", "PBS000", "SHL009", "APS403", "SHS309","RCL034" };

	private Transform[] contents;
	private int activeTab;
	private Dictionary<string, string> equipped;
	private string MechHandlerURL = "https://afternoon-temple-1885.herokuapp.com/mech";


	// Use this for initialization
	void Start () {
		equipped = new Dictionary<string, string>();
		Mech m = UserData.myData.Mech;
		equipped.Add("head", m.Head);
		equipped.Add("arms", m.Arms);
		equipped.Add("legs", m.Legs);
		equipped.Add("core", m.Core);
		equipped.Add("weapon1l", m.Weapon1L);
		equipped.Add("weapon1r", m.Weapon1R);
		equipped.Add("weapon2l", m.Weapon2L);
		equipped.Add("weapon2r", m.Weapon2R);
		equipped.Add("booster", m.Booster);

		Button[] buttons = GameObject.FindObjectsOfType<Button>();
		foreach (Button b in buttons) {
			b.image.overrideSprite = buttonTexture;
			b.GetComponentInChildren<Text>().color = Color.white;
		}

		activeTab = 0;
		contents = new Transform[Tabs.Length];
		for (int i = 0; i < Tabs.Length; i++) {
			Transform tabTransform = Tabs [i].transform;
			GameObject smallTab = tabTransform.Find ("Tab").gameObject;
			Color c = new Color (255, 255, 255, 63);
			smallTab.GetComponent<Image> ().color = c;
			int index = i;
			smallTab.GetComponent<Button> ().onClick.AddListener (() => activateTab(index));
			GameObject pane = tabTransform.Find ("Pane").gameObject;
			pane.SetActive (i == 0);
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
				if (part[1] == 'E')
					parent = 2;
				else
					parent = 5;
				break;
			case 'L':
				parent = 3;
				break;
			case 'P':
				parent = 4;
				break;
			default:
				parent = 5;
				break;
			}
			GameObject uiPart;
			if (parent != 5) uiPart = Instantiate(UIPart, new Vector3(0,0,0), Quaternion.identity) as GameObject;
			else uiPart = Instantiate(UIWeap, new Vector3(0,0,0), Quaternion.identity) as GameObject;
			uiPart.transform.SetParent(contents[parent]);
			uiPart.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
			uiPart.GetComponent<RectTransform>().position = new Vector3(0,0,0);
			Sprite s = Resources.Load<Sprite>(part);
			uiPart.GetComponentsInChildren<Image>()[1].sprite = s;
			uiPart.GetComponentInChildren<Text>().text = part;
			string p = part;
			if (parent !=5) uiPart.GetComponentInChildren<Button> ().onClick.AddListener (() => Equip(p,-1));
			else {
				Button[] btns = uiPart.GetComponentsInChildren<Button>();
				for (int i = 0; i < btns.Length; i++) {

					//if two-handed , skip 1 &3  temp.
					if (p == "RCL034" && (i == 1 || i == 3)){
						btns [i].image.enabled = false;
						continue;
					}

					int copy = i;
					Button b = btns[i];
					b.onClick.AddListener(() => Equip(p,copy));
				}
			}
		}

	}

	private void activateTab(int index) {
		for (int i = 0; i < Tabs.Length; i++) {
			Transform tabTransform = Tabs [i].transform;
			GameObject smallTab = tabTransform.Find ("Tab").gameObject;
			Color c = new Color (255, 255, 255, 63);
			smallTab.GetComponent<Image> ().color = c;
			tabTransform.Find ("Pane").gameObject.SetActive (i == index);
		}
		if ((index == 4 && activeTab != 4) ||(index != 4 && activeTab == 4)) {
			Debug.Log ("rotating");

			//create the rotation we need to be in to look at the target
			Quaternion newRot = Quaternion.Euler(new Vector3(Mech.transform.rotation.eulerAngles.x, Mech.transform.rotation.eulerAngles.y + 180, Mech.transform.rotation.eulerAngles.z));

			Vector3 rot = Mech.transform.rotation.eulerAngles;
			rot = new Vector3(rot.x,rot.y+180,rot.z);

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
		SceneManager.LoadScene ("Lobby");
	}

	public void Equip(string part, int weap) {
		Debug.Log("Equipping weap: " + weap);
		GameObject partGO = Resources.Load (part, typeof(GameObject)) as GameObject;
		SkinnedMeshRenderer newSMR = partGO.GetComponentInChildren<SkinnedMeshRenderer> () as SkinnedMeshRenderer;
		SkinnedMeshRenderer[] curSMR = Mech.GetComponentsInChildren<SkinnedMeshRenderer> ();
		Material material = Resources.Load (part + "mat", typeof(Material)) as Material;

		int parent = -1;
		switch (part [0]) {
		case 'C':
			parent = 0; equipped["core"] = part;
			UserData.myData.Mech.Core = part;
			break;
		case 'A':
			if (part [1] == 'E') {
				parent = 1; equipped["arms"] = part;
				UserData.myData.Mech.Arms = part;
			}
			else {
				parent = 5;
			}
			break;
		case 'L':
			parent = 2; equipped["legs"] = part;
			UserData.myData.Mech.Legs = part;
			break;
		case 'H':
			parent = 3; equipped["head"] = part;
			UserData.myData.Mech.Head = part;
			break;
		case 'P':
			parent = 4; equipped["booster"] = part;
			UserData.myData.Mech.Booster = part;
			break;
		default:
			parent = 5;
			break;
		}
		if (parent != 5) {
			curSMR[parent].sharedMesh = newSMR.sharedMesh;
			curSMR [parent].material = material;
		} else {
			/*
			if (part == "RCL034") { //temp , check if it is two handed weapon
				switch (weap) { //set to left hand
				case 0:
				case 1:
					equipped ["weapon1l"] = part;
					UserData.myData.Mech.Weapon1L = part;
					equipped ["weapon1r"] = "EmptyWeapon"; 
					UserData.myData.Mech.Weapon1R = "EmptyWeapon";
					GameObject.Find("MechFrame").GetComponent<BuildMech>().EquipWeapon(part, 0);
					Mech.GetComponentInChildren<Animator> ().SetBool ("UsingRCL", true);
				break;
				case 2:
				case 3:
					equipped ["weapon2l"] = part;
					UserData.myData.Mech.Weapon2L = part;
					equipped ["weapon2r"] = "EmptyWeapon"; 
					UserData.myData.Mech.Weapon2R = "EmptyWeapon";
					GameObject.Find("MechFrame").GetComponent<BuildMech>().EquipWeapon(part, 2);
					//Mech.GetComponentInChildren<Animator> ().SetBool ("UsingRCL", true);   //2l,2r do not show , currently
				break;
				default:
					Debug.Log ("Should not get here");
					break;
				}
			} else {
				//check if previous is two-handed (always on left hand)
				//the weapon is destroyed in BuildMech
				if(weap>=2){
					//check 2l
					if(equipped["weapon2l"] == "RCL034"){
						//since 2l,2r do not show 
						//Mech.GetComponentInChildren<Animator> ().SetBool ("UsingRCL", false);
					}
				}else{
					//check 1l
					if(equipped["weapon1l"] == "RCL034"){
						Mech.GetComponentInChildren<Animator> ().SetBool ("UsingRCL", false);
					}
				}*/


				switch (weap) {
				case 0:
					equipped ["weapon1l"] = part;
					UserData.myData.Mech.Weapon1L = part;
					break;
				case 1: 
					equipped ["weapon1r"] = part; 
					UserData.myData.Mech.Weapon1R = part;
					break;
				case 2:
					equipped ["weapon2l"] = part;
					UserData.myData.Mech.Weapon2L = part;
					break;
				case 3:
					equipped ["weapon2r"] = part; 
					UserData.myData.Mech.Weapon2R = part;
					break;
				default:
					Debug.Log ("Should not get here");
					break;
				}
				GameObject.Find("MechFrame").GetComponent<BuildMech>().EquipWeapon(part, weap);
			}


		}
		//			curSMR[i].enabled = true;
//		for (int i = 0; i < curSMR.Length; i++){
//			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
//			curSMR[i].material = materials[i];
//			curSMR[i].enabled = true;
//		}
//		arm (new string[4]{parts[5],parts[6],parts[7],parts[8]});
	}

