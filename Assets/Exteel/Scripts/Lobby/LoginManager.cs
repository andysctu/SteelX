using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : SceneManager {
    [SerializeField] private Text ConnectingText;
    [SerializeField] private Button login_button;
    [SerializeField] private InputField[] fields;
    [SerializeField] private Dropdown serverToConnect;
    [SerializeField] private GameObject error;
    [SerializeField] private AudioClip loginMusic;
    private MusicManager MusicManager;
    private string region = "US";
    private int selectedInputField = 0;

    public const string _sceneName = "Login";
    public string LoginURL = "https://afternoon-temple-1885.herokuapp.com/login";
    public string gameVersion = "3.0";

    public override void StartScene() {
        base.StartScene();
        ResetInputFields();
        Application.targetFrameRate = 60;

        if (MusicManager == null)
            MusicManager = FindObjectOfType<MusicManager>();
        MusicManager.ManageMusic(loginMusic);
    }

    public void Login() {
        /*WWWForm form = new WWWForm();

		if (fields [0].text.Length == 0) {
			fields [0].text = "andysctu";
			fields [1].text = "password";
		} else {
			PhotonNetwork.playerName = fields [0].text;
		}
		form.AddField("username", fields[0].text);
		form.AddField("password", fields[1].text);

		WWW www = new WWW(LoginURL, form);

		Debug.Log("Authenticating...");

		print ("PlayerName :" + PhotonNetwork.playerName);

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
			UserData.myData.Mech0.PopulateParts();
			PhotonNetwork.playerName = fields [0].text;
			Application.LoadLevel (1);
		} else {
			//error.SetActive(true);
		}*/

        // for debug
        UserData.myData.Mech = new Mech[4];
        //UserData.region = FindRegionCode(region);
        UserData.version = gameVersion;
        for (int i = 0; i < 4; i++)UserData.myData.Mech[i].PopulateParts();        
        //PhotonNetwork.playerName = (string.IsNullOrEmpty(fields[0].text)) ? "Guest" + Random.Range(0, 9999) : fields[0].text;        

        ConnectToServerSelected();
        StartCoroutine(LoadLobbyWhenConnected());
    }

    private IEnumerator LoadLobbyWhenConnected() {
        int attempt_times = 0;
        login_button.interactable = false;
        ConnectingText.gameObject.SetActive(true);
        //check if connected
        //while (!PhotonNetwork.connected && attempt_times < 20) {
        //    attempt_times++;
        //    string dots = "";
        //    for (int j = 0; j <= attempt_times % 3; j++)//UI effect
        //        dots += ".";
        //    ConnectingText.color = Color.yellow;
        //    ConnectingText.text = "Connecting" + dots;
        //    yield return new WaitForSeconds(0.3f);
        //}

        if (attempt_times >= 20) {
            ConnectingText.color = Color.red;
            ConnectingText.text = "Failed to connect";
            login_button.interactable = true;
            yield break;
        } else {
            ConnectingText.color = Color.green;
            ConnectingText.text = "Connected";
            yield return new WaitForSeconds(0.3f);
        }

        login_button.interactable = true;
        SceneStateController.LoadScene(LobbyManager._sceneName);
        ConnectingText.gameObject.SetActive(false);
    }

    private void Update() {
        InputProcess();
    }

    private void InputProcess() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            selectedInputField = (selectedInputField + 1) % 2;
            fields[selectedInputField].Select();
            fields[selectedInputField].ActivateInputField();
        }

        if (Input.GetKeyDown(KeyCode.Return)) {
            Login();
        }
    }

    public void ChangeServerToConnect() {
        region = serverToConnect.captionText.text;
    }

    private void ConnectToServerSelected() {
        //PhotonNetwork.ConnectToRegion(FindRegionCode(region), gameVersion);
    }

    //private CloudRegionCode FindRegionCode(string region) {
    //    switch (region) {
    //        case "US":
    //        return CloudRegionCode.us;
    //        case "EU":
    //        return CloudRegionCode.eu;
    //        case "KR":
    //        return CloudRegionCode.kr;
    //        case "SA":
    //        return CloudRegionCode.sa;
    //        case "JP":
    //        return CloudRegionCode.jp;
    //        case "Asia":
    //        return CloudRegionCode.asia;
    //        default:
    //        return CloudRegionCode.jp;
    //    }
    //}

    private void ResetInputFields() {
        foreach (InputField i in fields)
            i.text = "";

        fields[0].Select();
        fields[0].ActivateInputField();
    }

    public override string GetSceneName() {
        return _sceneName;
    }

    public void ExitGame() {
        Application.Quit();
    }

    public void OnFailedToConnectToPhoton(object parameters) {
        //Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    private void OnConnectedToMaster() {
        print("Connected to Server successfully.");
    }
}

//  void Awake() {
//      // the following line checks if this client was just created (and not yet online). if so, we connect
//      if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
//{
//	// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
//	//print("Connecting to server...");
//          //PhotonNetwork.ConnectUsingSettings(gameVersion);
//      }
//      // if you wanted more debug out, turn this on:
//      // PhotonNetwork.logLevel = NetworkLogLevel.Full;
//  }