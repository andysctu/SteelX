using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HangarManager : MonoBehaviour {

	[SerializeField] GameObject[] Tabs;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < Tabs.Length; i++) {
			Transform tabTransform = Tabs [i].transform;
			GameObject smallTab = tabTransform.FindChild ("Tab").gameObject;
			Color c = new Color (255, 255, 255, 63);
			smallTab.GetComponent<Image> ().color = c;
			int index = i;
			smallTab.GetComponent<Button> ().onClick.AddListener (() => activateTab(index));
			tabTransform.FindChild ("Pane").gameObject.SetActive (i == 0);
		}
		Debug.Log (UserData.myData.Mech.Arms);
	}

	private void activateTab(int index) {
		for (int i = 0; i < Tabs.Length; i++) {
			Transform tabTransform = Tabs [i].transform;
			GameObject smallTab = tabTransform.FindChild ("Tab").gameObject;
			Color c = new Color (255, 255, 255, 63);
			smallTab.GetComponent<Image> ().color = c;
			tabTransform.FindChild ("Pane").gameObject.SetActive (i == index);
		}
	}
}
