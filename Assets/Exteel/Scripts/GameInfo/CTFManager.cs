using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTFManager : GameManager {
    private GameObject GameEnvironment;//Blue base, red base , ...
    private CTFMsgDisplayer CTFMsgDisplayer;
    private CTFPanelManager CTFPanelManager;
    private Flag RedFlag, BlueFlag;
    private GameObject Map;
    private TerritoryController[] territories;
    private int TerrainLayerMask;
    private int blueScore = 0, redScore = 0;
    private int BlueBaseIndex, RedBaseIndex;
    private bool flag_is_sync = false, game_environment_is_built = false;
    private PunTeams.Team winTeam = PunTeams.Team.none;

    private enum SYNCEVENT { Flag };

    public PhotonPlayer BlueFlagHolder = null, RedFlagHolder = null;

    private CTFManager() {
        isTeamMode = true;
    }

    protected override void Awake() {
        base.Awake();
        InitComponents();
    }

    private void InitComponents() {
        CTFPanelManager g = Resources.Load<CTFPanelManager>("CTFPanel");
        CTFPanelManager = Instantiate(g, PanelCanvas);
        TransformExtension.SetLocalTransform(CTFPanelManager.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

        CTFMsgDisplayer = CTFPanelManager.GetComponentInChildren<CTFMsgDisplayer>();
    }

    protected override void OnEvent(byte eventcode, object content, int senderid) {
        switch (eventcode) {
            case GameEventCode.SYNC:
            SyncEvent(content, senderid);
            break;
            case GameEventCode.MSG:
            break;
        }
    }

    private void SyncEvent(object content, int senderid) {
        ExitGames.Client.Photon.Hashtable contentHashTable = content as ExitGames.Client.Photon.Hashtable;
        if (contentHashTable != null) {
            int Code = (int)contentHashTable["Code"];
            switch (Code) {
                case (int)SYNCEVENT.Flag:
                SetFlag((bool)contentHashTable["Grounded"], (int)contentHashTable["PVID"], (int)contentHashTable["Team"], (Vector3)contentHashTable["Pos"]);
                break;
                default:
                Debug.LogError("This event code does not exist : " + Code);
                break;
            }
        } else {
            Debug.LogWarning("Content can't be casted to Hashtable");
        }
    }

    protected override void Start() {
        base.Start();

        TerrainLayerMask = LayerMask.GetMask("Terrain");
        InitPlayerInZones();

        if (Offline) return;

        CTFMsgDisplayer.ShowWaitOtherPlayer(true);
    }

    protected override void InitRespawnPoints() {
        string map = GameInfo.Map;
        GameObject g = (GameObject)Resources.Load("GameEnvironment/" + map + "_CTF");

        TerritoryController[] territoryControllers = g.GetComponentsInChildren<TerritoryController>();
        RespawnPoints = new Vector3[territoryControllers.Length];
        for (int i = 0; i < territoryControllers.Length; i++) {
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
            if (PhotonNetwork.player.GetTeam() == PunTeams.Team.none && !Offline) {
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

    public override void OnPlayerDead(GameObject player, int shooter_ViewID, string weapon) {
        PhotonView player_pv = player.GetComponent<PhotonView>();
        if (player_pv == null) {
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
            if (BlueFlagHolder != null && player_pv.owner == BlueFlagHolder) {
                //Teleport the flag to ground
                Physics.Raycast(player.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
                photonView.RPC("DropFlag", PhotonTargets.All, player_pv.viewID, (int)PunTeams.Team.blue, hit.point);
            } else if (RedFlagHolder != null && player_pv.owner == RedFlagHolder) {
                Physics.Raycast(player.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
                photonView.RPC("DropFlag", PhotonTargets.All, player_pv.viewID, (int)PunTeams.Team.red, hit.point);
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

                if (flags == null || flags.Length == 0) return false;

                //Assign the flags
                foreach (Flag flag in flags) {
                    if (flag.flag_team == PunTeams.Team.blue) {
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
        GameEnvironment = PhotonNetwork.InstantiateSceneObject("GameEnvironment/" + map + "_CTF", Vector3.zero, Quaternion.identity, 0, null);
    }

    private void FindGameEnvironment() {//client must have a ref to this , since master may disconnet
        PhotonView[] pvs = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in pvs) {
            if (pv.name == (GameInfo.Map + "_CTF(Clone)")) {
                GameEnvironment = pv.gameObject;
                return;
            }
        }

        if (GameEnvironment == null) {
            Debug.LogError("GameEnvironment is null");
        }
    }

    private void InitTerritories() {
        territories = FindObjectsOfType<TerritoryController>();
        MapPanelController MapPanelController = GetMap().GetComponent<MapPanelController>();

        foreach (TerritoryController g in territories) {
            //Register territories to map panels
            g.AssignMapPanelController(MapPanelController);//TODO : consider remake this

            if (PhotonNetwork.isMasterClient) {
                //Master init terrotory IDs
                ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                    { "T_" + g.Territory_ID, (int)g.curTerritoryState }
                };
                PhotonNetwork.room.SetCustomProperties(h);

                if (!g.interactable || !g.gameObject.activeSelf) continue;
                g.ChangeTerritory((int)PunTeams.Team.none);
            } else {
                if (!g.interactable || !g.gameObject.activeSelf) continue;

                if (PhotonNetwork.room.CustomProperties["T_" + g.Territory_ID] != null) {
                    g.ChangeTerritory(int.Parse(PhotonNetwork.room.CustomProperties["T_" + g.Territory_ID].ToString()));
                } else {
                    g.ChangeTerritory((int)PunTeams.Team.none);
                }
            }
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
            GameObject mapPrefab = (GameObject)Resources.Load("Map/" + GameInfo.Map + "_CTF_Map");
            if (mapPrefab == null) Debug.LogError("Can't find : " + GameInfo.Map + "_CTF_Map" + " in Resources/Map/");
            Map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity, null);

            return Map;
        }
    }

    protected override void OnGameStart() {
        base.OnGameStart();        
        CTFMsgDisplayer.OnGameStart();
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

    private void InstantiateFlags() {//Called by master
        GameObject blueflag = PhotonNetwork.InstantiateSceneObject("BlueFlag", new Vector3(RespawnPoints[BlueBaseIndex].x, 0, RespawnPoints[BlueBaseIndex].z), Quaternion.Euler(Vector3.zero), 0, null),
        redFlag = PhotonNetwork.InstantiateSceneObject("RedFlag", new Vector3(RespawnPoints[RedBaseIndex].x, 0, RespawnPoints[RedBaseIndex].z), Quaternion.Euler(Vector3.zero), 0, null);

        BlueFlag = blueflag.GetComponent<Flag>();
        RedFlag = redFlag.GetComponent<Flag>();
    }

    public override void SetRespawnPoint(int num) {
        Debug.Log("Set respawn Point : " + num);
        PunTeams.Team player_team = PhotonNetwork.player.GetTeam();

        if (player_team == PunTeams.Team.none && !Offline) { Debug.LogWarning("This player is team none"); }//debug use

        if (PhotonNetwork.room.CustomProperties["T_" + num] == null) {//Master has not init this
            respawnPointNum = (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) ? RedBaseIndex : BlueBaseIndex;//Set to the base point
            Debug.Log("Call set respawn point :" + num + " but Master did not init this point.  Set to base : " + respawnPointNum);
            return;
        }

        if (player_team == PunTeams.Team.red && int.Parse(PhotonNetwork.room.CustomProperties["T_" + num].ToString()) == (int)PunTeams.Team.red) {
            respawnPointNum = num;
            Debug.Log("Set successfully with respawn point : " + num);
        } else if ((player_team == PunTeams.Team.blue || player_team == PunTeams.Team.none) && int.Parse(PhotonNetwork.room.CustomProperties["T_" + num].ToString()) == (int)PunTeams.Team.blue) {
            respawnPointNum = num;
            Debug.Log("Set successfully with respawn point : " + num);
        } else {
            respawnPointNum = (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) ? RedBaseIndex : BlueBaseIndex;
            Debug.Log("Set failed : " + num);
        }
    }

    [PunRPC]
    private void SyncFlagRequest(PhotonMessageInfo info) {//Always received by master
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        int[] targetActors = { info.sender.ID };
        raiseEventOptions.TargetActors = targetActors;
        int PVID = 0;
        Vector3 Pos = new Vector3(0, 0, 0);

        //Sync blue flag
        PVID = (BlueFlagHolder == null) ? -1 : (BlueFlag.transform.root).GetComponent<PhotonView>().viewID;
        if (BlueFlagHolder == null) {//set to the current position
            Pos = BlueFlag.transform.position;
        } else {
            Pos = Vector3.zero;
        }

        ExitGames.Client.Photon.Hashtable contentHashTable = new ExitGames.Client.Photon.Hashtable {
            { "Code", SYNCEVENT.Flag },
            { "Grounded", (BlueFlagHolder==null)? true : false },
            { "PVID", PVID },
            { "Team", (int)PunTeams.Team.blue},
            { "Pos", Pos}
        };

        PhotonNetwork.RaiseEvent(GameEventCode.SYNC, contentHashTable, true, raiseEventOptions);

        //Sync red flag
        PVID = (RedFlagHolder == null) ? -1 : (RedFlag.transform.root).GetComponent<PhotonView>().viewID;
        if (RedFlagHolder == null) {
            Pos = RedFlag.transform.position;
        } else {
            Pos = Vector3.zero;
        }

        contentHashTable = new ExitGames.Client.Photon.Hashtable {
            { "Code", SYNCEVENT.Flag },
            { "Grounded", (RedFlagHolder==null)? true : false },
            { "PVID", PVID },
            { "Team", (int)PunTeams.Team.red},
            { "Pos", Pos}
        };

        PhotonNetwork.RaiseEvent(GameEventCode.SYNC, contentHashTable, true, raiseEventOptions);
    }

    [PunRPC]
    private void DropFlag(int player_viewID, int flag, Vector3 pos) {
        PhotonView pv = (player_viewID == -1)? null : PhotonView.Find(player_viewID);

        //TODO : Consider remake this
        //Set EN.
        if (pv != null && pv.isMine) {
            MechCombat mcbt = pv.GetComponent<MechCombat>();
            mcbt.SetMaxEN((int)mcbt.MAX_EN * 2);
        }

        if (flag == (int)PunTeams.Team.blue) {            
            SetFlagProperties((int)PunTeams.Team.blue, null, pos, null);

            //when disabling player , flag's renderer gets turn off
            Renderer[] renderers = BlueFlag.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = true;
            }
            BlueFlag.OnDroppedAction();
        } else {            
            SetFlagProperties((int)PunTeams.Team.red, null, pos, null);

            Renderer[] renderers = RedFlag.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = true;
            }
            RedFlag.OnDroppedAction();
        }
        
        //Broadcast msg
        if(pv != null)CTFMsgDisplayer.PlayerDroppedFlag(pv.owner);
    }

    [PunRPC]
    private void GetFlagRequest(int flag, PhotonMessageInfo info) {//Always received by master
        PhotonPlayer player = info.sender;
        PunTeams.Team player_team = player.GetTeam();
        int player_viewID = info.photonView.viewID;

        if (flag == (int)PunTeams.Team.blue) {
            //Check the current holder
            if (BlueFlagHolder != null) {
                return;
            } else {
                if(player_team == PunTeams.Team.red) {//give the flag to the player
                    photonView.RPC("SetFlag", PhotonTargets.All, false, player_viewID, flag, Vector3.zero);
                    Debug.Log(player + " has the blue flag.");
                } else {//return to base
                    photonView.RPC("SetFlag", PhotonTargets.All, true, player_viewID, flag, RespawnPoints[BlueBaseIndex]);
                }
            }
        } else {
            if (RedFlagHolder != null) {
                return;
            } else {
                if (player_team == PunTeams.Team.red) {
                    photonView.RPC("SetFlag", PhotonTargets.All, true, player_viewID, flag, RespawnPoints[RedBaseIndex]);
                } else {
                    photonView.RPC("SetFlag", PhotonTargets.All, false, player_viewID, flag, Vector3.zero);
                    Debug.Log(player + " has the red flag.");
                }                
            }
        }
    }

    private void SetFlagProperties(int flag, Transform parent, Vector3 pos, PhotonPlayer holder) {
        if (flag == (int)PunTeams.Team.blue) {
            if (parent != null) {
                //parent to the player
                BlueFlag.transform.parent = parent;
                TransformExtension.SetLocalTransform(BlueFlag.transform, Vector3.zero, Quaternion.Euler(new Vector3(225, -180, 180)));
                BlueFlagHolder = holder;
            } else {
                //drop the flag to the pos
                BlueFlag.transform.parent = null;
                TransformExtension.SetLocalTransform(BlueFlag.transform, pos, Quaternion.identity);
                BlueFlagHolder = null;
            }
        } else {
            if (parent != null) {
                RedFlag.transform.parent = parent;
                TransformExtension.SetLocalTransform(RedFlag.transform, Vector3.zero, Quaternion.Euler(new Vector3(225, -180, 180)));
                RedFlagHolder = holder;
            } else {
                RedFlag.transform.parent = null;
                TransformExtension.SetLocalTransform(RedFlag.transform, pos, Quaternion.identity);
                RedFlagHolder = null;
            }
        }
    }

    [PunRPC]
    private void SetFlag(bool grounded, int player_viewID, int flag, Vector3 pos) {
        if (BlueFlag == null || RedFlag == null) return;

        PhotonView pv = PhotonView.Find(player_viewID);
        PhotonPlayer pvOwner = (pv == null) ? null : pv.owner;

        flag_is_sync = true;

        if (grounded) {//put the flag to the pos
            if (flag == (int)PunTeams.Team.blue) {
                SetFlagProperties((int)PunTeams.Team.blue, null, pos, null);

                if (pos.x == RespawnPoints[BlueBaseIndex].x && pos.z == RespawnPoints[BlueBaseIndex].z) {
                    BlueFlag.OnBaseAction();

                    if (pv != null && pv.isMine && pvOwner.GetTeam() == PunTeams.Team.red) {//TODO : consider remake this
                        MechCombat mcbt = pv.GetComponent<MechCombat>();
                        mcbt.SetMaxEN((int)mcbt.MAX_EN * 2);
                    }
                }
            } else {
                SetFlagProperties((int)PunTeams.Team.red, null, pos, null);

                if (pos.x == RespawnPoints[RedBaseIndex].x && pos.z == RespawnPoints[RedBaseIndex].z) {
                    RedFlag.OnBaseAction();

                    if (pv != null && pv.isMine && pvOwner.GetTeam() == PunTeams.Team.blue) {
                        MechCombat mcbt = pv.GetComponent<MechCombat>();
                        mcbt.SetMaxEN((int)mcbt.MAX_EN * 2);
                    }
                }
            }

            if(pvOwner != null && (int)pvOwner.GetTeam() == flag) {//Broadcast msg
                CTFMsgDisplayer.PlayerReturnFlag(pvOwner);
            }

        } else {
            if (pv == null) {
                Debug.LogWarning("SetFlag : This player is disconnected.");
                return;
            }

            if (pv.isMine) {//TODO : consider remake this
                MechCombat mcbt = pv.GetComponent<MechCombat>();
                mcbt.SetMaxEN((int)mcbt.MAX_EN / 2);
            }

            if (flag == (int)PunTeams.Team.blue) {//TODO : improve this
                SetFlagProperties((int)PunTeams.Team.blue, pv.transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/BackPack_Bone"), Vector3.zero, pv.owner);
                BlueFlag.OnParentToPlayerAction();
            } else {
                SetFlagProperties((int)PunTeams.Team.red, pv.transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/BackPack_Bone"), Vector3.zero, pv.owner);
                RedFlag.OnParentToPlayerAction();
            }

            //Broadcast msg
            CTFMsgDisplayer.PlayerGetFlag(pvOwner);
        }
    }

    [PunRPC]
    private void GetScoreRequest(PhotonMessageInfo info) {//Always received by master        
        PhotonPlayer sender = info.sender;

        if (sender == null) {
            Debug.LogWarning("GetScoreRequest : sender is null");
            return;
        }

        //check if no one is taking another flag
        if (sender.GetTeam() == PunTeams.Team.blue || sender.GetTeam() == PunTeams.Team.none) {
            if (BlueFlagHolder != null || RedFlagHolder == null) return;

            photonView.RPC("RegisterScore", PhotonTargets.All, sender);

            //Send back the flag
            photonView.RPC("SetFlag", PhotonTargets.All, true, info.photonView.viewID, (int)PunTeams.Team.red, new Vector3(RespawnPoints[RedBaseIndex].x, 0, RespawnPoints[RedBaseIndex].z));
        } else {//Redteam : blue flag holder
            if (BlueFlagHolder == null || RedFlagHolder != null) return;


            photonView.RPC("RegisterScore", PhotonTargets.All, sender);

            photonView.RPC("SetFlag", PhotonTargets.All, true, info.photonView.viewID, (int)PunTeams.Team.blue, new Vector3(RespawnPoints[BlueBaseIndex].x, 0, RespawnPoints[BlueBaseIndex].z));
        }
    }

    [PunRPC]
    private void RegisterScore(PhotonPlayer player) {
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

        //Broadcast msg
        CTFMsgDisplayer.PlayerGetScore(player);
    }

    protected override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        base.OnPhotonPlayerDisconnected(player);

        if (!PhotonNetwork.isMasterClient) return;

        //Check if this player has the flag
        if (player.NickName == ((BlueFlagHolder == null) ? "" : BlueFlagHolder.NickName)) {
            //teleport the flag to ground
            RaycastHit hit;
            Physics.Raycast(BlueFlag.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
            photonView.RPC("DropFlag", PhotonTargets.All, -1, (int)PunTeams.Team.blue, hit.point);
        } else if (player.NickName == ((RedFlagHolder == null) ? "" : RedFlagHolder.NickName)) {
            RaycastHit hit;
            Physics.Raycast(RedFlag.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);
            photonView.RPC("DropFlag", PhotonTargets.All, -1, (int)PunTeams.Team.red, hit.point);
        }
    }

    protected override void MasterOnGameOverAction() {
        PunTeams.Team winTeam = PunTeams.Team.none;

        if(blueScore > redScore) {
            winTeam = PunTeams.Team.blue;
        } else if(redScore > blueScore) {
            winTeam = PunTeams.Team.red;
        } else {
            winTeam = PunTeams.Team.none;
        }

        photonView.RPC("EndGame", PhotonTargets.All, (int)winTeam);
    }

    [PunRPC]
    private void EndGame(int winTeam) {
        Debug.Log("win team : " + winTeam);
        this.winTeam = (PunTeams.Team)winTeam;
        gameEnding = true;

        EndGameProcess();
    }

    protected override void EndGameProcess() {
        base.EndGameProcess();

        CTFMsgDisplayer.ShowGameOver(winTeam);
    }

    protected override IEnumerator PlayFinalGameScene() {
        yield break;
    }

    protected override void OnEndGameRelease() {
        base.OnEndGameRelease();

        if (!PhotonNetwork.isMasterClient) {
            //Destroy scene objects
            PhotonNetwork.Destroy(GameEnvironment);
            PhotonNetwork.Destroy(BlueFlag.gameObject);
            PhotonNetwork.Destroy(RedFlag.gameObject);
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