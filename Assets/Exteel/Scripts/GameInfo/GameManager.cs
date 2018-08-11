using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameManager : Photon.MonoBehaviour {
    protected RespawnPanel RespawnPanel;    
    protected Vector3[] RespawnPoints;
    protected Transform PanelCanvas;
    protected GameObject MechPrefab;
    protected Timer Timer = new Timer();
    protected GameObject player;
    protected MechCombat player_mcbt;
    protected Camera[] thePlayerMainCameras;
    protected int respawnPointNum;//The current respawn point choosed , may be invalid
    private InGameChat InGameChat;
    public static bool isTeamMode;

    public enum Team { BLUE, RED, NONE };
    public enum Status { Waiting, InBattle};

    //end & start
    private bool gameEnding = false, callEndGame = false, ExitingGame = false;
    public static bool gameIsBegin = false;
    public bool calledGameBegin = false;

    private bool IsMasterInitGame = false;

    //check the player number
    private bool canStart = false; // canStart is true when all player finish loading
    private float lastCheckCanStartTime = 0;
    private int NumOfPlayerfinishedloading = 0;

    //sync time
    private bool OnSyncTimeRequest = false, is_Time_init = false;
    private int waitTimes = 0, sendTimes = 0;

    //Display time on lobby
    private float lastUpdateRoomTime = 0;
    private const float updateRoomTimeInterval = 3;

    //debug
    public bool Offline = false;
    public bool endGameImmediately = false;//debug use

    protected GameManager() {
        gameIsBegin = false;
    }

    protected virtual void Awake() {
        Application.targetFrameRate = UserData.preferredFrameRate;//TODO : player can choose target frame rate

        InitComponents();

        if ((Offline = FindObjectOfType<GameSceneManager>().test)) {LoadOfflineInfo();return; }

        RegisterOnPhotonEvent();

        BuildGameScene();

        LoadGameInfo();
    }

    private void InitComponents() {
        PanelCanvas = GameObject.Find("PanelCanvas").transform;
        MechPrefab = Resources.Load<GameObject>("MechFrame");
        RespawnPanel = PanelCanvas.GetComponentInChildren<RespawnPanel>(true);
        InGameChat = FindObjectOfType<InGameChat>();
    }

    protected virtual void RegisterOnPhotonEvent() {
        PhotonNetwork.OnEventCall += this.OnEvent;
    }

    protected virtual void OnEvent(byte eventcode, object content, int senderid) {
    }

    protected virtual void BuildGameScene() {
        string mapName = PhotonNetwork.room.CustomProperties["Map"].ToString(); ;
        GameObject map_res = (GameObject)Resources.Load("GameScene/" + mapName);
        if (map_res == null) {
            Debug.LogError("Can't find : " + mapName + " in GameScene/");
            return;
        }
        GameObject map = Instantiate(map_res);
    }

    protected virtual void LoadGameInfo() {
        GameInfo.Map = PhotonNetwork.room.CustomProperties["Map"].ToString();
        GameInfo.GameMode = PhotonNetwork.room.CustomProperties["GameMode"].ToString();
        GameInfo.MaxTime = int.Parse(PhotonNetwork.room.CustomProperties["MaxTime"].ToString());
        GameInfo.MaxPlayers = PhotonNetwork.room.MaxPlayers;

        Debug.Log("Map : " + GameInfo.Map + " Gamemode :" + GameInfo.GameMode);
    }

    protected virtual void Start() {
        InitRespawnPoints();

        if (Offline) { ApplyOfflineSettings(); return; }

        Timer.Init();

        //Master initializes room's properties ( team score, ... )
        if (PhotonNetwork.isMasterClient) MasterInitGame();

        ClientInitSelf();

        InstantiatePlayer();

        SetPlayerTagObject();

        StartCoroutine(LateStart());
    }

    protected virtual void ClientInitSelf() {
        ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable {
            { "Kills", 0 },
            { "Deaths", 0 },
            { "weaponOffset", 0 }
        };
        PhotonNetwork.player.SetCustomProperties(h2);
    }

    public abstract void InstantiatePlayer();

    protected abstract void SetPlayerTagObject();

    protected abstract void InitRespawnPoints();

    private IEnumerator LateStart() {
        //Send sync request
        if (!IsMasterInitGame && sendTimes < 15) {//sometime master connects very slow
            sendTimes++;
            InGameChat.AddLine("Sending sync game request..." + sendTimes);
            yield return new WaitForSeconds(1f);
            SendSyncInitGameRequest();
            yield return StartCoroutine(LateStart());
        } else {
            if (sendTimes >= 15 && !IsMasterInitGame) {
                InGameChat.AddLine("Failed to sync game properties. Is master disconnected ? ", Color.red);
                Debug.Log("master not connected");

                //Exit the game
                ExitGame();
            } else {
                InGameChat.AddLine("Game is sync.", Color.green);
                photonView.RPC("PlayerFinishedLoading", PhotonTargets.AllBuffered);
            }

            OnMasterFinishInit();
        }
    }

    private void SendSyncInitGameRequest() {
        if (bool.Parse(PhotonNetwork.room.CustomProperties["GameInit"].ToString()) && CheckIfGameSync()) {
            IsMasterInitGame = true;
        }
    }

    protected virtual void OnMasterFinishInit() {
        SyncPanel();
    }

    protected virtual void SyncPanel() {
    }

    public abstract void OnPlayerDead(GameObject player, int shooter_id, string weapon);

    protected abstract bool CheckIfGameSync();


    public abstract void RegisterPlayer(int player_viewID);

    protected virtual void MasterInitGame() {
        SyncTime();
        MasterLoadMapSetting();
        IsMasterInitGame = true;
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                  { "GameInit", true }//this is set to false when master pressing "start" in game lobby
        };
        PhotonNetwork.room.SetCustomProperties(h);
    }

    protected abstract void MasterLoadMapSetting();

    private void SyncTime() {
        Timer.MasterSyncTime();
        is_Time_init = true;
    }

    public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        if (propertiesThatChanged.ContainsKey("startTime") && propertiesThatChanged.ContainsKey("duration")) {
            Timer.SetStoredTime((int)propertiesThatChanged["startTime"], (int)propertiesThatChanged["duration"]);
        }
    }

    protected virtual void Update() {
        if (!gameEnding && !ExitingGame) {
            if (!player_mcbt.isDead) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            ShowScorePanel(Input.GetKey(KeyCode.CapsLock));
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Offline) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !ExitingGame) {
            ExitGame();  
            return;
        }

        // Update time
        if (!is_Time_init) {
            //Send sync time request
            if (!OnSyncTimeRequest) StartCoroutine(SyncTimeRequest(1f));
        } else if (gameIsBegin) {
            Timer.UpdateTime();
        }

        if (PhotonNetwork.isMasterClient && !gameEnding) {
            if (CheckIfGameOver()) {
                gameEnding = true;
                photonView.RPC("EndGame", PhotonTargets.All);
            }
        }

        //TODO : debug take out
        if (endGameImmediately && !gameEnding) {
            gameEnding = true;
            photonView.RPC("EndGame", PhotonTargets.All);
        }
    }

    private void ExitGame() {
        ExitingGame = true;

        //Player remove his tag object
        PhotonNetwork.player.TagObject = null;

        PhotonNetwork.LeaveRoom();//LeaveRoom() needs some time to process
    }

    protected virtual void OnLeftRoom() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneStateController.SetSceneToLoadOnLoaded(LobbyManager._sceneName);
        SceneManager.LoadScene("MainScenes");
    }

    protected abstract void ShowScorePanel(bool b);

    private void FixedUpdate() {
        if (!gameIsBegin) {
            if (is_Time_init && Timer.CheckIfGameBegin()) {
                SetGameBegin();
                OnGameStart();
                Debug.Log("Set game begin");
            }

            if (PhotonNetwork.isMasterClient && !calledGameBegin) {
                if (!canStart) {
                    if (waitTimes <= 5) {// check if all player finish loading every 2 sec
                        if (Time.time - lastCheckCanStartTime >= 2f) {
                            waitTimes++;
                            lastCheckCanStartTime = Time.time;
                        }
                    } else {//wait too long
                        print("Wait too long , set start diff :" + (Timer.GetCurrentTimeDiff() + 2));
                        photonView.RPC("CallGameBeginDiff", PhotonTargets.AllBuffered, Timer.GetCurrentTimeDiff() + 2);
                        calledGameBegin = true;
                    }
                } else {//all player finish loading
                    if (is_Time_init) {
                        print("start time diff :" + (Timer.GetCurrentTimeDiff() + 2));
                        photonView.RPC("CallGameBeginDiff", PhotonTargets.AllBuffered, Timer.GetCurrentTimeDiff() + 2);
                        calledGameBegin = true;
                    }
                }
            }
        }

        if (PhotonNetwork.isMasterClient) {
            //Update display time in lobby
            if(Time.time - lastUpdateRoomTime > updateRoomTimeInterval) {
                lastUpdateRoomTime = Time.time;
                ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                  { "time", Timer.GetCurrentFormatTime(false) }
                };

                PhotonNetwork.room.SetCustomProperties(h);
            }
        }

    }

    private IEnumerator SyncTimeRequest(float time) {
        OnSyncTimeRequest = true;
        is_Time_init = Timer.SyncTime();
        yield return new WaitForSeconds(time);
        OnSyncTimeRequest = false;
    }

    protected virtual IEnumerator ExecuteAfterTime(float time) {
        yield return new WaitForSeconds(time);

        ShowScorePanel(true);

        //Play final game scene
        yield return StartCoroutine(PlayFinalGameScene());

        //Destroy scene objects
        OnEndGameRelease();        

        //return to game lobby
        if (PhotonNetwork.isMasterClient) {
            PhotonNetwork.LoadLevel("MainScenes");
            PhotonNetwork.room.IsOpen = true;

            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                { "Status", (int)Status.Waiting }
            };
            PhotonNetwork.room.SetCustomProperties(h);
        }
        SceneStateController.SetSceneToLoadOnLoaded(GameLobbyManager._sceneName);
    }

    protected abstract IEnumerator PlayFinalGameScene();

    protected virtual void OnGameStart() {
    }

    protected virtual void OnEndGameRelease() {
        //Master remove his buffered rpc calls
        if (PhotonNetwork.isMasterClient) {
            PhotonNetwork.RemoveRPCs(PhotonNetwork.player);

            //remove all build mech buffered rpcs
            BuildMech[] bms = FindObjectsOfType<BuildMech>();
            foreach(BuildMech bm in bms) {
                PhotonNetwork.RemoveRPCs(bm.photonView);
            }

            //Master destroy his mech
            if(player_mcbt.gameObject !=null)
                PhotonNetwork.Destroy(player_mcbt.gameObject);

            //remove buffered rpcs in GameManager
            PhotonNetwork.RemoveRPCs(photonView);
        }

        //Player remove his tag object
        PhotonNetwork.player.TagObject = null;
    }

    public abstract void RegisterKill(int shooter_viewID, int victim_viewID);

    protected void DisplayKillMsg(string shooter, string target, string weapon) {
        DisplayMsgOnGameChat(shooter + " killed " + photonView.name + " by " + weapon);
    }

    public bool CheckIfGameOver() {//Called by master
        return CheckEndGameCondition();        
    }

    protected virtual bool CheckEndGameCondition() {
        return Timer.CheckIfGameEnd();
    }

    private void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        InGameChat.AddLine(newPlayer + " is connected.", Color.green);
    }

    protected virtual void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        InGameChat.AddLine(player + " is disconnected.", Color.red);
    }

    public abstract void SetRespawnPoint(int num);

    public int GetRespawnPoint() {
        return respawnPointNum;
    }

    public abstract Vector3 GetRespawnPointPosition(int num);

    public void CallRespawn(int mech_num) {
        Debug.Log("Call respawn mech num : " + mech_num + " respoint : " + respawnPointNum);
        SetRespawnPoint(respawnPointNum);//set again to make sure not changed

        EnableRespawnPanel(false);
        player_mcbt.GetComponent<PhotonView>().RPC("EnablePlayer", PhotonTargets.All, respawnPointNum, mech_num);
    }

    public void EnableRespawnPanel(bool b) {
        RespawnPanel.ShowRespawnPanel(b);
    }

    //Return map 
    public abstract GameObject GetMap();

    public Camera[] GetThePlayerMainCameras() {
        return thePlayerMainCameras;
    }

    private void LoadOfflineInfo() {
        GameInfo.MaxTime = 1;
        GameInfo.Map = "Offline";
    }

    protected virtual void ApplyOfflineSettings() {
        Debug.Log("Offline mode is on");
        InitComponents();
        //PhotonNetwork.ConnectToRegion(CloudRegionCode.jp, "1.0");
        PhotonNetwork.player.NickName = "Player1";
        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom("offline");             
        gameIsBegin = true;        
        RespawnPoints = new Vector3[1];
        RespawnPoints[0] = Vector3.zero;
        InstantiatePlayer();
    }

    [PunRPC]
    protected void EndGame() {
        if (endGameImmediately) {
            InGameChat.AddLine("Master forced end game", Color.red);
        }

        gameEnding = true;
        EndGameProcess();
    }

    protected virtual void EndGameProcess() {
        gameEnding = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(ExecuteAfterTime(3));
    }

    [PunRPC]
    protected void PlayerFinishedLoading() {
        NumOfPlayerfinishedloading++;//in case master switches
        if (!PhotonNetwork.isMasterClient) {
            return;
        }

        if (NumOfPlayerfinishedloading >= PhotonNetwork.room.PlayerCount) {
            canStart = true;
            print("All players has connected. Set canStart to true.");
        }
    }

    [PunRPC]
    protected void CallGameBeginDiff(int diff) {//diff : (serverTimeStamp - storedtime) / 1000
        Timer.SetGameBeginTimeDiff(diff);
    }

    private void SetGameBegin() {
        gameIsBegin = true;
    }

    public void DisplayMsgOnGameChat(string str) {
        InGameChat.AddLine(str);
    }
}