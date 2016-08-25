using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class HangarManager : MonoBehaviour {

	[SerializeField] GameObject[] Tabs;

	private string[] testParts = { "CES301", "LTN411", "HDS003", "AES707", "AES104", "PBS000" };

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
		foreach (string part in testParts) {
			switch (part [0]) {
			case 'H':
				Debug.Log ("Head");
				break;
			}


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
	}

	public void Back() {
		SceneManager.LoadScene ("Lobby");
	}
}
