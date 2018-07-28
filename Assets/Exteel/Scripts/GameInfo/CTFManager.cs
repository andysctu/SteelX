using UnityEngine;
using System.Collections;

public class CTFManager : GameManager {
    private GameObject GameEnvironment;//Blue base, red base , ... 
    private CTFMsgDisplayer CTFMsgDisplayer;
    private CTFPanelManager CTFPanelManager;    
    private Flag RedFlag, BlueFlag;
    private TerritoryController[] territories;    
    private int TerrainLayerMask;
    private int blueScore = 0, redScore = 0;
    private int BlueBaseIndex, RedBaseIndex;
    private bool flag_is_sync = false, game_environment_is_built = false;
    
    public PhotonPlayer BlueFlagHolder = null, RedFlagHolder = null;

    CTFManager() {
        isTeamMode = true;
    }

    protected override void Awake() {
        base.Awake();
        InitComponents();
    }

    private void InitComponents() {
        CTFPanelManager g = Resources.Load<CTFPanelManager>("CTFPanel");
        CTFPanelManager = Instantiate(g, PanelCanvas);
        TransformExtension.SetLocalTransform(CTFPanelManager.transform, Vector3.zero, Quaternion.identity, new Vector3(1,1,1));

        CTFMsgDisplayer = CTFPanelManager.GetComponentInChildren<CTFMsgDisplayer>();
    }

    protected override void Start() {
        base.Start();        

        TerrainLayerMask = LayerMask.GetMask("Terrain");

        if(Offline)return;

        CTFMsgDisplayer.ShowWaitOtherPlayer(true);
    }

    protected override void InitRespawnPoints() {
        string map = GameInfo.Map;
        GameObject g = (GameObject)Resources.Load("GameEnvironment/" + map + "_CTF");
        
        TerritoryController[] territoryControllers = g.GetComponentsInChildren<TerritoryController>();
        RespawnPoints = new Vector3[territoryControllers.Length];
        for(int i=0;i< territoryControllers.Length; i++) {            
            RespawnPoints[territoryControllers[i].Territory_ID] = territoryControllers[i].transform.position;
        }

        InitBaseIndex();
    }

    private void InitBaseIndex() {
        BlueBaseIndex = 0;
        RedBaseIndex = RespawnPoints.Length - 1;
    }

    public override void InstantiatePlayer() {
        Vector3 StartPos;
        Quaternion StartRot;

        if (PhotonNetwork.player.GetTeam() == PunTeams.Team.blue || PhotonNetwork.player.GetTeam() == PunTeams.Team.none) {
            if(PhotonNetwork.player.GetTeam() == PunTeams.Team.none) {
                Debug.LogWarning("This player's team is null");
            }
            SetRespawnPoint(BlueBaseIndex);//set default
            
            StartPos = TransformExtension.RandomXZposition(RespawnPoints[BlueBaseIndex], 20);
            StartRot = Quaternion.Euler(new Vector3(0, Random.Range(0,180), 0));
        } else {
            SetRespawnPoint(RedBaseIndex);
            StartPos = TransformExtension.RandomXZposition(RespawnPoints[RedBaseIndex], 20);
            StartRot = Quaternion.Euler(new Vector3(0, Random.Range(0, 180), 0));
        }

        Mech m = (Offline)? new Mech() : UserData.myData.Mech[0];//Default 0  //TODO : remove offline check

        player = PhotonNetwork.Instantiate(MechPrefab.name, StartPos, StartRot, 0);
        BuildMech mechBuilder = player.GetComponent<BuildMech>();
        mechBuilder.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);

        player_mcbt = player.GetComponent<MechCombat>();        
    }

    public override void OnPlayerDead(GameObject player, int shooter_ViewID, string weapon) {      
        PhotonView player_pv = player.GetComponent<PhotonView>();
        if(player_pv == null) {
            Debug.LogWarning("This player does not have photonview.");
            return;
        }

        // Update scoreboard
        RegisterKill(shooter_ViewID, player_pv.viewID);
        PhotonView shooterpv = PhotonView.Find(shooter_ViewID);
        DisplayKillMsg(shooterpv.owner.NickName, player_pv.owner.NickName, weapon);

        //Master check if this player has the flag
        if (PhotonNetwork.isMasterClient) {
            RaycastHit hit;
            if (BlueFlagHolder != null && player_pv.owner ==  BlueFlagHolder) {
                //Teleport the flag to ground
                Physics.Raycast(player.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
                photonView.RPC("DropFlag", PhotonTargets.All, player_pv.viewID, (int)Team.BLUE, hit.point);
            } else if (RedFlagHolder != null && player_pv.owner == RedFlagHolder) {
                Physics.Raycast(player.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
                photonView.RPC("DropFlag", PhotonTargets.All, player_pv.viewID, (int)Team.RED, hit.point);
            }
        }
    }

    protected override void SyncPanel() {//TODO : check if this sync well
        blueScore = (PhotonNetwork.room.CustomProperties["BlueScore"] == null) ? 0 : int.Parse(PhotonNetwork.room.CustomProperties["BlueScore"].ToString());
        redScore = (PhotonNetwork.room.CustomProperties["RedScore"] == null) ? 0 : int.Parse(PhotonNetwork.room.CustomProperties["RedScore"].ToString());
        CTFPanelManager.UpdateScoreText(blueScore, redScore);
    }

    protected override bool CheckIfGameSync() {
        if (flag_is_sync && game_environment_is_built) {//sync condition
            return true;
        } else {
            if (!game_environment_is_built) {                
                if (FindObjectOfType<TerritoryController>() != null) {
                    game_environment_is_built = true;
                }
            }

            if (!flag_is_sync) {
                Flag[] flags = FindObjectsOfType<Flag>();

                if(flags == null || flags.Length == 0)return false;

                //Assign the flags
                foreach (Flag flag in flags) {
                    if (flag.team == PunTeams.Team.blue) {
                        BlueFlag = flag;
                    } else {
                        RedFlag = flag;
                    }
                }

                photonView.RPC("SyncFlagRequest", PhotonTargets.MasterClient);
            }
            return false;
        }
    }

    protected override void MasterInitGame() {
        base.MasterInitGame();

        //Init scores
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                  { "BlueScore", 0 },
                  { "RedScore", 0 },
        };
        PhotonNetwork.room.SetCustomProperties(h);

        InstantiateFlags();
    }

    protected override void OnMasterFinishInit() {
        base.OnMasterFinishInit();

        InitTerritories();
        FindGameEnvironment();
    }

    protected override void MasterLoadMapSetting() {
        string map = GameInfo.Map;
        GameEnvironment = PhotonNetwork.InstantiateSceneObject("GameEnvironment/"+map + "_CTF", Vector3.zero, Quaternion.identity, 0, null);
    }

    private void FindGameEnvironment() {//client must have a ref to this , since master may disconnet
        PhotonView[] pvs = FindObjectsOfType<PhotonView>();
        foreach(PhotonView pv in pvs) {
            if(pv.name == (GameInfo.Map + "_CTF(Clone)") ) {
                GameEnvironment = pv.gameObject;
                return;
            }
        }

        if(GameEnvironment == null) {
            Debug.LogError("GameEnvironment is null");
        }
    }

    private void InitTerritories() {
        territories = FindObjectsOfType<TerritoryController>();
        MapPanelController[] MapPanelControllers = FindObjectsOfType<MapPanelController>();

        foreach (TerritoryController g in territories) {
            //Register territories to map panels            
            g.FindMapPanels(MapPanelControllers);

            if (PhotonNetwork.isMasterClient) {
                //Master init terrotory IDs
                ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                    { "T_" + g.Territory_ID, (int)g.curTerritoryState }
                };
                PhotonNetwork.room.SetCustomProperties(h);

                if (!g.interactable || !g.gameObject.activeSelf) continue;
                g.ChangeTerritory((int)Team.NONE);
            } else {
                if (!g.interactable || !g.gameObject.activeSelf) continue;

                if (PhotonNetwork.room.CustomProperties["T_" + g.Territory_ID] != null) {
                    g.ChangeTerritory(int.Parse(PhotonNetwork.room.CustomProperties["T_" + g.Territory_ID].ToString()));
                } else {
                    g.ChangeTerritory((int)Team.NONE);
                }
            }
        }
    }

    public override GameObject GetMap() {
        GameObject map = (GameObject)Resources.Load("Map/" + GameInfo.Map + "_CTF_Map");
        if(map == null)Debug.LogError("Can't find : " + GameInfo.Map + "_CTF_Map" + " in Resources/Map/");
        return map;
    }

    protected override void OnGameStart() {
        base.OnGameStart();
        CTFMsgDisplayer.ShowWaitOtherPlayer(false);
    }

    public override void RegisterPlayer(int player_viewID) {
        CTFPanelManager.RegisterPlayer(player_viewID);
    }

    protected override void ShowScorePanel(bool b) {
        CTFPanelManager.ShowPanel(b);
    }

    public override void RegisterKill(int shooter_viewID, int victim_viewID) {
        CTFPanelManager.RegisterKill(shooter_viewID, victim_viewID);        
    }

    void InstantiateFlags() {//Called by master
        GameObject blueflag = PhotonNetwork.InstantiateSceneObject("BlueFlag", new Vector3(RespawnPoints[BlueBaseIndex].x, 0, RespawnPoints[BlueBaseIndex].z), Quaternion.Euler(Vector3.zero), 0, null),
        redFlag = PhotonNetwork.InstantiateSceneObject("RedFlag", new Vector3(RespawnPoints[RedBaseIndex].x, 0, RespawnPoints[RedBaseIndex].z), Quaternion.Euler(Vector3.zero), 0, null);

        BlueFlag = blueflag.GetComponent<Flag>();
        RedFlag = redFlag.GetComponent<Flag>();
    }

    public override void SetRespawnPoint(int num) {
        PunTeams.Team player_team = PhotonNetwork.player.GetTeam();

        if (player_team == PunTeams.Team.none) { Debug.LogWarning("This player is team none"); }//debug use

        if (PhotonNetwork.room.CustomProperties["T_" + num] == null) {//Master has not init this
            respawnPointNum = (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) ? RedBaseIndex : BlueBaseIndex;//Set to the base point
            return;
        }

        if (player_team == PunTeams.Team.red && int.Parse(PhotonNetwork.room.CustomProperties["T_" + num].ToString()) == (int)Team.RED) {
            respawnPointNum = num;            
        } else if( (player_team == PunTeams.Team.blue || player_team == PunTeams.Team.none)  && int.Parse(PhotonNetwork.room.CustomProperties["T_" + num].ToString()) == (int)Team.BLUE)  {
            respawnPointNum = num;
        } else {            
            respawnPointNum = (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) ? RedBaseIndex : BlueBaseIndex;            
        }
    }

    [PunRPC]
    void SyncFlagRequest() {
        //Always received by master

        if (BlueFlagHolder == null) {//set to the current position
            photonView.RPC("SetFlag", PhotonTargets.All, -1, (int)Team.BLUE, BlueFlag.transform.position);//TODO : remake this so that only send to player who request
        } else {//set to the player who is holding this
            photonView.RPC("SetFlag", PhotonTargets.All, (BlueFlag.transform.root).GetComponent<PhotonView>().viewID, (int)Team.BLUE, Vector3.zero);
        }

        if (RedFlagHolder == null) {
            photonView.RPC("SetFlag", PhotonTargets.All, -1, (int)Team.RED, RedFlag.transform.position);
        } else {
            photonView.RPC("SetFlag", PhotonTargets.All, (RedFlag.transform.root).GetComponent<PhotonView>().viewID, (int)Team.RED, Vector3.zero);
        }
    }

    [PunRPC]
    void DropFlag(int player_viewID, int flag, Vector3 pos) {//also call when disable player
        if (flag == 0) {
            SetFlagProperties((int)Team.BLUE, null, pos, null);

            //when disabling player , flag's renderer gets turn off
            Renderer[] renderers = BlueFlag.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = true;
            }
        } else {
            SetFlagProperties((int)Team.RED, null, pos, null);

            Renderer[] renderers = RedFlag.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = true;
            }
        }
    }

    [PunRPC]
    void GetFlagRequest(int player_viewID, int flag) {//Always received by master
        PhotonPlayer player = PhotonView.Find(player_viewID).owner;

        //check the current holder
        if (flag == (int)Team.BLUE) {
            if (BlueFlagHolder != null) {//someone has taken it first
                return;
            } else {//RPC all to give this flag to the player
                photonView.RPC("SetFlag", PhotonTargets.All, player_viewID, flag, Vector3.zero);
                Debug.Log(player + " has the blue flag.");
            }
        } else {
            if (RedFlagHolder != null) {
                return;
            } else {
                photonView.RPC("SetFlag", PhotonTargets.All, player_viewID, flag, Vector3.zero);
                Debug.Log(player + " has the red flag.");
            }
        }
    }

    void SetFlagProperties(int flag, Transform parent, Vector3 pos, PhotonPlayer holder) {
        if (flag == (int)Team.BLUE) {
            if (parent != null) {
                //parent to the player
                BlueFlag.transform.parent = parent;
                TransformExtension.SetLocalTransform(BlueFlag.transform, Vector3.zero, Quaternion.Euler(new Vector3(225, -180, 180)));
                BlueFlagHolder = holder;
                BlueFlag.OnParentToPlayerAction();
            } else {
                //drop the flag to the pos
                BlueFlag.transform.parent = null;
                TransformExtension.SetLocalTransform(BlueFlag.transform, pos, Quaternion.identity);
                BlueFlagHolder = null;
                BlueFlag.OnDroppedAction();
            }
        } else {
            if (parent != null) {
                RedFlag.transform.parent = parent;
                TransformExtension.SetLocalTransform(RedFlag.transform, Vector3.zero, Quaternion.Euler(new Vector3(225, -180, 180)));
                RedFlagHolder = holder;
                RedFlag.OnParentToPlayerAction();
            } else {
                RedFlag.transform.parent = null;
                TransformExtension.SetLocalTransform(RedFlag.transform, pos, Quaternion.identity);
                RedFlagHolder = null;
                RedFlag.OnDroppedAction();
            }
        }
    }

    [PunRPC]
    void SetFlag(int player_viewID, int flag, Vector3 pos) {
        if (BlueFlag == null || RedFlag == null)return;

        flag_is_sync = true;
        if (player_viewID == -1) {//put the flag to the pos 
            if (flag == (int)Team.BLUE) {
                SetFlagProperties((int)Team.BLUE, null, pos, null);

                if (BlueFlag.transform.position.x == RespawnPoints[BlueBaseIndex].x && BlueFlag.transform.position.z == RespawnPoints[BlueBaseIndex].z) {
                    BlueFlag.OnBaseAction();
                }
            } else {
                SetFlagProperties((int)Team.RED, null, pos, null);

                if (RedFlag.transform.position.x == RespawnPoints[RedBaseIndex].x && RedFlag.transform.position.z == RespawnPoints[RedBaseIndex].z) {
                    RedFlag.OnBaseAction();
                }
            }
        } else {
            PhotonView pv = PhotonView.Find(player_viewID);
            if(pv == null) {
                Debug.LogWarning("SetFlag : This player is disconnected.");
                return;
            }

            if (flag == (int)Team.BLUE) {
                SetFlagProperties((int)Team.BLUE, pv.transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/BackPack_Bone"), Vector3.zero, pv.owner);
            } else {
                SetFlagProperties((int)Team.RED, pv.transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/BackPack_Bone"), Vector3.zero, pv.owner);
            }
        }
    }

    [PunRPC]
    void GetScoreRequest(int player_viewID) {
        //Always received by master

        PhotonView pv = PhotonView.Find(player_viewID);
        if (pv == null) {
            Debug.LogWarning("GetScoreRequest : Can't find pv");
            return;
        }

        //check if no one is taking the another flag
        if (pv.owner.GetTeam() == PunTeams.Team.blue || pv.owner.GetTeam() == PunTeams.Team.none) {
            if (BlueFlagHolder != null || RedFlagHolder == null)return;


            photonView.RPC("RegisterScore", PhotonTargets.All, player_viewID);

            //send back the flag
            photonView.RPC("SetFlag", PhotonTargets.All, -1, (int)Team.RED, new Vector3(RespawnPoints[RedBaseIndex].x, 0, RespawnPoints[RedBaseIndex].z));
            
        } else {//Redteam : blue flag holder
            if (BlueFlagHolder == null || RedFlagHolder != null)return;
            
            photonView.RPC("RegisterScore", PhotonTargets.All, player_viewID);

            photonView.RPC("SetFlag", PhotonTargets.All, -1, (int)Team.BLUE, new Vector3(RespawnPoints[BlueBaseIndex].x, 0, RespawnPoints[BlueBaseIndex].z));
        }
    }

    protected override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        base.OnPhotonPlayerDisconnected(player);

        if(!PhotonNetwork.isMasterClient)return;

        //Check if this player has the flag
        if (player.NickName == ((BlueFlagHolder == null) ? "" : BlueFlagHolder.NickName)) {
            //teleport the flag to ground
            RaycastHit hit;
            Physics.Raycast(BlueFlag.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
            photonView.RPC("DropFlag", PhotonTargets.All, -1, (int)Team.BLUE, hit.point);

        } else if (player.NickName == ((RedFlagHolder == null) ? "" : RedFlagHolder.NickName)) {
            RaycastHit hit;
            Physics.Raycast(RedFlag.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
            photonView.RPC("DropFlag", PhotonTargets.All, -1, (int)Team.RED, hit.point);
        }
    }

    protected override void EndGameProcess() {
        base.EndGameProcess();

        //Check the condition and display win & lose


        CTFMsgDisplayer.ShowGameOver();
    }

    protected override IEnumerator PlayFinalGameScene() {
        yield break;
    }

    protected override void OnEndGameRelease() {
        base.OnEndGameRelease();
        if(!PhotonNetwork.isMasterClient)return;

        //Destroy scene objects
        PhotonNetwork.Destroy(GameEnvironment);
        PhotonNetwork.Destroy(BlueFlag.gameObject);
        PhotonNetwork.Destroy(RedFlag.gameObject);
    }

    [PunRPC]
    void RegisterScore(int player_viewID) {
        PhotonPlayer player = PhotonView.Find(player_viewID).owner;

        if (player.GetTeam() == PunTeams.Team.blue || player.GetTeam() == PunTeams.Team.none) {
            blueScore++;
        } else {
            redScore++;
        }

        CTFPanelManager.UpdateScoreText(blueScore, redScore);

        if (PhotonNetwork.isMasterClient) {
            if (player.GetTeam() == PunTeams.Team.blue || player.GetTeam() == PunTeams.Team.none) {
                ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
                h.Add("BlueScore", blueScore);
                PhotonNetwork.room.SetCustomProperties(h);
            } else {
                ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
                h.Add("RedScore", redScore);
                PhotonNetwork.room.SetCustomProperties(h);
            }
        }
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
        CTFMsgDisplayer.ShowWaitOtherPlayer(false);
        isTeamMode = false;
    }
}