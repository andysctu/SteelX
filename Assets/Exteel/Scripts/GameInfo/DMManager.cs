using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class DMManager : GameManager {
    TerritoryController[] territoryControllers;
    private GameObject GameEnvironment;//Blue base, red base , ... 
    private DMMsgDisplayer DMMsgDisplayer;
    private DMPanelManager DMPanelManager;
    private GameObject Map;
    private int MaxKills = 2, CurrentMaxKills = 0;
    private string CurrentLeader = ""; // TODO: change to playerID
    private bool game_environment_is_built = false;

    DMManager() {
        isTeamMode = false;
    }

    protected override void Awake() {
        base.Awake();
        InitComponents();
    }

    protected override void LoadGameInfo() {
        base.LoadGameInfo();

        GameInfo.MaxKills = int.Parse(PhotonNetwork.room.CustomProperties["MaxKills"].ToString());
        MaxKills = GameInfo.MaxKills;
        Debug.Log("Max kills : "+ MaxKills);
    }

    private void InitComponents() {
        DMPanelManager g = Resources.Load<DMPanelManager>("DMPanel");
        DMPanelManager = Instantiate(g, PanelCanvas);
        TransformExtension.SetLocalTransform(DMPanelManager.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

        DMMsgDisplayer = DMPanelManager.GetComponentInChildren<DMMsgDisplayer>();
    }

    protected override void Start() {
        base.Start();

        InitPlayerInZones();

        if (Offline) return;

        DMMsgDisplayer.ShowWaitOtherPlayer(true);
    }

    protected override void InitRespawnPoints() {
        string map = GameInfo.Map;
        GameObject g = (GameObject)Resources.Load("GameEnvironment/" + map + "_DM");

        territoryControllers = g.GetComponentsInChildren<TerritoryController>();
        RespawnPoints = new Vector3[territoryControllers.Length];
        for (int i = 0; i < territoryControllers.Length; i++) {
            RespawnPoints[territoryControllers[i].Territory_ID] = territoryControllers[i].transform.position;
        }
    }

    public override void InstantiatePlayer() {
        Vector3 StartPos;
        Quaternion StartRot;

        //Instantiate at random point
        int randomPoint = Random.Range(0, territoryControllers.Length - 1);
        SetRespawnPoint(randomPoint);

        StartPos = TransformExtension.RandomXZposition(RespawnPoints[randomPoint], 20);
        StartRot = Quaternion.Euler(new Vector3(0, Random.Range(0, 180), 0));

        Mech m = (Offline) ? new Mech() : UserData.myData.Mech[0];//Default 0  //TODO : remove offline check

        player = PhotonNetwork.Instantiate(MechPrefab.name, StartPos, StartRot, 0);
        BuildMech mechBuilder = player.GetComponent<BuildMech>();
        mechBuilder.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);

        player_mcbt = player.GetComponent<MechCombat>();

        FindPlayerMainCameras(player);
    }

    private void FindPlayerMainCameras(GameObject player) {
        Camera[] playerCameras = player.GetComponentsInChildren<Camera>(true);
        List<Camera> mainCameras = new List<Camera>();

        foreach (Camera cam in playerCameras) {
            if (cam.tag == "MainCamera")
                mainCameras.Add(cam);
        }

        thePlayerMainCameras = mainCameras.ToArray();
    }

    public override void OnPlayerDead(PhotonPlayer victim, PhotonPlayer shooter, string weapon) {
        if(PhotonNetwork.isMasterClient)RegisterKill(victim, shooter);

        DisplayKillMsg(shooter.NickName, victim.NickName, weapon);
    }

    protected override void SyncPanel() {//TODO : check if this sync well
        return;
    }

    protected override bool CheckIfGameSync() {
        if (!game_environment_is_built) {//sync condition
            if (FindObjectOfType<TerritoryController>() != null) {
                game_environment_is_built = true;
            }
        }

        return base.CheckIfGameSync() && game_environment_is_built;        
    }

    protected override void MasterInitGame() {
        base.MasterInitGame();

        //Init scores
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                  { "BlueScore", 0 },
                  { "RedScore", 0 },
        };
        PhotonNetwork.room.SetCustomProperties(h);
    }

    protected override void OnMasterFinishInit() {
        base.OnMasterFinishInit();

        InitTerritories();
        FindGameEnvironment();
    }

    protected override void MasterLoadMapSetting() {
        string map = GameInfo.Map;
        GameEnvironment = PhotonNetwork.InstantiateSceneObject("GameEnvironment/" + map + "_DM", Vector3.zero, Quaternion.identity, 0, null);
    }

    private void FindGameEnvironment() {//client must have a ref to this , since master may disconnet
        PhotonView[] pvs = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in pvs) {
            if (pv.name == (GameInfo.Map + "_DM(Clone)")) {
                GameEnvironment = pv.gameObject;
                return;
            }
        }

        if (GameEnvironment == null) {
            Debug.LogError("GameEnvironment is null");
        }
    }

    private void InitTerritories() {
        territoryControllers = FindObjectsOfType<TerritoryController>();
        MapPanelController MapPanelController = GetMap().GetComponent<MapPanelController>();

        foreach (TerritoryController g in territoryControllers) {
            //Register territories to map panels            
            g.AssignMapPanelController(MapPanelController);//TODO : consider remake this
        }
    }

    private void InitPlayerInZones() {
        PlayerInZone[] playerInZones = FindObjectsOfType<PlayerInZone>();
        foreach (PlayerInZone p in playerInZones) {
            p.SetPlayer(player);
        }
    }

    public override GameObject GetMap() {
        if (Map != null) {
            return Map;
        } else {
            //Instantiate one
            GameObject mapPrefab = (GameObject)Resources.Load("Map/" + GameInfo.Map + "_DM_Map");
            if (mapPrefab == null) Debug.LogError("Can't find : " + GameInfo.Map + "_DM_Map" + " in Resources/Map/");
            Map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity, null);

            return Map;
        }
    }

    protected override void OnGameStart() {
        base.OnGameStart();
        DMMsgDisplayer.ShowWaitOtherPlayer(false);
    }

    public override void RegisterPlayer(PhotonPlayer player) {
        DMPanelManager.RegisterPlayer(player);
    }

    protected override void ShowScorePanel(bool b) {
        DMPanelManager.ShowPanel(b);
    }

    public override void RegisterKill(PhotonPlayer victim, PhotonPlayer shooter) {
        int shooter_newKills = DMPanelManager.GetPlayerKillCount(shooter.NickName) + 1;
        int victim_newDeaths = DMPanelManager.GetPlayerDeathCount(victim.NickName) + 1;

        //only master update the player properties
        if (PhotonNetwork.isMasterClient) {
            ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable();
            h2.Add("Kills", shooter_newKills);

            ExitGames.Client.Photon.Hashtable h3 = new ExitGames.Client.Photon.Hashtable();
            h3.Add("Deaths", victim_newDeaths);

            shooter.SetCustomProperties(h2);
            victim.SetCustomProperties(h3);
        }

        //Update max kills
        if (shooter_newKills > CurrentMaxKills) {
            CurrentMaxKills = shooter_newKills;
            CurrentLeader = shooter.NickName;
        }

        photonView.RPC("UpdateScores", PhotonTargets.All, shooter, shooter_newKills, victim, victim_newDeaths);
    }

    [PunRPC]
    private void UpdateScores(PhotonPlayer shooter, int shooter_kills, PhotonPlayer victim, int victim_deaths) {
        if(shooter != null) {
            DMPanelManager.RegisterKill(shooter.NickName, shooter_kills);
        }

        if(victim != null) {
            DMPanelManager.RegisterDeath(victim.NickName, victim_deaths);
        }
    }

    public override void SetRespawnPoint(int num) {
        Debug.Log("Set respawn Point : " + num);
        respawnPointNum = num;
    }

    protected override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        base.OnPhotonPlayerDisconnected(player);
    }

    protected override bool CheckEndGameCondition() {
        return base.CheckEndGameCondition() || (CurrentMaxKills >= MaxKills);
    }

    protected override void EndGameProcess() {
        base.EndGameProcess();

        //Check the condition and display win & lose

        DMMsgDisplayer.ShowGameOver();
    }

    protected override IEnumerator PlayFinalGameScene() {
        yield break;
    }

    protected override IEnumerator SaveGameStats()
    {
        Debug.Log("Saving game stats");

        WWWForm form = new WWWForm();

        // Infer game start time with current time - match duration
        System.DateTime now = System.DateTime.Now;
        System.DateTime start = now.AddMinutes(-GameInfo.MaxTimeInMinutes);
        form.AddField("start_time", start.ToString(dateTimeFormat));
        form.AddField("end_time", now.ToString(dateTimeFormat));

        form.AddField("game_type", GameInfo.GameMode);
        form.AddField("victor", CurrentLeader);

        string playerHistories = JsonConvert.SerializeObject(DMPanelManager.PlayerScores(), Formatting.Indented);
        Debug.Log(playerHistories);
        form.AddField("player_histories", playerHistories);

        WWW www = new WWW(GameStatsURL, form);

        while (!www.isDone)
        {
            yield return null;
        }
        foreach (KeyValuePair<string, string> entry in www.responseHeaders)
        {
            Debug.Log(entry.Key + ": " + entry.Value);
        }
    }

    protected override void OnEndGameRelease() {
        base.OnEndGameRelease();
        if (!PhotonNetwork.isMasterClient) return;

        //Destroy scene objects
        PhotonNetwork.Destroy(GameEnvironment);
    }

    public override Vector3 GetRespawnPointPosition(int num) {
        return RespawnPoints[num];
    }

    protected override void ApplyOfflineSettings() {
        base.ApplyOfflineSettings();
        DMMsgDisplayer.ShowWaitOtherPlayer(false);
        isTeamMode = false;
    }
}