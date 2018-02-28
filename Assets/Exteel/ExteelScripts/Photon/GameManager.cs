using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Photon.MonoBehaviour {
	public static bool isTeamMode;
	public float TimeLeft;

	[SerializeField] GameObject PlayerPrefab;
	[SerializeField] GameObject Scoreboard,RespawnPanel;
	[SerializeField] GameObject PlayerStat;
	[SerializeField] Text Timer;
	[SerializeField] bool Offline;
	[SerializeField] GameObject Panel_RedTeam, Panel_BlueTeam;
	[SerializeField] GameObject RedScore, BlueScore;
	[SerializeField] Text RedScoreText, BlueScoreText;
	[SerializeField] GameObject MechFrame;

	GreyZone Zone;

	public InRoomChat InRoomChat;
	public Transform[] SpawnPoints;
	public PhotonPlayer BlueFlagHolder = null, RedFlagHolder = null;
	private GameObject RedFlag, BlueFlag;
	private GameObject player;

	public int MaxTimeInSeconds = 300;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;
	private int bluescore = 0, redscore = 0;
	private int timerDuration;
	private int currentTimer = 999;

	private bool showboard = false;
	private HUD hud;
	public Camera cam;
	private MechCombat mcbt;
	private bool gameEnding = false;
	private bool OnSyncTimeRequest = false;
	private bool IsMasterInitGame = false; 
	private bool OnCheckInitGame = false;
	private bool flag_is_sync = false;
	private bool canStart = false; // this is true when all player finish loading
	private int waitTimes = 0;
	private float lastCheckCanStartTime = 0;

	public  bool GameIsBegin = false; 
	private bool callGameBegin = false;
	private int GameBeginTime = 0; 

	private int sendTimes = 0;
	private int respawnPoint;
	private int playerfinishedloading = 0;
	//debug
	public bool callEndGame = false;

	private Dictionary<string, GameObject> playerScorePanels;
	public Dictionary<string, Score> playerScores;

	int storedStartTime;
	int storedDuration;

	private BuildMech mechBuilder;
	float curtime;

	void Start() {
		if (Offline) {
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.CreateRoom("offline");
			GameInfo.MaxKills = 1;
			GameInfo.MaxTime = 1;
		}
		//Load game info
		GameInfo.Map = PhotonNetwork.room.CustomProperties ["Map"].ToString();
		GameInfo.GameMode = PhotonNetwork.room.CustomProperties ["GameMode"].ToString();
		GameInfo.MaxKills = int.Parse(PhotonNetwork.room.CustomProperties ["MaxKills"].ToString());
		GameInfo.MaxTime =  int.Parse(PhotonNetwork.room.CustomProperties ["MaxTime"].ToString());
		GameInfo.MaxPlayers = PhotonNetwork.room.MaxPlayers;
		MaxKills = GameInfo.MaxKills;
		MaxTimeInSeconds = GameInfo.MaxTime * 60;
		InRoomChat.enabled = true;
		playerScorePanels = new Dictionary<string, GameObject>();
		RedFlagHolder = null;
		BlueFlagHolder = null;

		Debug.Log ("GameInfo gamemode :" + GameInfo.GameMode + " MaxKills :" + GameInfo.MaxKills);

		//If is master client , initialize room's properties ( team score, ... )
		if (PhotonNetwork.isMasterClient) {
			MasterInitGame();
		}

		//Check team mode
		if(GameInfo.GameMode.Contains("Team") || GameInfo.GameMode.Contains("Capture")){
			Debug.Log ("Team mode is on.");
			isTeamMode = true;
		}else{
			Debug.Log ("Team mode is off.");
			//close Panel_RedTeam on Scorepanel
			Panel_RedTeam.SetActive (false);
			//close teamscores
			RedScore.SetActive (false);
			BlueScore.SetActive (false);
			isTeamMode = false;
		}
			
		StartCoroutine(LateStart());

		// client ini himself
		ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
		h2.Add ("Kills", 0);
		h2.Add ("Deaths", 0);
		h2.Add ("weaponOffset", 0);
		PhotonNetwork.player.SetCustomProperties (h2);


		if (isTeamMode) {
			if(PhotonNetwork.player.GetTeam() == PunTeams.Team.blue || PhotonNetwork.player.GetTeam() == PunTeams.Team.none)
				InstantiatePlayer (PlayerPrefab.name, RandomXZposition (SpawnPoints [0].position, 20), SpawnPoints [0].rotation, 0);
			else{
				InstantiatePlayer (PlayerPrefab.name, RandomXZposition (SpawnPoints [1].position, 20),SpawnPoints [1].rotation, 0);
			}
			Zone = GameObject.Find ("GreyZone").GetComponent<GreyZone>();
			if (PhotonNetwork.room.CustomProperties ["Zone"] != null) {
				Zone.ChangeZone (int.Parse (PhotonNetwork.room.CustomProperties ["Zone"].ToString ()));
			} else {
				Zone.ChangeZone (-1);
			}
		}else{
			InstantiatePlayer (PlayerPrefab.name, RandomXZposition (SpawnPoints [0].position, 20), SpawnPoints [0].rotation, 0);
		}

		hud.ShowWaitOtherPlayer (true);
	}
		
	IEnumerator LateStart(){
		if(!IsMasterInitGame && sendTimes <10){//sometime master connects very slow
				sendTimes++;
				InRoomChat.AddLine ("Sending sync game request..." + sendTimes);
				yield return new WaitForSeconds (1f);
				SendSyncInitGameRequest ();
				yield return StartCoroutine (LateStart ());
		}else{
			if(sendTimes >= 10){
				InRoomChat.AddLine ("Failed to sync game properties. Is master disconnected ? ");
				Debug.Log ("master not connected");
			}else{
				InRoomChat.AddLine ("Game is sync.");
				photonView.RPC ("PlayerFinishedLoading", PhotonTargets.AllBuffered);
			}

			BlueScoreText.text = (PhotonNetwork.room.CustomProperties ["BlueScore"]==null)? "0" : PhotonNetwork.room.CustomProperties ["BlueScore"].ToString();
			RedScoreText.text = (PhotonNetwork.room.CustomProperties ["RedScore"]==null)? "0" : PhotonNetwork.room.CustomProperties ["RedScore"].ToString ();
			bluescore = int.Parse (BlueScoreText.text);
			redscore = int.Parse (RedScoreText.text);

		}

	}

	void SendSyncInitGameRequest(){
		if(bool.Parse(PhotonNetwork.room.CustomProperties["GameInit"].ToString())){
			if(GameInfo.GameMode.Contains("Capture") && !flag_is_sync){
				BlueFlag = GameObject.Find ("BlueFlag(Clone)");
				RedFlag = GameObject.Find ("RedFlag(Clone)");

				photonView.RPC ("SyncFlagRequest", PhotonTargets.MasterClient);	
			} else {
				IsMasterInitGame = true;
			}
		}
	}

	public void InstantiatePlayer(string name, Vector3 StartPos, Quaternion StartRot, int group){
		player = PhotonNetwork.Instantiate (PlayerPrefab.name, StartPos, StartRot, 0);
		mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech[0]; // HERE !
		mechBuilder.Build (m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);


		if(player.GetComponent<PhotonView>().isMine){
			cam = player.transform.Find("Camera").GetComponent<Camera>();
			hud = GameObject.Find("Canvas").GetComponent<HUD>();
			mcbt = player.GetComponent<MechCombat> ();
		}
	}
		
	public void RegisterPlayer(int viewID, int teamID) {
		PhotonView pv = PhotonView.Find (viewID);
		string name;
		if(viewID == 2){//Drone
			name = "Drone";
		}else{
			name = pv.owner.NickName;
		}

		//bug : client is not ini. K/D even if ini. in start
		if(pv.isMine){
			ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
			h2.Add ("Kills", 0);
			h2.Add ("Deaths", 0);
			PhotonNetwork.player.SetCustomProperties (h2);
		}

		if (playerScores == null) {
			playerScores = new Dictionary<string, Score>();
		}

		GameObject ps = Instantiate (PlayerStat, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
		ps.transform.Find("Pilot Name").GetComponent<Text>().text = name;
		ps.transform.Find("Kills").GetComponent<Text>().text = "0";
		ps.transform.Find("Deaths").GetComponent<Text>().text = "0";

		Score score = new Score ();
		if (viewID != 2) {
			string kills, deaths;
			kills = pv.owner.CustomProperties ["Kills"].ToString ();
			deaths = pv.owner.CustomProperties ["Deaths"].ToString ();
			ps.transform.Find ("Kills").GetComponent<Text> ().text = kills;
			ps.transform.Find ("Deaths").GetComponent<Text> ().text = deaths;

			score.Kills = int.Parse(kills);
			score.Deaths = int.Parse(deaths);
		}
		playerScores.Add (name, score);

		//scorepanel
		if(isTeamMode){
			if(teamID == 0){
				ps.transform.SetParent(Panel_BlueTeam.transform);
			}else{
				ps.transform.SetParent(Panel_RedTeam.transform);
			}
		}else
			ps.transform.SetParent(Panel_BlueTeam.transform);

		ps.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		playerScorePanels.Add(name, ps);
	}

	void MasterInitGame(){
		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("BlueScore", 0);
		h.Add ("RedScore", 0);
		h.Add ("GameInit", true);//this is set to false when master pressing "start"
		if(isTeamMode){
			h.Add ("Zone", -1);
		}
		PhotonNetwork.room.SetCustomProperties (h);
		SyncTime();

		//Instantiate flags
		if(GameInfo.GameMode.Contains("Capture")){
			InstantiateFlags ();
		}

		IsMasterInitGame = true;
	}

	void InstantiateFlags(){
		BlueFlag = PhotonNetwork.InstantiateSceneObject ("BlueFlag", new Vector3(SpawnPoints [0].position.x , 0 , SpawnPoints [0].position.z), Quaternion.Euler(Vector3.zero), 0, null);
		RedFlag = PhotonNetwork.InstantiateSceneObject ("RedFlag", new Vector3(SpawnPoints [1].position.x , 0 , SpawnPoints [1].position.z), Quaternion.Euler(Vector3.zero), 0, null);
	}

	void SyncTime() {
		int startTime = PhotonNetwork.ServerTimestamp;
		ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "startTime", startTime }, { "duration", MaxTimeInSeconds } };
		Debug.Log("Setting " + startTime + ", " + MaxTimeInSeconds);
		PhotonNetwork.room.SetCustomProperties(ht);
	}

	public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
		if (propertiesThatChanged.ContainsKey("startTime") && propertiesThatChanged.ContainsKey("duration")) {
			storedStartTime = (int)propertiesThatChanged["startTime"];
			storedDuration = (int)propertiesThatChanged["duration"];
		}
	}

	void Update() {
		if (!GameOver () && !gameEnding) {
			if (!mcbt.isDead) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}else{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			Scoreboard.SetActive(Input.GetKey(KeyCode.CapsLock));
		}
	

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Lobby");
		}
		// Update time
		if (storedStartTime != 0 && storedDuration != 0) {
			timerDuration = (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
			currentTimer = storedDuration - timerDuration;

			int seconds = currentTimer % 60;
			int minutes = currentTimer / 60;
			Timer.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");
		}else{
			if (!OnSyncTimeRequest)
				StartCoroutine (SyncTimeRequest(1f));
		}

		if (GameOver() && !gameEnding) {
			if(PhotonNetwork.isMasterClient){
				photonView.RPC ("EndGame", PhotonTargets.All);
			}
		}
	}

	void FixedUpdate(){
		if(!GameIsBegin){
			if(currentTimer <= GameBeginTime){
				SetGameBegin ();
				hud.ShowWaitOtherPlayer (false);
				Debug.Log ("set game begin");
			}

			if(PhotonNetwork.isMasterClient && !callGameBegin){
				if (!canStart) {
					if (waitTimes <= 5) {// check if all player finish loading every 2 sec
						if (Time.time - lastCheckCanStartTime >= 2f) {
							waitTimes++;
							lastCheckCanStartTime = Time.time;
						}
					} else {//wait too long
						photonView.RPC ("CallGameBeginAtTime", PhotonTargets.AllBuffered, currentTimer - 2);
						callGameBegin = true;
					}
				}
				else{//all player finish loading
					photonView.RPC ("CallGameBeginAtTime", PhotonTargets.AllBuffered, currentTimer-2);
					callGameBegin = true;
				}
			}
		}	
	}


	IEnumerator SyncTimeRequest(float time){
		OnSyncTimeRequest = true;
		storedStartTime = int.Parse(PhotonNetwork.room.CustomProperties ["startTime"].ToString());
		storedDuration = int.Parse(PhotonNetwork.room.CustomProperties["duration"].ToString());
		yield return new WaitForSeconds (time);
		OnSyncTimeRequest = false;
	}

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		if(PhotonNetwork.isMasterClient){//master destroy scene objects
			if(GameInfo.GameMode.Contains("Capture")){
				PhotonNetwork.Destroy (BlueFlag);
				PhotonNetwork.Destroy (RedFlag);
			}
			PhotonNetwork.LoadLevel("GameLobby");
		}

		// Code to execute after the delay
		Cursor.visible = true;
	}

	public void RegisterKill(int shooter_viewID, int victim_viewID) {

		if(victim_viewID == 2){//drone
			return;
		}
		PhotonPlayer shooter_player = null,victime_player = null;
		shooter_player = PhotonView.Find (shooter_viewID).owner;
		victime_player = PhotonView.Find (victim_viewID).owner;

		string shooter = shooter_player.NickName, victim = victime_player.NickName;

		//only master update the room properties
		if(PhotonNetwork.isMasterClient){
			ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
			h2.Add ("Kills", playerScores[shooter].Kills + 1);

			ExitGames.Client.Photon.Hashtable h3 = new ExitGames.Client.Photon.Hashtable ();
			h3.Add ("Deaths", playerScores[victim].Deaths + 1 );
			shooter_player.SetCustomProperties (h2);
			victime_player.SetCustomProperties (h3);

			if (GameInfo.GameMode == "Team Deathmatch") {
				if (shooter_player.GetTeam () == PunTeams.Team.blue || shooter_player.GetTeam () == PunTeams.Team.none) {
					bluescore++;
					BlueScoreText.text = bluescore.ToString();
					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("BlueScore", bluescore);
					PhotonNetwork.room.SetCustomProperties (h);
				}else{
					redscore++;
					RedScoreText.text = redscore.ToString();
					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("RedScore", redscore);
					PhotonNetwork.room.SetCustomProperties (h);
				}
			}
		}else{
			if(GameInfo.GameMode == "Team Deathmatch"){
				if (shooter_player.GetTeam () == PunTeams.Team.blue || shooter_player.GetTeam () == PunTeams.Team.none) {
					bluescore++;
					BlueScoreText.text = bluescore.ToString();
				} else {
					redscore++;
					RedScoreText.text = redscore.ToString();
				}
			}
		}
		Score newShooterScore = new Score ();
		newShooterScore.Kills = playerScores[shooter].Kills + 1;
		newShooterScore.Deaths = playerScores[shooter].Deaths;
		playerScores [shooter] = newShooterScore;

		Score newVictimScore = new Score ();
		newVictimScore.Kills = playerScores [victim].Kills;
		newVictimScore.Deaths = playerScores [victim].Deaths + 1;
		playerScores [victim] = newVictimScore;

		playerScorePanels [shooter].transform.Find("Kills").GetComponent<Text>().text = playerScores[shooter].Kills.ToString();
		playerScorePanels [victim].transform.Find("Deaths").GetComponent<Text>().text = playerScores[victim].Deaths.ToString();

		if (newShooterScore.Kills > CurrentMaxKills) CurrentMaxKills = newShooterScore.Kills;
	}

	public bool GameOver() {
		if (storedStartTime != 0 && storedDuration != 0) {
			if (currentTimer <= 0 || callEndGame) {
				return true;
			} else {
				return CurrentMaxKills >= MaxKills;
			}
		} else
			return false;
	}
	void OnPhotonPlayerConnected(PhotonPlayer newPlayer){
		//if (!PhotonNetwork.isMasterClient)
		//	return;
		InRoomChat.AddLine (newPlayer + " is connected.");
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer player){
		InRoomChat.AddLine (player + " is disconnected.");
		playerScorePanels.Remove (player.NickName);
		playerScores.Remove (player.NickName);

		//Remove datas from scoreboard
		Text[] Ts = Scoreboard.GetComponentsInChildren<Text> ();
		foreach(Text text in Ts){
			if(text.text == player.NickName){
				Destroy (text.transform.parent.gameObject);
				break;
			}
		}

		//check if he had the flag
		if (GameInfo.GameMode.Contains ("Capture")) {
			if (player.NickName == ((BlueFlagHolder == null) ? "" : BlueFlagHolder.NickName)) {
				print (player.NickName + " " + BlueFlagHolder.NickName);
				SetFlagProperties (0, null, new Vector3 (BlueFlag.transform.position.x, 0, BlueFlag.transform.position.z), null);
			}else if(player.NickName == ((RedFlagHolder == null) ? "" : RedFlagHolder.NickName)){
				SetFlagProperties (1, null, new Vector3 (RedFlag.transform.position.x, 0, RedFlag.transform.position.z), null);
			}
		}
	}


	[PunRPC]
	void GetFlagRequest(int player_viewID, int flag){//flag 0 : blue team ;  1 : red
		//this is always received by master

		PhotonPlayer player = PhotonView.Find (player_viewID).owner;

		//check the current holder
		if(flag == 0){
			if(BlueFlagHolder!=null){//someone has taken it first
				return;
			}else{
				//RPC all to give this flag to the player
				photonView.RPC("SetFlag",PhotonTargets.All, player_viewID, flag, Vector3.zero);
				Debug.Log (player + " has the blue flag.");
			}
		}else{
			if(RedFlagHolder!=null){
				return;
			}else{
				//RPC all to give this flag to the player
				photonView.RPC("SetFlag",PhotonTargets.All, player_viewID, flag, Vector3.zero);
				Debug.Log (player + " has the red flag.");
			}
		}
	}
	[PunRPC]
	void SyncFlagRequest(){
		//always received by master

		if(BlueFlagHolder==null){
			photonView.RPC ("SetFlag", PhotonTargets.All, -1, 0, BlueFlag.transform.position);
		}else{
			photonView.RPC ("SetFlag", PhotonTargets.All, (BlueFlag.transform.root).GetComponent<PhotonView>().viewID, 0, Vector3.zero);
		}
			
		if(RedFlagHolder==null){
			photonView.RPC ("SetFlag", PhotonTargets.All, -1, 1, RedFlag.transform.position);
		}else{
			photonView.RPC ("SetFlag", PhotonTargets.All, (RedFlag.transform.root).GetComponent<PhotonView>().viewID, 1, Vector3.zero);
		}
	}

	public void SetRespawnPoint(int num){
		if(isTeamMode){//num 2 is the grey zone
			if(num==2){
				if (PhotonNetwork.player.GetTeam () == PunTeams.Team.red) {
					if (int.Parse (PhotonNetwork.room.CustomProperties ["Zone"].ToString ()) == 1) {
						respawnPoint = num;
						print ("set point : " + num);
					}
				} else {
					if (int.Parse (PhotonNetwork.room.CustomProperties ["Zone"].ToString ()) == 0) {
						respawnPoint = num;
						print ("set point : " + num);
					}
				}
			}else{
				if (PhotonNetwork.player.GetTeam () == PunTeams.Team.red) {
					if (num==1) {
						respawnPoint = num;
						print ("set point : " + num);
					}
				} else {
					if (num==0) {
						respawnPoint = num;
						print ("set point : " + num);
					}
				}
			}
		} else {
			respawnPoint = num;
			print ("set point : " + num);
		}
	}

	public int GetRespawnPoint(){
		return respawnPoint;
	}

	public void CallRespawn(int mech_num){
		print ("call respawn with point : " + respawnPoint);
		CloseRespawnPanel();
		mcbt.GetComponent<PhotonView> ().RPC ("EnablePlayer", PhotonTargets.All, respawnPoint, mech_num);
		mcbt.isDead = false;
	}

	public void ShowRespawnPanel(){
		RespawnPanel.SetActive (true);
		MechFrame.GetComponent<BuildMech> ().CheckAnimatorState ();
	}

	public void CloseRespawnPanel(){
		RespawnPanel.SetActive (false);
	}

	[PunRPC]
	void SetFlag(int player_viewID, int flag, Vector3 pos){
		if (BlueFlag == null || RedFlag == null)
			return;
		flag_is_sync = true;
		if(player_viewID == -1){//put the flag to the pos 
			if(flag == 0){
				SetFlagProperties (0, null, pos, null);

				if(BlueFlag.transform.position.x == SpawnPoints[0].position.x && BlueFlag.transform.position.z == SpawnPoints[0].position.z){
					BlueFlag.GetComponent<Flag> ().isOnBase = true;
				}
			}else{
				SetFlagProperties (1, null, pos, null);

				if(RedFlag.transform.position.x == SpawnPoints[1].position.x && RedFlag.transform.position.z == SpawnPoints[1].position.z){
					RedFlag.GetComponent<Flag> ().isOnBase = true;
				}
			}

		}else{
			PhotonView pv = PhotonView.Find (player_viewID);
			if(flag == 0){
				SetFlagProperties (0, pv.transform.Find ("CurrentMech/metarig/hips/spine/chest/neck"), Vector3.zero, pv.owner);
				BlueFlag.GetComponent<Flag> ().isOnBase = false;
			}else{
				SetFlagProperties (1, pv.transform.Find ("CurrentMech/metarig/hips/spine/chest/neck"), Vector3.zero, pv.owner);
				RedFlag.GetComponent<Flag> ().isOnBase = false;
			}
		}
	}

	[PunRPC]
	void DropFlag(int player_viewID, int flag, Vector3 pos){//also call when disable player
		if(flag == 0){
			SetFlagProperties (0, null, pos, null);

			//when disabling player , flag's renderer gets turn off
			Renderer[] renderers = BlueFlag.GetComponentsInChildren<Renderer> ();
			foreach(Renderer renderer in renderers){
				renderer.enabled = true;
			}
		}else{
			SetFlagProperties (1, null, pos, null);

			Renderer[] renderers = RedFlag.GetComponentsInChildren<Renderer> ();
			foreach(Renderer renderer in renderers){
				renderer.enabled = true;
			}
		}

	}

	[PunRPC]
	void GetScoreRequest(int player_viewID){
		//this is always received by master

		PhotonView pv = PhotonView.Find (player_viewID);

		//check if no one is taking the another flag
		if(pv.owner.GetTeam() == PunTeams.Team.blue || pv.owner.GetTeam() == PunTeams.Team.none){
			if (BlueFlagHolder!=null || RedFlagHolder==null) {
				return;
			}else{
				photonView.RPC ("RegisterScore", PhotonTargets.All, player_viewID);

				//send back the flag
				photonView.RPC ("SetFlag", PhotonTargets.All, -1, 1, new Vector3 (SpawnPoints[1].transform.position.x, 0, SpawnPoints[1].transform.position.z));
			}
		}else{//Redteam : blue flag holder
			if (BlueFlagHolder==null || RedFlagHolder != null) {
				return;
			}else{
				photonView.RPC ("RegisterScore", PhotonTargets.All, player_viewID);

				photonView.RPC ("SetFlag", PhotonTargets.All, -1, 0, new Vector3 (SpawnPoints [0].transform.position.x, 0, SpawnPoints [0].transform.position.z));
			}
		}
	}

	[PunRPC]
	void RegisterScore(int player_viewID){
		PhotonPlayer player = PhotonView.Find (player_viewID).owner;

		if (player.GetTeam () == PunTeams.Team.blue || player.GetTeam () == PunTeams.Team.none) {
			bluescore++;
			BlueScoreText.text = bluescore.ToString();
		} else {
			redscore++;
			RedScoreText.text = redscore.ToString();
		}

		if(PhotonNetwork.isMasterClient){
			if (player.GetTeam () == PunTeams.Team.blue || player.GetTeam () == PunTeams.Team.none) {
				ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
				h.Add ("BlueScore", bluescore);
				PhotonNetwork.room.SetCustomProperties (h);
			}else{
				ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
				h.Add ("RedScore", redscore);
				PhotonNetwork.room.SetCustomProperties (h);
			}
		}
	}

	void SetFlagProperties(int flag, Transform parent, Vector3 pos, PhotonPlayer holder){
		if(flag==0){
			if (parent != null) {
				BlueFlag.transform.parent = parent;
				BlueFlag.transform.localPosition = Vector3.zero;
				BlueFlag.transform.localRotation = Quaternion.Euler (new Vector3(-30,0,0));
				BlueFlagHolder = holder;
				BlueFlag.GetComponent<Flag> ().isGrounded = false;
			}else{
				BlueFlag.transform.parent = null;
				BlueFlag.transform.position = pos;
				BlueFlag.transform.rotation = Quaternion.identity;
				BlueFlagHolder = null;
				BlueFlag.GetComponent<Flag> ().isGrounded = true;
			}
		}else{
			if (parent != null) {
				RedFlag.transform.parent = parent;
				RedFlag.transform.localPosition = Vector3.zero;
				RedFlag.transform.localRotation = Quaternion.Euler (new Vector3(-30,0,0));
				RedFlagHolder = holder;
				RedFlag.GetComponent<Flag> ().isGrounded = false;
			}else{
				RedFlag.transform.parent = null;
				RedFlag.transform.position = pos;
				RedFlag.transform.rotation = Quaternion.identity;
				RedFlagHolder = null;
				RedFlag.GetComponent<Flag> ().isGrounded = true;
			}
		}
	}

	[PunRPC]
	void EndGame(){
		gameEnding = true;
		Cursor.lockState = CursorLockMode.None;
		hud.ShowText(cam, cam.transform.position + new Vector3(0,0,0.5f), "GameOver");//every player's hud on Gamemanager is his
		Scoreboard.SetActive(true);
		StartCoroutine(ExecuteAfterTime(3));
	}

	[PunRPC]
	void PlayerFinishedLoading(){
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
	void CallGameBeginAtTime(int time){
		GameBeginTime = time;
	}

	void SetGameBegin(){
		GameIsBegin = true;
	}

	Vector3 RandomXZposition(Vector3 pos, float radius){
		float x = Random.Range (pos.x - radius, pos.x + radius);
		float z = Random.Range (pos.z - radius, pos.z + radius);
		return new Vector3 (x, pos.y, z);
	}

	Quaternion RandomYrotation(Quaternion qua){
		Vector3 euler = qua.eulerAngles;
		return Quaternion.Euler (new Vector3 (euler.x, Random.Range (0, 180), euler.z));
	}
}
