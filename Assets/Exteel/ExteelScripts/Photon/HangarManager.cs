using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class HangarManager : MonoBehaviour {

	[SerializeField] GameObject[] Tabs;
	[SerializeField] GameObject UIPart;
	[SerializeField] GameObject Mech;

	private string[] testParts = { "CES301", "LTN411", "HDS003", "AES707", "AES104", "PBS000", "SHL009", "APS403" };

	private Transform[] contents;
	private int activeTab;

	// Use this for initialization
	void Start () {
		activeTab = 0;
		contents = new Transform[Tabs.Length];
		for (int i = 0; i < Tabs.Length; i++) {
			Transform tabTransform = Tabs [i].transform;
			GameObject smallTab = tabTransform.FindChild ("Tab").gameObject;
			Color c = new Color (255, 255, 255, 63);
			smallTab.GetComponent<Image> ().color = c;
			int index = i;
			smallTab.GetComponent<Button> ().onClick.AddListener (() => activateTab(index));
			GameObject pane = tabTransform.FindChild ("Pane").gameObject;
			pane.SetActive (i == 0);
			contents[i] = pane.transform.FindChild("Viewport/Content");
		}

		foreach (string part in testParts) {
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
			GameObject uiPart = Instantiate(UIPart, new Vector3(0,0,0), Quaternion.identity) as GameObject;
			uiPart.transform.SetParent(contents[parent]);
			uiPart.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
			uiPart.GetComponent<RectTransform>().position = new Vector3(0,0,0);
			Sprite s = Resources.Load<Sprite>(part);
			uiPart.GetComponentsInChildren<Image>()[1].sprite = s;
			uiPart.GetComponentInChildren<Text>().text = part;
			string p = part;
			uiPart.GetComponentInChildren<Button> ().onClick.AddListener (() => Equip(p));
		}

	}

	private void activateTab(int index) {
		for (int i = 0; i < Tabs.Length; i++) {
			Transform tabTransform = Tabs [i].transform;
			GameObject smallTab = tabTransform.FindChild ("Tab").gameObject;
			Color c = new Color (255, 255, 255, 63);
			smallTab.GetComponent<Image> ().color = c;
			tabTransform.FindChild ("Pane").gameObject.SetActive (i == index);
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
		SceneManager.LoadScene ("Lobby");
	}

	public void Equip(string part) {
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
			if (part [1] == 'E')
				parent = 1;
			else
				parent = 5;
			break;
		case 'L':
			parent = 2;
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
		Debug.Log (part);
		Debug.Log (curSMR.Length);
		curSMR[parent].sharedMesh = newSMR.sharedMesh;
		curSMR [parent].material = material;
		//			curSMR[i].enabled = true;
//		for (int i = 0; i < curSMR.Length; i++){
//			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
//			curSMR[i].material = materials[i];
//			curSMR[i].enabled = true;
//		}
//		arm (new string[4]{parts[5],parts[6],parts[7],parts[8]});
	}
}
