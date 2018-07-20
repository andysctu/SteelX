using UnityEngine;
using System.Collections.Generic;

public class CTFManager : GameManager { 
    private CTFPanelManager CTFPanelManager;    
    private GameObject RedFlag, BlueFlag;
    private TerritoryController[] territories;
    private Transform PanelCanvas;
    private int TerrainLayerMask;
    private int blueScore = 0, redScore = 0;    
    private bool flag_is_sync = false;

    public PhotonPlayer BlueFlagHolder = null, RedFlagHolder = null;

    CTFManager() {
        isTeamMode = true;
    }

    protected override void Awake() {
        base.Awake();
        LoadCTFPanelManager();
    }

    private void LoadCTFPanelManager() {
        PanelCanvas = GameObject.Find("PanelCanvas").transform;
        CTFPanelManager g = Resources.Load<CTFPanelManager>("CTFPanel");
        CTFPanelManager = Instantiate(g, PanelCanvas);
        CTFPanelManager.transform.localPosition = Vector3.zero;
        CTFPanelManager.transform.localScale = new Vector3(1,1,1);
    }

    protected override void Start() {
        base.Start();        

        InitTerritories();
        InstantiatePlayer();

        SetHealthPoolLookAtPlayer();
        SetTerritoriesLookAtPlayer();
        TerrainLayerMask = LayerMask.GetMask("Terrain");
    }

    public override void InstantiatePlayer() {
        Vector3 StartPos;
        Quaternion StartRot;

        if (PhotonNetwork.player.GetTeam() == PunTeams.Team.blue || PhotonNetwork.player.GetTeam() == PunTeams.Team.none) {
            if(PhotonNetwork.player.GetTeam() == PunTeams.Team.none) {
                Debug.Log("this player's team is null");
            }
            SetRespawnPoint((int)Team.BLUE);//set default
            
            StartPos = RandomXZposition(territories[0].transform.position + new Vector3(0, 5, 0), 20);
            StartRot = Quaternion.Euler(new Vector3(0, Random.Range(0,180), 0));
        } else {
            SetRespawnPoint((int)Team.RED);
            StartPos = RandomXZposition(territories[territories.Length-1].transform.position + new Vector3(0, 5, 0), 20);
            StartRot = Quaternion.Euler(new Vector3(0, Random.Range(0, 180), 0));
        }

        Mech m;
        if (Offline) {
            m = new Mech();
        } else {
            m = UserData.myData.Mech[0];//default 0
        }

        player = PhotonNetwork.Instantiate(PlayerPrefab.name, StartPos, StartRot, 0);
        BuildMech mechBuilder = player.GetComponent<BuildMech>();
        mechBuilder.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);

        if (player.GetComponent<PhotonView>().isMine) {
            player_mcbt = player.GetComponent<MechCombat>();
        }
    }

    public override void OnPlayerDead(GameObject player, int shooter_ViewID, string weapon) {      
        PhotonView player_pv = player.GetComponent<PhotonView>();
        if(player_pv == null) {
            Debug.LogError("This player does not have photonview.");
            return;
        }

        // Update scoreboard
        RegisterKill(shooter_ViewID, player_pv.viewID);
        PhotonView shooterpv = PhotonView.Find(shooter_ViewID);
        DisplayKillMsg(shooterpv.owner.NickName, player_pv.owner.NickName, weapon);


        //Master check if this player has the flag
        if (PhotonNetwork.isMasterClient) {
            if (BlueFlagHolder != null && player_pv.owner.NickName ==  BlueFlagHolder.NickName) {
                //Teleport the flag to ground
                RaycastHit hit;
                Physics.Raycast(player.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);

                photonView.RPC("DropFlag", PhotonTargets.All, player_pv.viewID, 0, hit.point);
            } else if (RedFlagHolder != null && player_pv.owner.NickName == RedFlagHolder.NickName) {
                RaycastHit hit;
                Physics.Raycast(player.transform.position, -Vector3.up, out hit, 1000, TerrainLayerMask);

                photonView.RPC("DropFlag", PhotonTargets.All, player_pv.viewID, 1, hit.point);
            }
        }
    }

    protected override void SyncPanel() {//TODO : check if this sync well
        CTFPanelManager.Init();
        blueScore = (PhotonNetwork.room.CustomProperties["BlueScore"] == null) ? 0 : int.Parse(PhotonNetwork.room.CustomProperties["BlueScore"].ToString());
        redScore = (PhotonNetwork.room.CustomProperties["RedScore"] == null) ? 0 : int.Parse(PhotonNetwork.room.CustomProperties["RedScore"].ToString());
        CTFPanelManager.UpdateScoreText(blueScore, redScore);
    }

    protected override bool CheckIfGameSync() {
        if (flag_is_sync) {
            return true;
        } else {
            BlueFlag = GameObject.Find("BlueFlag(Clone)");
            RedFlag = GameObject.Find("RedFlag(Clone)");

            photonView.RPC("SyncFlagRequest", PhotonTargets.MasterClient);
            return false;
        }
    }

    protected override void MasterInitGame() {
        base.MasterInitGame();

        //Init scores
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                  { "BlueScore", 0 },
                  { "RedScore", 0 },
                  { "GameInit", true }//this is set to false when master pressing "start"
        };
        PhotonNetwork.room.SetCustomProperties(h);


        InstantiateFlags();
    }

    protected override void LoadMapSetting() {
        string map = GameInfo.Map;
        GameObject g = (GameObject)Resources.Load("MapSetting/"+map + "_CTF");//debug use

        if(g == null)
            Debug.LogError("Can't find "+ map + "_CTF"+ " in Resources/MapSetting folder.");

        PhotonNetwork.InstantiateSceneObject("MapSetting/"+map + "_CTF", Vector3.zero, Quaternion.identity, 0, null);

        PanelCanvas.GetComponentInChildren<RespawnPanel>(true).InitMap();
        CTFPanelManager.InitMap();

        territories = FindObjectsOfType<TerritoryController>();

    }

    private void SetHealthPoolLookAtPlayer() {
        HealthPool[] healthpools = FindObjectsOfType<HealthPool>();
        foreach (HealthPool h in healthpools) {
            h.player = player;
            h.Init();
        }
    }

    private void InitTerritories() {        
        foreach (TerritoryController g in territories) {
            g.Init();

            if (PhotonNetwork.isMasterClient) {
                Debug.Log("init point : "+ g.Territory_ID);
                ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable {
                    { "T_" + g.Territory_ID, g.curTerritoryState }
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

    private void SetTerritoriesLookAtPlayer() {
        territories = FindObjectsOfType<TerritoryController>();
        foreach (TerritoryController g in territories) {
            g.SetPlayerToLookAt(player);
        }
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

    void InstantiateFlags() {
        BlueFlag = PhotonNetwork.InstantiateSceneObject("BlueFlag", new Vector3(territories[0].transform.position.x, 0, territories[0].transform.position.z), Quaternion.Euler(Vector3.zero), 0, null);
        RedFlag = PhotonNetwork.InstantiateSceneObject("RedFlag", new Vector3(territories[territories.Length - 1].transform.position.x, 0, territories[territories.Length - 1].transform.position.z), Quaternion.Euler(Vector3.zero), 0, null);
    }

    protected override bool CheckIfGameEnd() {
        return Timer.CheckIfGameEnd();
    }

    public override void SetRespawnPoint(int num) {
        Debug.Log("Set point : "+num);
        PunTeams.Team player_team = PhotonNetwork.player.GetTeam();
        if (player_team == PunTeams.Team.red && int.Parse(PhotonNetwork.room.CustomProperties["T_" + num].ToString()) == (int)Team.RED) {//TODO : not T_ + num
            respawnPoint = num;
        } else if(player_team == PunTeams.Team.blue && int.Parse(PhotonNetwork.room.CustomProperties["T_" + num].ToString()) == (int)Team.BLUE) {
            respawnPoint = num;
        } else {
            respawnPoint = (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) ? territories.Length-1 : 0;//RedBase must be the last one of territories
        }
    }

    [PunRPC]
    void SyncFlagRequest() {//TODO : check this
        //Always received by master

        if (BlueFlagHolder == null) {
            photonView.RPC("SetFlag", PhotonTargets.All, -1, (int)Team.BLUE, BlueFlag.transform.position);
        } else {
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
    void GetFlagRequest(int player_viewID, int flag) {//flag 0 : blue team ;  1 : red 
    //Always received by master
        PhotonPlayer player = PhotonView.Find(player_viewID).owner;

        //check the current holder
        if (flag == (int)Team.BLUE) {
            if (BlueFlagHolder != null) {//someone has taken it first
                return;
            } else {
                //RPC all to give this flag to the player
                photonView.RPC("SetFlag", PhotonTargets.All, player_viewID, flag, Vector3.zero);
                Debug.Log(player + " has the blue flag.");
            }
        } else {
            if (RedFlagHolder != null) {
                return;
            } else {
                //RPC all to give this flag to the player
                photonView.RPC("SetFlag", PhotonTargets.All, player_viewID, flag, Vector3.zero);
                Debug.Log(player + " has the red flag.");
            }
        }
    }

    void SetFlagProperties(int flag, Transform parent, Vector3 pos, PhotonPlayer holder) {
        if (flag == (int)Team.BLUE) {
            if (parent != null) {
                BlueFlag.transform.parent = parent;
                BlueFlag.transform.localPosition = Vector3.zero;
                BlueFlag.transform.localRotation = Quaternion.Euler(new Vector3(225, -180, 180));
                BlueFlagHolder = holder;
                BlueFlag.GetComponent<Flag>().isGrounded = false;
            } else {
                BlueFlag.transform.parent = null;
                BlueFlag.transform.position = pos;
                BlueFlag.transform.rotation = Quaternion.identity;
                BlueFlagHolder = null;
                BlueFlag.GetComponent<Flag>().isGrounded = true;
                BlueFlag.GetComponent<Flag>().isOnBase = false;
                BlueFlag.layer = 2;//ignoreRaycast
            }
        } else {
            if (parent != null) {
                RedFlag.transform.parent = parent;
                RedFlag.transform.localPosition = Vector3.zero;
                RedFlag.transform.localRotation = Quaternion.Euler(new Vector3(225, -180, 180));
                RedFlagHolder = holder;
                RedFlag.GetComponent<Flag>().isGrounded = false;
            } else {
                RedFlag.transform.parent = null;
                RedFlag.transform.position = pos;
                RedFlag.transform.rotation = Quaternion.identity;
                RedFlagHolder = null;
                RedFlag.GetComponent<Flag>().isGrounded = true;
                RedFlag.GetComponent<Flag>().isOnBase = false;
                RedFlag.layer = 2;
            }
        }
    }

    [PunRPC]
    void SetFlag(int player_viewID, int flag, Vector3 pos) {
        if (BlueFlag == null || RedFlag == null)
            return;
        flag_is_sync = true;
        if (player_viewID == -1) {//put the flag to the pos 
            if (flag == (int)Team.BLUE) {
                SetFlagProperties((int)Team.BLUE, null, pos, null);

                if (BlueFlag.transform.position.x == territories[0].transform.position.x && BlueFlag.transform.position.z == territories[0].transform.position.z) {
                    BlueFlag.GetComponent<Flag>().isOnBase = true;
                }
            } else {
                SetFlagProperties((int)Team.RED, null, pos, null);

                if (RedFlag.transform.position.x == territories[territories.Length - 1].transform.position.x && RedFlag.transform.position.z == territories[territories.Length - 1].transform.position.z) {
                    RedFlag.GetComponent<Flag>().isOnBase = true;
                }
            }

        } else {
            PhotonView pv = PhotonView.Find(player_viewID);
            if (flag == (int)Team.BLUE) {
                SetFlagProperties((int)Team.BLUE, pv.transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/BackPack_Bone"), Vector3.zero, pv.owner);
                BlueFlag.GetComponent<Flag>().isOnBase = false;
            } else {
                SetFlagProperties((int)Team.RED, pv.transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/BackPack_Bone"), Vector3.zero, pv.owner);
                RedFlag.GetComponent<Flag>().isOnBase = false;
            }
        }
    }

    [PunRPC]
    void GetScoreRequest(int player_viewID) {
        //Always received by master

        PhotonView pv = PhotonView.Find(player_viewID);
        if (pv == null) {
            Debug.LogWarning("can't find pv");
            return;
        }

        //check if no one is taking the another flag
        if (pv.owner.GetTeam() == PunTeams.Team.blue || pv.owner.GetTeam() == PunTeams.Team.none) {
            if (BlueFlagHolder != null || RedFlagHolder == null) {
                return;
            } else {
                photonView.RPC("RegisterScore", PhotonTargets.All, player_viewID);

                //send back the flag
                photonView.RPC("SetFlag", PhotonTargets.All, -1, (int)Team.RED, new Vector3(territories[territories.Length - 1].transform.position.x, 0, territories[territories.Length - 1].transform.position.z));
            }
        } else {//Redteam : blue flag holder
            if (BlueFlagHolder == null || RedFlagHolder != null) {
                return;
            } else {
                photonView.RPC("RegisterScore", PhotonTargets.All, player_viewID);

                photonView.RPC("SetFlag", PhotonTargets.All, -1, (int)Team.BLUE, new Vector3(territories[0].transform.position.x, 0, territories[0].transform.position.z));
            }
        }
    }

    protected override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        base.OnPhotonPlayerDisconnected(player);
        Debug.Log("Called OnPhotonPlayerDisconnected in CTFManager.");
        //Check if this player has the flag
        if (player.NickName == ((BlueFlagHolder == null) ? "" : BlueFlagHolder.NickName)) {
            SetFlagProperties((int)Team.BLUE, null, new Vector3(BlueFlag.transform.position.x, 0, BlueFlag.transform.position.z), null);
        } else if (player.NickName == ((RedFlagHolder == null) ? "" : RedFlagHolder.NickName)) {
            SetFlagProperties((int)Team.RED, null, new Vector3(RedFlag.transform.position.x, 0, RedFlag.transform.position.z), null);
        }
    }

    protected override void OnGameEndRelease() {
        if(!PhotonNetwork.isMasterClient)return;

        //Destroy scene objects
        PhotonNetwork.Destroy(BlueFlag);
        PhotonNetwork.Destroy(RedFlag);
    }

    [PunRPC]
    void RegisterScore(int player_viewID) {
        PhotonPlayer player = PhotonView.Find(player_viewID).owner;

        if (player.GetTeam() == PunTeams.Team.blue || player.GetTeam() == PunTeams.Team.none) {
            blueScore++;
            //BlueScoreText.text = bluescore.ToString();
        } else {
            redScore++;
            //RedScoreText.text = redscore.ToString();
        }

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
        return territories[num].transform.position + new Vector3(0,5,0);
    }

    public Vector3 GetBlueBasePosition() {
        return territories[0].transform.position;
    }

    public Vector3 GetRedBasePosition() {
        return territories[territories.Length-1].transform.position;
    }
}