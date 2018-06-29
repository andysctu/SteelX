using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoginManager : MonoBehaviour {
    [SerializeField]private Text ConnectingText;
    [SerializeField]private Button login_button;
    private MySceneManager MySceneManager;
    private Coroutine connectionCoroutine = null;
    private string region = "US" , gameVersion = "1.7";
    private int focus = 0;  

	public InputField[] fields;
	public GameObject error;
    public Dropdown serverToConnect;
    public string LoginURL = "https://afternoon-temple-1885.herokuapp.com/login";

    void Awake() {
        MySceneManager = FindObjectOfType<MySceneManager>();

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
		{
			// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
			//print("Connecting to server...");
            //PhotonNetwork.ConnectUsingSettings(gameVersion);
        }
        // if you wanted more debug out, turn this on:
        // PhotonNetwork.logLevel = NetworkLogLevel.Full;

         Application.targetFrameRate = 60;//TODO : consider remake this
    }

	void OnConnectedToMaster(){
		print ("Connected to Server successfully.");
	}

	void Start() {
		fields[0].Select();
		fields[0].ActivateInputField();
	}

	public void Login(){
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
		UserData.myData.Mech = new Mech[4]; // receiving Json will set the array to null
		for (int i = 0; i < 4; i++) {
			UserData.myData.Mech [i].PopulateParts ();
		}
		PhotonNetwork.playerName = fields [0].text;
        ConnectToServerSelected();

        if (connectionCoroutine!=null)StopCoroutine(connectionCoroutine);
        connectionCoroutine = StartCoroutine(LoadLobbyWhenConnected());

    }

    IEnumerator LoadLobbyWhenConnected() {
        int times = 0;
        login_button.interactable = false;
        ConnectingText.gameObject.SetActive(true);
        //check if connected
        while (!PhotonNetwork.connected && times < 25) {
            times++;
            string dots = "";
            for(int j=0;j<=times%3;j++)//UI effect
                dots+=".";
            ConnectingText.color = Color.yellow;
            ConnectingText.text = "Connecting" + dots;
            yield return new WaitForSeconds(0.3f);
        }
        
        if(times >= 15) {
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
        MySceneManager.LoadScene(MySceneManager.SceneName.Lobby);
        ConnectingText.gameObject.SetActive(false);        
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

    public void ChangeServerToConnect() {
        region = serverToConnect.captionText.text;
    }

    void ConnectToServerSelected() {
        switch (region) {
            case "US":
            PhotonNetwork.ConnectToRegion(CloudRegionCode.us, gameVersion);
            break;
            case "EU":
            PhotonNetwork.ConnectToRegion(CloudRegionCode.eu, gameVersion);
            break;
            case "KR":
            PhotonNetwork.ConnectToRegion(CloudRegionCode.kr, gameVersion);
            break;
            case "SA":
            PhotonNetwork.ConnectToRegion(CloudRegionCode.sa, gameVersion);
            break;
            case "JP":
            PhotonNetwork.ConnectToRegion(CloudRegionCode.jp, gameVersion);
            break;
        }
    }

	public void OnFailedToConnectToPhoton(object parameters)
	{
		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
	}

    public void Exit() {
        Application.Quit();
    }
}
