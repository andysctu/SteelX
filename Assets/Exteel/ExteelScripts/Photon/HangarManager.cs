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
	[SerializeField] GameObject[] Mech_Display = new GameObject[4];
	[SerializeField] Sprite buttonTexture;
	[SerializeField] Button displaybutton1,displaybutton2;
	private string[] testParts = { "CES301", "LTN411", "HDS003", "AES707", "AES104", "PBS000", "SHL009", "APS403", "SHS309","RCL034", "BCN029","BRF025","SGN150","LMG012", "ENG041" };

	private Transform[] contents;
	private int activeTab;
	//private Dictionary<string, string> equipped;
	private string MechHandlerURL = "https://afternoon-temple-1885.herokuapp.com/mech";
	public int Mech_Num = 0;
	// Use this for initialization
	void Start () {
		Mech m = UserData.myData.Mech[Mech_Num];

		displaybutton1.onClick.AddListener (() => Mech_Display[Mech_Num].GetComponent<BuildMech>().DisplayFirstWeapons());
		displaybutton2.onClick.AddListener (() => Mech_Display[Mech_Num].GetComponent<BuildMech>().DisplaySecondWeapons());

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
				if (part [1] == 'M')
					parent = 5;
				else
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
					if ((p == "RCL034" || p == "BCN029") && (i == 1 || i == 3)) {
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
			parent = 0;
			UserData.myData.Mech[Mech_Num].Core = part;
			break;
		case 'A':
			if (part [1] == 'E') {
				parent = 1;
				UserData.myData.Mech[Mech_Num].Arms = part;
			}
			else {
				parent = 5;
			}
			break;
		case 'L':
			if (part [1] != 'M') {
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
			curSMR [parent].material = material;
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
					Debug.Log ("Should not get here");
					break;
				}
			Mech_Display[Mech_Num].GetComponent<BuildMech>().EquipWeapon(part, weap);
			}


		}
		//			curSMR[i].enabled = true;
//		for (int i = 0; i < curSMR.Length; i++){
//			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
//			curSMR[i].material = materials[i];
//			curSMR[i].enabled = true;
//		}
//		arm (new string[4]{parts[5],parts[6],parts[7],parts[8]});
	public void ChangeDisplayMech(int Num){
			
		Mech_Display [Mech_Num].SetActive (false);
		Mech_Display [Num].SetActive (true);
		Mech_Num = Num;
		displaybutton1.onClick.RemoveAllListeners ();
		displaybutton2.onClick.RemoveAllListeners ();

		displaybutton1.onClick.AddListener (() => Mech_Display[Num].GetComponent<BuildMech>().DisplayFirstWeapons());
		displaybutton2.onClick.AddListener (() => Mech_Display[Num].GetComponent<BuildMech>().DisplaySecondWeapons());

		Mech_Display [Mech_Num].GetComponent<BuildMech> ().CheckAnimatorState ();
	}
}

