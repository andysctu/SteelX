using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadOnClick : MonoBehaviour {

	public string LoginURL = "https://afternoon-temple-1885.herokuapp.com/login";
	public InputField[] fields;
	public Text error;

	private int focus = 0;

	void Start() {
		fields[0].Select();
		fields[0].ActivateInputField();
	}

	public void Login(){
		WWWForm form = new WWWForm();

		form.AddField("username", fields[0].text);
		form.AddField("password", fields[1].text);

		WWW www = new WWW(LoginURL, form);

		Debug.Log("Authenticating...");
		while (!www.isDone) {}
		foreach (KeyValuePair<string,string> entry in www.responseHeaders) {
			Debug.Log(entry.Key + ": " + entry.Value);
		}
			
		Debug.Log(www.text);
		if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK") {
			string json = www.text;
			Data d = JsonUtility.FromJson<Data>(json);
			UserData.myData = d;
			Application.LoadLevel (1);
		}
		else {
			error.gameObject.SetActive(true);
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			focus = (focus+1)%2;
			fields[focus].Select();
			fields[focus].ActivateInputField();
		}

		if (Input.GetKeyDown(KeyCode.Return)) {
			Login();
		}
	}
}
