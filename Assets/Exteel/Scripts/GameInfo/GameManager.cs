using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class GameManager : Photon.MonoBehaviour {
    protected Transform PanelCanvas;
    protected RespawnPanel RespawnPanel;
    protected EscPanel EscPanel;

    protected GameObject MechPrefab, PlayerMech;
    protected Camera[] PlayerMainCameras;

    protected Vector3[] RespawnPoints;
    protected int CurRespawnPoint;//The current respawn point chosen , may be invalid

    protected Timer Timer = new Timer();
    protected InGameChat InGameChat;
    public static bool IsTeamMode;

    public enum RoomStatus { Waiting, InBattle};

    //Block input
    private BlockInputSet BlockInputSet = new BlockInputSet();
    public bool BlockInput { get { return _blockInput; } }//Set by BlockInputController method
    private bool _blockInput = false;

    //end & start
    protected bool GameEnding = false, ExitingGame = false;
    public bool GameIsBegin = false;
    public bool calledGameBegin = false;

    private bool IsMasterInitGame = false;

    //check the player number
    private bool canStart = false; // canStart is true when all player finish loading
    private float lastCheckCanStartTime = 0;
    private int NumOfPlayerfinishedloading = 0;

    //sync time
    public float SyncServerTickInterval = 2;
    private bool OnSyncTimeRequest = false, isTimeInit = false;
    private int _waitTimes, _sendTimes;
    private float ServerTickInterval = 0.05f, _preSyncTime;//time between processing player inputs
    private int _curServerTick;//tick : 0 ... 1023 
    private double _curServerTickTime;// TickTime : the PhotonNetwork.time of _curServerTick

    //Display time on lobby
    private float lastUpdateRoomTime = 0;
    private const float updateRoomTimeInterval = 3;

    //debug
    public bool Offline = false;
    public bool endGameImmediately = false;//debug use

    protected Text TickText, TickTimeText;
    public event System.Action OnWorldUpdate;

    protected virtual void Awake() {
        Application.targetFrameRate = UserData.preferredFrameRate;//TODO : player can choose target frame rate

        InitComponents();

        if ((Offline = FindObjectOfType<GameSceneManager>().test)) {LoadOfflineInfo();return; }

        RegisterOnPhotonEvent();

        BuildGameScene();

        LoadGameInfo();

        SyncServerTick();
    }

    private void InitComponents() {
        PanelCanvas = GameObject.Find("PanelCanvas").transform;
        MechPrefab = Resources.Load<GameObject>("MechFrame");
        RespawnPanel = PanelCanvas.GetComponentInChildren<RespawnPanel>(true);
        InGameChat = FindObjectOfType<InGameChat>();
        EscPanel = PanelCanvas.Find("EscPanel").GetComponent<EscPanel>();
    }

    protected virtual void RegisterOnPhotonEvent() {
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
    }

    protected virtual void OnPhotonEvent(byte eventcode, object content, int senderid) {
    }

    protected virtual void BuildGameScene() {
        string mapName = PhotonNetwork.room.CustomProperties["Map"].ToString(); ;
        GameObject map_res = (GameObject)Resources.Load("GameScene/" + mapName);
        if (map_res == null) {
            Debug.LogError("Can't find : " + mapName + " in GameScene/");
            return;
        }
        Instantiate(map_res);
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

        RegisterTimerEvents();

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

    protected abstract void InitRespawnPoints();

    public virtual void RegisterTimerEvent(int time_in_sec, System.Action action) {
        Timer.RegisterTimeEvent(time_in_sec, action);
    }

    protected virtual void RegisterTimerEvents() {        
        RegisterCloseRoomEvent();        
    }

    protected void RegisterCloseRoomEvent() {
        Timer.RegisterTimeEvent(GameInfo.MaxTime * 60 / 2, () => {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.room.IsOpen = false;
            }
        });
    }

    private IEnumerator LateStart() {
        //Send sync request
        if (!IsMasterInitGame && _sendTimes < 15) {//sometime master connects very slow
            _sendTimes++;
            InGameChat.AddLine("Sending sync game request..." + _sendTimes);
            yield return new WaitForSeconds(1f);
            SendSyncInitGameRequest();
            yield return StartCoroutine(LateStart());
        } else {
            if (_sendTimes >= 15 && !IsMasterInitGame) {
                InGameChat.AddLine("Failed to sync game properties. Is master disconnected ? ", Color.red);
                Debug.Log("master not connected");

                //Game not init => exit the game
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

    protected virtual bool CheckIfGameSync() {
        if (!isTimeInit) {
            if (!OnSyncTimeRequest) StartCoroutine(SyncTimeRequest(1f));
        }

        return isTimeInit;
    }

    public abstract void RegisterPlayer(PhotonPlayer player);

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
        isTimeInit = true;
    }

    public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        if (propertiesThatChanged.ContainsKey("startTime") && propertiesThatChanged.ContainsKey("duration")) {
            Timer.SetStoredTime((int)propertiesThatChanged["startTime"], (int)propertiesThatChanged["duration"]);
        }
    }

    protected virtual void Update() {
        if (!BlockInput) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;            
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ShowScorePanel(Input.GetKey(KeyCode.CapsLock));

        if (Input.GetKeyDown(KeyCode.Escape) && !ExitingGame) {
            EscPanel.EnableEscPanel();            
        }

        if (Offline) return;

        //Update tick
        if (PhotonNetwork.time - _curServerTickTime > ServerTickInterval || PhotonNetwork.time - _curServerTickTime < 0){
            if (OnWorldUpdate != null) OnWorldUpdate();


            double timeDiff = PhotonNetwork.time > _curServerTickTime ? PhotonNetwork.time - _curServerTickTime : 4294967.295 - (_curServerTickTime - PhotonNetwork.time);//4294967.295 is max PhotonNetwork.time
            _curServerTick = (_curServerTick + (int) (timeDiff / ServerTickInterval)) % 1024;
            _curServerTickTime += (int)(timeDiff / ServerTickInterval) * ServerTickInterval;
            if (_curServerTickTime > 4294967.295) _curServerTickTime -= 4294967.295;

            if(TickText != null){
                TickText.text = _curServerTick.ToString();
                TickTimeText.text = _curServerTickTime.ToString();
            }
        }

        // Update time
        if (isTimeInit && GameIsBegin) {
            Timer.UpdateTime();
        }

        //Master check end game condition
        if (PhotonNetwork.isMasterClient && !GameEnding) {
            if (CheckEndGameCondition()) {
                GameEnding = true;
                SetBlockInput(BlockInputSet.Elements.GameEnding, true);
                MasterOnGameOverAction();
            }
        }

        //TODO : debug take out
        if (endGameImmediately && !GameEnding) {
            GameEnding = true;
            SetBlockInput(BlockInputSet.Elements.GameEnding, true);
            photonView.RPC("EndGame", PhotonTargets.All);
        }
    }

    public void ExitGame() {
        ExitingGame = true;
        SetBlockInput(BlockInputSet.Elements.ExitingGame, true);
        PhotonNetwork.LeaveRoom();//LeaveRoom() needs some time to process
    }

    protected virtual void MasterOnGameOverAction() {        
        photonView.RPC("EndGame", PhotonTargets.All);
    }

    protected virtual void OnLeftRoom() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneStateController.SetSceneToLoadOnLoaded(LobbyManager._sceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScenes");
    }

    protected abstract void ShowScorePanel(bool b);

    public abstract void OnPlayerDead(PhotonPlayer player, PhotonPlayer shooter, string weapon);

    private void FixedUpdate() {
        if (!GameIsBegin) {
            if (isTimeInit && Timer.CheckIfGameBegin()) {
                SetGameBegin();
                OnGameStart();
                Debug.Log("Set game begin");
            }

            if (PhotonNetwork.isMasterClient && !calledGameBegin) {//todo : improve this
                if (!canStart) {
                    if (_waitTimes <= 5) {// check if all player finish loading every 2 sec
                        if (Time.time - lastCheckCanStartTime >= 2f) {
                            _waitTimes++;
                            lastCheckCanStartTime = Time.time;
                        }
                    } else {//wait too long
                        print("Wait too long , set start diff :" + (Timer.GetCurrentTimeDiff() + 2));
                        photonView.RPC("CallGameBeginDiff", PhotonTargets.AllBuffered, Timer.GetCurrentTimeDiff() + 2);
                        calledGameBegin = true;
                    }
                } else {//all player finish loading
                    if (isTimeInit) {
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
        isTimeInit = Timer.SyncTime();
        yield return new WaitForSeconds(time);
        OnSyncTimeRequest = false;
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
            if(PlayerMech !=null)PhotonNetwork.Destroy(PlayerMech.gameObject);

            //remove buffered rpcs in GameManager
            PhotonNetwork.RemoveRPCs(photonView);
        }
    }

    public abstract void RegisterKill(PhotonPlayer victim, PhotonPlayer shooter);

    protected void DisplayKillMsg(string shooter, string victim, string weapon) {
        DisplayMsgOnGameChat(shooter + " killed " + victim + " by " + weapon);
    }

    public bool IsGameEnding() {
        return GameEnding;
    }

    public void SetBlockInput(BlockInputSet.Elements element, bool b) {
        BlockInputSet.SetElement(element, b);
        _blockInput = BlockInputSet.IsInputBlocked();
        Debug.Log("Set blockInput : "+element.ToString() + " , "+b);
    }

    protected virtual bool CheckEndGameCondition() {//Called by master
        return Timer.CheckIfGameEnd();
    }

    protected virtual void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        InGameChat.AddLine(newPlayer + " is connected.", Color.green);
    }

    protected virtual void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        InGameChat.AddLine(player + " is disconnected.", Color.red);
    }

    public abstract void SetRespawnPoint(int num);

    public int GetRespawnPoint() {
        return CurRespawnPoint;
    }

    public abstract Vector3 GetRespawnPointPosition(int num);

    public void CallRespawn(int mech_num) {
        Debug.Log("Call respawn mech num : " + mech_num + " respoint : " + CurRespawnPoint);
        SetRespawnPoint(CurRespawnPoint);//set again to make sure not changed

        EnableRespawnPanel(false);
        PlayerMech.GetComponent<PhotonView>().RPC("EnablePlayer", PhotonTargets.All, CurRespawnPoint, mech_num);
    }

    public void EnableRespawnPanel(bool b) {
        RespawnPanel.ShowRespawnPanel(b);
    }

    //Return map 
    public abstract GameObject GetMap();

    public virtual GameObject GetThePlayerMech() {
        return PlayerMech;
    }

    public Camera[] GetThePlayerMainCameras() {
        return PlayerMainCameras;
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
        GameIsBegin = true;        
        RespawnPoints = new Vector3[1];
        RespawnPoints[0] = Vector3.zero;
        InstantiatePlayer();
    }

    [PunRPC]
    protected void EndGame() {
        if (endGameImmediately) {
            InGameChat.AddLine("Master forced end game", Color.red);
        }

        GameEnding = true;
        SetBlockInput(BlockInputSet.Elements.GameEnding, true);
        EndGameProcess();
    }

    protected virtual void EndGameProcess() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(ExecuteAfterTime(3));
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
                { "Status", (int)RoomStatus.Waiting }
            };
            PhotonNetwork.room.SetCustomProperties(h);
        }
        SceneStateController.SetSceneToLoadOnLoaded(GameLobbyManager._sceneName);
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
        GameIsBegin = true;
    }

    public void DisplayMsgOnGameChat(string str) {
        InGameChat.AddLine(str);
    }

    protected void SyncServerTick(){
        if(PhotonNetwork.isMasterClient)return;

        StartCoroutine(SyncServerTickCoroutine());
    }

    private IEnumerator SyncServerTickCoroutine(){
        while (true){
            if (Time.time - _preSyncTime > SyncServerTickInterval){
                _preSyncTime = Time.time;
                photonView.RPC("SyncServerTick", PhotonTargets.MasterClient, PhotonNetwork.player);
            }

            yield return new WaitForFixedUpdate();
        }
    } 

    [PunRPC]
    protected void SyncServerTick(PhotonPlayer sender){
        photonView.RPC("ClientUpdateServerTick", sender, _curServerTickTime, _curServerTick, ServerTickInterval);
    }

    [PunRPC]
    protected void ClientUpdateServerTick(double masterServerTickTime, int masterServerTick, float ServerTickInterval, PhotonMessageInfo info) {
        this.ServerTickInterval = ServerTickInterval;
        _curServerTickTime = masterServerTickTime;
        
        //roundtrip time
        double timeDiff = PhotonNetwork.time > info.timestamp ? PhotonNetwork.time - info.timestamp : 4294967.295 - (info.timestamp - PhotonNetwork.time);//4294967.295 is max PhotonNetwork.time
        _curServerTick = (masterServerTick + (int) (timeDiff / ServerTickInterval))%1024;
        _curServerTickTime += (int)(timeDiff / ServerTickInterval) * ServerTickInterval;
        if(_curServerTickTime > 4294967.295)_curServerTickTime -= 4294967.295;
    }

    public double GetServerTickTime(){
        return _curServerTickTime;
    }

    public int GetServerTick(){
        return _curServerTick;
    }
}
