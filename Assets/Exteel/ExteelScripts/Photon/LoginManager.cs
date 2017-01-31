using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoginManager : MonoBehaviour {

	public string LoginURL = "https://afternoon-temple-1885.herokuapp.com/login";
	public InputField[] fields;
	public GameObject error;

	private int focus = 0;

	void Awake() {
		// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
		PhotonNetwork.automaticallySyncScene = true;

		// the following line checks if this client was just created (and not yet online). if so, we connect
		if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
		{
			// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
			PhotonNetwork.ConnectUsingSettings("0.9");
		}

		// generate a name for this player, if none is assigned yet
		if (string.IsNullOrEmpty(PhotonNetwork.playerName))
		{
			PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
		}

		// if you wanted more debug out, turn this on:
		// PhotonNetwork.logLevel = NetworkLogLevel.Full;
	}

	void Start() {
		fields[0].Select();
		fields[0].ActivateInputField();
	}

	public void Login(){
		WWWForm form = new WWWForm();

		if (fields[0].text.Length == 0) {
			fields[0].text = "andysctu";
			fields[1].text = "password";
		}
		form.AddField("username", fields[0].text);
		form.AddField("password", fields[1].text);

		WWW www = new WWW(LoginURL, form);

		Debug.Log("Authenticating...");
		while (!www.isDone) {}
//		foreach (KeyValuePair<string,string> entry in www.responseHeaders) {
//			Debug.Log(entry.Key + ": " + entry.Value);
//		}

		if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK") {
			string json = www.text;
			Data d = JsonUtility.FromJson<Data>(json);
			UserData.myData = d;
			UserData.myData.Mech.PopulateParts();
			PhotonNetwork.playerName = fields [0].text;
			Application.LoadLevel (1);
		} else {
			error.SetActive(true);
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

	public void OnFailedToConnectToPhoton(object parameters)
	{
		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
	}
}
