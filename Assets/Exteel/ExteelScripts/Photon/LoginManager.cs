using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoginManager : MonoBehaviour {

	//public string LoginURL = "https://afternoon-temple-1885.herokuapp.com/login";
	public string LoginURL = "http://steelxdata.servegame.com/login.php";

	public InputField[] fields;
	public GameObject error;

	private int focus = 0;

	void Awake() {
		// the following line checks if this client was just created (and not yet online). if so, we connect
		if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
		{
			// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
			print("Connecting to server...");
			PhotonNetwork.ConnectUsingSettings("1.0");
		}
		// if you wanted more debug out, turn this on:
		// PhotonNetwork.logLevel = NetworkLogLevel.Full;
	}
	void OnConnectedToMaster(){
		print ("Connected to Server successfully.");
	}

	void Start() {
		fields[0].Select();
		fields[0].ActivateInputField();
	}

	public void Login(){
		WWWForm form = new WWWForm();

		if (fields [0].text.Length == 0) {
			fields [0].text = "andysctu";
			//fields [1].text = "password";
		} else {
			PhotonNetwork.playerName = fields [0].text;
		}
		form.AddField("username", fields[0].text);
		form.AddField("password", fields[1].text);

		//WWW www = new WWW(LoginURL, form);

		if (fields [1].text [0] != '0')
			return;
		
		Debug.Log("Authenticating...");

		print ("PlayerName :" + PhotonNetwork.playerName);

		/* 
		while (!www.isDone) {}
		foreach (KeyValuePair<string,string> entry in www.responseHeaders) {
			Debug.Log(entry.Key + ": " + entry.Value);
		}

		if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK") {
			string json = www.text;
			//Data test = new Data();
			//print(JsonUtility.ToJson (test));
			Data d = JsonUtility.FromJson<Data>(json);
			UserData.myData = d;
			UserData.myData.Mech.PopulateParts();
			PhotonNetwork.playerName = fields [0].text;
			Application.LoadLevel (1);
		} else {
			error.SetActive(true);
		}*/

		// for debug
		for (int i = 0; i < 4; i++) {
			UserData.myData.Mech [i].PopulateParts ();
		}
		PhotonNetwork.playerName = fields [0].text;
		Application.LoadLevel (1);
		//


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

	public void OnFailedToConnectToPhoton(object parameters)
	{
		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
	}
}
