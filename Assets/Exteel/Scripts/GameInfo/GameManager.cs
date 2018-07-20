using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public abstract class GameManager : Photon.MonoBehaviour {    
    private RespawnPanel RespawnPanel;    
    private GameMsgDisplayer GameMsgDisplayer;    
    private InGameChat InGameChat;
    protected Transform PanelCanvas;
    protected GameObject PlayerPrefab;
    protected Timer Timer = new Timer();
    protected GameObject player;
    protected MechCombat player_mcbt;

    public enum Team { BLUE, RED, NONE };

    //TODO : remove this
    public int MaxKills = 2, CurrentMaxKills = 0;

	private bool gameEnding = false;

	private bool IsMasterInitGame = false, canStart = false; // this is true when all player finish loading
	private float lastCheckCanStartTime = 0;
    private bool OnSyncTimeRequest = false, is_Time_init = false;
    private int waitTimes = 0, sendTimes = 0, playerfinishedloading = 0;
    private bool callGameBegin = false;

    public bool GameIsBegin = false;
	protected int respawnPoint;
	
	public bool callEndGame = false, Offline = false;

    public static bool isTeamMode;

    private bool endGameImmediately = false;//debug use

    //TODO : player can choose target frame rate
    protected virtual void Awake(){
		Application.targetFrameRate = 60;//60:temp
        InitComponents();
    }

	protected virtual void Start() {           
        Timer.Init();

        if (Offline) { ApplyOfflineSettings();return;}

        LoadGameInfo();//TODO : remake this

        MaxKills = GameInfo.MaxKills;//TODO : check this	

		//Master initializes room's properties ( team score, ... )
		if (PhotonNetwork.isMasterClient)MasterInitGame();

        // client ini himself
        ClientInitSelf();

        GameMsgDisplayer.ShowWaitOtherPlayer (true);

        StartCoroutine(LateStart());
    }

    private void InitComponents() {
        PanelCanvas = GameObject.Find("PanelCanvas").transform;
        PlayerPrefab = Resources.Load<GameObject>("MechFrame");
        RespawnPanel = PanelCanvas.GetComponentInChildren<RespawnPanel>(true);
        GameMsgDisplayer = GetComponentInChildren<GameMsgDisplayer>();
        InGameChat = FindObjectOfType<InGameChat>();
    }

    private void LoadGameInfo() {
        GameInfo.Map = PhotonNetwork.room.CustomProperties["Map"].ToString();
        GameInfo.GameMode = PhotonNetwork.room.CustomProperties["GameMode"].ToString();
        GameInfo.MaxKills = int.Parse(PhotonNetwork.room.CustomProperties["MaxKills"].ToString());
        GameInfo.MaxTime = int.Parse(PhotonNetwork.room.CustomProperties["MaxTime"].ToString());
        GameInfo.MaxPlayers = PhotonNetwork.room.MaxPlayers;
        Debug.Log("Map : "+ GameInfo.Map + "Gamemode :" + GameInfo.GameMode + " MaxKills :" + GameInfo.MaxKills);
    }
		
	IEnumerator LateStart(){
		if(!IsMasterInitGame && sendTimes <10){//sometime master connects very slow
				sendTimes++;
				InGameChat.AddLine ("Sending sync game request..." + sendTimes);
				yield return new WaitForSeconds (1f);
				SendSyncInitGameRequest ();
				yield return StartCoroutine (LateStart ());
		}else{
			if(sendTimes >= 10){
				InGameChat.AddLine ("Failed to sync game properties. Is master disconnected ? ");
				Debug.Log ("master not connected");
			}else{
				InGameChat.AddLine ("Game is sync.");
				photonView.RPC ("PlayerFinishedLoading", PhotonTargets.AllBuffered);
			}

            SyncPanel();
		}

	}

    protected virtual void ClientInitSelf() {
        ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable {
            { "Kills", 0 },
            { "Deaths", 0 },
            { "weaponOffset", 0 }
        };
    }

    protected virtual void SyncPanel() { }
    public abstract void OnPlayerDead(GameObject player, int shooter_id, string weapon);

    private void SendSyncInitGameRequest(){
		if(bool.Parse(PhotonNetwork.room.CustomProperties["GameInit"].ToString())){
            if(CheckIfGameSync())IsMasterInitGame = true;
		}
	}

    protected abstract bool CheckIfGameSync();
    
    public abstract void InstantiatePlayer();

    public abstract void RegisterPlayer(int player_viewID);

	protected virtual void MasterInitGame(){
        SyncTime();
        LoadMapSetting();
        IsMasterInitGame = true;

        //      ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
        //          { "BlueScore", 0 },
        //          { "RedScore", 0 },
        //          { "GameInit", true }//this is set to false when master pressing "start"
        //      };
        //      if (isTeamMode){
        //	h.Add ("Zone", -1);
        //}
        //PhotonNetwork.room.SetCustomProperties (h);
    }

    protected abstract void LoadMapSetting();

	void SyncTime() {
        Timer.MasterSyncTime();
        is_Time_init = true;
    }

	public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
		if (propertiesThatChanged.ContainsKey("startTime") && propertiesThatChanged.ContainsKey("duration")) {
            Timer.SetStoredTime((int)propertiesThatChanged["startTime"], (int)propertiesThatChanged["duration"]);
		}
	}

	void Update() {
		if (!GameOver () && !gameEnding) {
			if (!player_mcbt.isDead) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}else{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
            ShowScorePanel(Input.GetKey(KeyCode.CapsLock));
		}

        if (Offline) return;//TODO : comment this

        if (Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			PhotonNetwork.LeaveRoom();
            SceneStateController.SetSceneToLoadOnLoaded(LobbyManager._sceneName);
			SceneManager.LoadScene("MainScenes");            
		}

        // Update time
        if (is_Time_init) {
            Timer.UpdateTime();
        } else {
            //Send sync time request every second
            if (!OnSyncTimeRequest)StartCoroutine(SyncTimeRequest(1f));
        }

		if (GameOver () && !gameEnding) {
			if (PhotonNetwork.isMasterClient) {
				photonView.RPC ("EndGame", PhotonTargets.All);
			}
		}
		
        //TODO : debug take out
        if (endGameImmediately) {
            Timer.EndGameImmediately();
        }
	}

    protected abstract void ShowScorePanel(bool b);

	void FixedUpdate(){
        if (!GameIsBegin){
			if(Timer.CheckIfGameBegin()){
				SetGameBegin ();
                GameMsgDisplayer.ShowWaitOtherPlayer (false);
				Debug.Log ("Set game begin");
			}

			if(PhotonNetwork.isMasterClient && !callGameBegin){
				if (!canStart) {
					if (waitTimes <= 5) {// check if all player finish loading every 2 sec
						if (Time.time - lastCheckCanStartTime >= 2f) {
							waitTimes++;
							lastCheckCanStartTime = Time.time;
						}
					} else {//wait too long
						print ("start time :" + (Timer.GetCurrentTime() - 2)); 
						photonView.RPC ("CallGameBeginAtTime", PhotonTargets.AllBuffered, Timer.GetCurrentTime() - 2);
						callGameBegin = true;
					}
				}
				else{//all player finish loading
					if (is_Time_init) {
						print ("start time :" + (Timer.GetCurrentTime() - 2)); 
						photonView.RPC ("CallGameBeginAtTime", PhotonTargets.AllBuffered, Timer.GetCurrentTime() - 2);
						callGameBegin = true;
					}
				}
			}
		}	
	}

	IEnumerator SyncTimeRequest(float time){
		OnSyncTimeRequest = true;
        is_Time_init = Timer.SyncTime();
		yield return new WaitForSeconds (time);
		OnSyncTimeRequest = false;
	}

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

        OnGameEndRelease();

        if (PhotonNetwork.isMasterClient){
            PhotonNetwork.LoadLevel("MainScenes");
            PhotonNetwork.room.IsOpen = true;

            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                { "Status", 0 }
            };
            PhotonNetwork.room.SetCustomProperties(h);
        }
        SceneStateController.SetSceneToLoadOnLoaded(GameLobbyManager._sceneName);

        Cursor.visible = true;
	}

    protected virtual void OnGameEndRelease() {}

    public abstract void RegisterKill(int shooter_viewID, int victim_viewID);

    protected void DisplayKillMsg(string shooter, string target, string weapon) {
        DisplayMsgOnRoomChat(shooter + " killed " + photonView.name + " by " + weapon);
    }

    public bool GameOver() {
        return CheckIfGameEnd();
	}

    protected abstract bool CheckIfGameEnd();

	void OnPhotonPlayerConnected(PhotonPlayer newPlayer){
		InGameChat.AddLine (newPlayer + " is connected.");
	}

	protected virtual void OnPhotonPlayerDisconnected(PhotonPlayer player){//TODO : test this
		InGameChat.AddLine (player + " is disconnected.");
        
    }

	public abstract void SetRespawnPoint(int num);

	public int GetRespawnPoint(){
		return respawnPoint;
	}

    public abstract Vector3 GetRespawnPointPosition(int num) ;

	public void CallRespawn(int mech_num){
		SetRespawnPoint (respawnPoint);//set again to make sure not changed

		CloseRespawnPanel();
		player_mcbt.GetComponent<PhotonView> ().RPC ("EnablePlayer", PhotonTargets.All, respawnPoint, mech_num);
	}

	public void ShowRespawnPanel(){
        RespawnPanel.ShowRespawnPanel(true);
    }

	public void CloseRespawnPanel(){
        RespawnPanel.ShowRespawnPanel(false);
    }

    private void ApplyOfflineSettings() {
        Debug.Log("Offline mode is on");
        PhotonNetwork.ConnectToRegion(CloudRegionCode.jp, "1.0");
        PhotonNetwork.player.NickName = "Player1";
        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom("offline");
        GameInfo.MaxKills = 100;
        GameInfo.MaxTime = 1;
        InstantiatePlayer();
        GameIsBegin = true;
        GameMsgDisplayer.ShowWaitOtherPlayer(false);

        isTeamMode = false;
    }

	[PunRPC]
    protected void EndGame(){
		gameEnding = true;
		Cursor.lockState = CursorLockMode.None;
        GameMsgDisplayer.ShowGameOver();
		ShowScorePanel(true);
		StartCoroutine(ExecuteAfterTime(3));
	}

	[PunRPC]
	protected void PlayerFinishedLoading(){
		playerfinishedloading++;//in case master switches
		if(!PhotonNetwork.isMasterClient){
			return;
		}

		if(playerfinishedloading >= PhotonNetwork.room.PlayerCount){
			canStart = true;
			print ("can start now.");
		}
	}

	[PunRPC]
    protected void CallGameBeginAtTime(int time){
        Timer.SetGameBeginTime(time);
	}

	void SetGameBegin(){
		GameIsBegin = true;
	}

    public void DisplayMsgOnRoomChat(string str) {
        InGameChat.AddLine(str);
    }

	protected Vector3 RandomXZposition(Vector3 pos, float radius){
		float x = Random.Range (pos.x - radius, pos.x + radius);
		float z = Random.Range (pos.z - radius, pos.z + radius);
		return new Vector3 (x, pos.y, z);
	}
}
