using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
//Data uid , eid

public class StoreManager : SceneManager {

	[SerializeField] GameObject[] Tabs;
	[SerializeField] GameObject UIPart;
	[SerializeField] GameObject UIWeap;
	[SerializeField] GameObject Mech;
	[SerializeField] Sprite buttonTexture;
	private string[] AvailableParts = { "CES301", "LTN411", "HDS003", "AES707", "AES104", "PBS000", "SHL009", "APS403", "SHS309","RCL034", "BCN029","BRF025","SGN150","LMG012", "ENG041" };

	private string[] PartsInOrder = { "AES104", "CES301", "LTN411", "HDS003", "AES707", "PBS000", "SHS309" };
	private Transform[] contents;

	private int activeTab;
	private string MechHandlerURL = "https://afternoon-temple-1885.herokuapp.com/purchase";
	private int eid_to_pass;
	public int Mech_Num = 0;
    public static readonly string _sceneName = "Store";

    void Start () {
		//Mech m = UserData.myData.Mech[Mech_Num];

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
			
		foreach (string part in AvailableParts) {
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
			if (parent != 5) {
				uiPart.transform.Find("BuyButton").GetComponentInChildren<Button> ().onClick.AddListener (() => Buy (p));
				uiPart.transform.Find("EquipButton").GetComponentInChildren<Button> ().onClick.AddListener (() => Equip (p,-1));
			}else {
				uiPart.transform.Find("BuyButton").GetComponentInChildren<Button> ().onClick.AddListener (() => Buy (p));
				uiPart.transform.Find("EquipLButton").GetComponentInChildren<Button> ().onClick.AddListener (() => Equip (p,0));

				if (!(p == "RCL034" || p == "BCN029")) {
					uiPart.transform.Find ("EquipRButton").GetComponentInChildren<Button> ().onClick.AddListener (() => Equip (p, 1));
				}else{
					uiPart.transform.Find ("EquipRButton").gameObject.SetActive (false);//close equip R button
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
			//Quaternion newRot = Quaternion.Euler(new Vector3(Mech.transform.rotation.eulerAngles.x, Mech.transform.rotation.eulerAngles.y + 180, Mech.transform.rotation.eulerAngles.z));

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
		UnityEngine.SceneManagement.SceneManager.LoadScene ("Lobby");
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
			break;
		case 'A':
			if (part [1] == 'E') {
				parent = 1;
			}
			else {
				parent = 5;
			}
			break;
		case 'L':
			if (part [1] != 'M') {
				parent = 2; 
			} else
				parent = 5;
			break;
		case 'H':
			parent = 3;
			break;
		case 'P':
			parent = 4;
			break;
		default:
			parent = 5;
			break;
		}
		if (parent != 5) {
			curSMR[parent].sharedMesh = newSMR.sharedMesh;
			curSMR [parent].material = material;
		} else {
			Mech.GetComponent<BuildMech>().EquipWeapon(part, weap);
		}
	}
	//			curSMR[i].enabled = true;
	//		for (int i = 0; i < curSMR.Length; i++){
	//			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
	//			curSMR[i].material = materials[i];
	//			curSMR[i].enabled = true;
	//		}
	//		arm (new string[4]{parts[5],parts[6],parts[7],parts[8]});

	public void Buy(string part){
		WWWForm form = new WWWForm();

		form.AddField("uid", UserData.myData.User.Uid);
		form.AddField("eid", PartToEid(part));

		WWW www = new WWW(MechHandlerURL, form);

		while (!www.isDone) {}
		foreach (KeyValuePair<string,string> entry in www.responseHeaders) {
			Debug.Log(entry.Key + ": " + entry.Value);
		}
		string str = www.text;

		print ("get str : "+str);
		if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK" && bool.Parse(str)) {
			int i;
			string[] newOwns = new string[UserData.myData.Owns.Length + 1];
			for(i=0;i<UserData.myData.Owns.Length;i++){
				newOwns[i] = UserData.myData.Owns[i];
			}
			newOwns [i] = part;
			UserData.myData.Owns = newOwns;
			print ("buy success");
		} else {
			print ("buy failed.");
			//error.SetActive(true);
		}
	}

	int PartToEid(string part){
		int eid = -1;
		for(int i=0;i<PartsInOrder.Length;i++){
			if(PartsInOrder[i] == part){
				eid = i;
				break;
			}
		}

		return eid;
	}

    public override string GetSceneName() {
        return _sceneName;
    }
}

