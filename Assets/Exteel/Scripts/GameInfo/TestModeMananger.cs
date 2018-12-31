using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestModeManager : GameManager {
    private GameObject GameEnvironment;//Blue base, red base , ...
    private Flag RedFlag, BlueFlag;
    private int TerrainLayerMask;
    private int blueScore = 0, redScore = 0;
    private int BlueBaseIndex, RedBaseIndex;
    private bool flag_is_sync = false, game_environment_is_built = false;

    private string playerName;
    private bool isCreated = false;

    private TestModeManager() {
        IsTeamMode = false;
        GameIsBegin = true;
    }

    protected override void Awake(){
        Debug.Log("Start Test Mode");
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.sendRate = 60;
        PhotonNetwork.sendRateOnSerialize = 30;

        RegisterOnPhotonEvent();

        PanelCanvas = GameObject.Find("PanelCanvas").transform;
        MechPrefab = Resources.Load<GameObject>("MechFrame");
        RespawnPanel = PanelCanvas.GetComponentInChildren<RespawnPanel>(true);
        InGameChat = FindObjectOfType<InGameChat>();
        EscPanel = PanelCanvas.Find("EscPanel").GetComponent<EscPanel>();
    }

    protected override void Start(){
        TerrainLayerMask = LayerMask.GetMask("Terrain");
        InitRespawnPoints();
        playerName = "Player" + Random.Range(0, 999).ToString();
        PhotonNetwork.playerName = playerName;
        //PhotonNetwork.ConnectToMaster("192.168.0.13", 5055, "", "1.0");
        PhotonNetwork.ConnectToRegion(CloudRegionCode.us, "1.0");
    }

    private void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
    }

    private void OnJoinedLobby() {
        Debug.Log("Joined lobby");

        //RoomInfo[] roomInfos = PhotonNetwork.GetRoomList();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;
        roomOptions.IsOpen = true;
        PhotonNetwork.JoinOrCreateRoom("Test", roomOptions, TypedLobby.Default);

    }

    private void OnJoinedRoom() {
        if (isCreated) return;

        Debug.Log("Joined room " + PhotonNetwork.room.Name);

        Debug.Log("Connected to server");
        InstantiatePlayer();

        isCreated = true;
    }

    protected override void InitRespawnPoints() {
        string map = GameInfo.Map;
        GameObject g = (GameObject)Resources.Load("GameEnvironment/" + map + "_CTF");

        RespawnPoints = new Vector3[5];
        for (int i = 0; i < 5; i++){
            RespawnPoints[i] = new Vector3(0,5,0);
        }

        InitBaseIndex();
    }

    private void InitBaseIndex() {
        BlueBaseIndex = 0;
        RedBaseIndex = RespawnPoints.Length - 1;
    }

    public override void InstantiatePlayer() {
        //tell master to build it
        Mech m = new Mech();
        photonView.RPC("MasterInstantiatePlayer",PhotonTargets.MasterClient, PhotonNetwork.player, m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
    }

    [PunRPC]
    private void MasterInstantiatePlayer(PhotonPlayer owner, string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r, int[] skillIDs) {
        Vector3 StartPos;
        Quaternion StartRot;

        if (owner.GetTeam() == PunTeams.Team.blue || owner.GetTeam() == PunTeams.Team.none) {
            if (owner.GetTeam() == PunTeams.Team.none && !Offline) {
                Debug.LogWarning("This player's team is none");
            }
            SetRespawnPoint(BlueBaseIndex);//set default

            StartPos = TransformExtension.RandomXZposition(RespawnPoints[BlueBaseIndex], 20);
            StartRot = Quaternion.Euler(new Vector3(0, Random.Range(0, 180), 0));
        } else {
            SetRespawnPoint(RedBaseIndex);
            StartPos = TransformExtension.RandomXZposition(RespawnPoints[RedBaseIndex], 20);
            StartRot = Quaternion.Euler(new Vector3(0, Random.Range(0, 180), 0));
        }

        Vector3 tmp = new Vector3(StartPos.x, 0.2f, StartPos.z);

        PlayerMech = PhotonNetwork.Instantiate(MechPrefab.name, tmp, StartRot, 0);
        BuildMech mechBuilder = PlayerMech.GetComponent<BuildMech>();
        mechBuilder.Build(owner, c, a,l,h,b,w1l,w1r,w2l,w2r, skillIDs);

        FindPlayerMainCameras(PlayerMech);
    }

    private void FindPlayerMainCameras(GameObject player) {//TODO : improve this
        Camera[] playerCameras = player.GetComponentsInChildren<Camera>(true);
        List<Camera> mainCameras = new List<Camera>();

        foreach (Camera cam in playerCameras) {
            if (cam.tag == "MainCamera")
                mainCameras.Add(cam);
        }

        PlayerMainCameras = mainCameras.ToArray();
    }

    public override void OnPlayerDead(PhotonPlayer victim, PhotonPlayer shooter, string weapon) {
        GameObject playerMech = (GameObject)victim.TagObject;
        if (playerMech == null) { Debug.LogWarning("Player mech is null"); return; }
    }

    protected override bool CheckIfGameSync() {
        return true;
    }

    public override void SetRespawnPoint(int num) {
         CurRespawnPoint = num;
    }

    public override Vector3 GetRespawnPointPosition(int num) {
        return RespawnPoints[num];
    }

    public Vector3 GetBlueBasePosition() {
        return RespawnPoints[BlueBaseIndex];
    }

    public Vector3 GetRedBasePosition() {
        return RespawnPoints[RedBaseIndex];
    }

    protected override void ApplyOfflineSettings() {
        base.ApplyOfflineSettings();
        IsTeamMode = false;
    }

    public override void RegisterPlayer(PhotonPlayer player) {
    }

    protected override void MasterLoadMapSetting() {
    }

    protected override void ShowScorePanel(bool b) {
    }

    protected override IEnumerator PlayFinalGameScene() {
        yield break;
    }

    public override void RegisterKill(PhotonPlayer victim, PhotonPlayer shooter) {
    }

    private GameObject Map;

    public override GameObject GetMap() {
        GameObject mapPrefab = (GameObject)Resources.Load("Map/" + "Offline_CTF_Map");
        if (mapPrefab == null) Debug.LogError("Can't find : " + "Offline_CTF_Map" + " in Resources/Map/");
        Map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity, null);

        return Map;
    }
}