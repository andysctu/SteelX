using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Photon.MonoBehaviour {
	public static bool isTeamMode;
	public float TimeLeft;

	[SerializeField] GameObject PlayerPrefab;
	[SerializeField] GameObject Scoreboard;
	[SerializeField] GameObject PlayerStat;
	[SerializeField] Text Timer;
	[SerializeField] bool Offline;
	[SerializeField] GameObject Panel_RedTeam, Panel_BlueTeam;
	[SerializeField] GameObject RedScore, BlueScore;
	[SerializeField] Text RedScoreText, BlueScoreText;

	public InRoomChat InRoomChat;
	public Transform[] SpawnPoints;
	private PhotonPlayer BlueFlagHolder = null, RedFlagHolder = null;
	private static Vector3 BlueFlagPos = Vector3.zero, RedFlagPos = Vector3.zero;

	//when a player disconnects , if he is the flag holder , we can only use this name check ( can't get customproperty from a disconnected player )
	private string BlueFlagHolderName = "",RedFlagHolderName = "";
	private GameObject RedFlag, BlueFlag;

	public int MaxTimeInSeconds = 300;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;
	private int bluescore = 0, redscore = 0;
	private int timerDuration;
	private int currentTimer = 999;

	private bool showboard = false;
	private HUD hud;
	private Camera cam;
	private bool gameEnding = false;
	private bool OnSyncTimeRequest = false;
	private bool IsMasterInitGame = false; 
	private bool OnCheckInitGame = false;
	private int sendTimes = 0;

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
		h2.Add ("isHoldFlag", false);
		PhotonNetwork.player.SetCustomProperties (h2);

		if (isTeamMode) {
			if(PhotonNetwork.player.GetTeam() == PunTeams.Team.blue || PhotonNetwork.player.GetTeam() == PunTeams.Team.none)
				InstantiatePlayer (PlayerPrefab.name, SpawnPoints [0].position, SpawnPoints [0].rotation, 0);
			else{
				InstantiatePlayer (PlayerPrefab.name, SpawnPoints [1].position, SpawnPoints [0].rotation, 0);
			}
		}else{
			InstantiatePlayer (PlayerPrefab.name, SpawnPoints [0].position, SpawnPoints [0].rotation, 0);
		}
	}
		
	IEnumerator LateStart(){
		if(!IsMasterInitGame && sendTimes <10){
				sendTimes++;
				InRoomChat.AddLine ("Sending sync game request..." + sendTimes);
				yield return new WaitForSeconds (0.4f);
				SendSyncInitGameRequest ();
				yield return StartCoroutine (LateStart ());
		}else{
			if(sendTimes >= 10){
				InRoomChat.AddLine ("Failed to sync game properties. Is master disconnected ? ");
				Debug.Log ("master not connected");
			}else{
				InRoomChat.AddLine ("Game is sync.");
			}

			BlueScoreText.text = (PhotonNetwork.room.CustomProperties ["BlueScore"]==null)? "0" : PhotonNetwork.room.CustomProperties ["BlueScore"].ToString();
			RedScoreText.text = (PhotonNetwork.room.CustomProperties ["RedScore"]==null)? "0" : PhotonNetwork.room.CustomProperties ["RedScore"].ToString ();
			bluescore = int.Parse (BlueScoreText.text);
			redscore = int.Parse (RedScoreText.text);

			if (GameInfo.GameMode.Contains ("Capture")) {
				BlueFlag = GameObject.Find ("BlueFlag(Clone)");
				RedFlag = GameObject.Find ("RedFlag(Clone)");

				//when new player joins , put the flag to the right guy
				if (PhotonNetwork.room.CustomProperties ["BlueFlagHolder"] != null) {
					if(int.Parse(PhotonNetwork.room.CustomProperties ["BlueFlagHolder"].ToString()) != -1){
						playerHoldFlag (int.Parse (PhotonNetwork.room.CustomProperties ["BlueFlagHolder"].ToString ()), 0);
					}else{
						BlueFlag.transform.position = StringToVector3 (PhotonNetwork.room.CustomProperties ["BlueFlagPos"].ToString ());
					}
				}
				if (PhotonNetwork.room.CustomProperties ["RedFlagHolder"] != null) {
					if (int.Parse (PhotonNetwork.room.CustomProperties ["RedFlagHolder"].ToString ()) != -1) {
						playerHoldFlag (int.Parse (PhotonNetwork.room.CustomProperties ["RedFlagHolder"].ToString ()), 1);
					}else{
						RedFlag.transform.position = StringToVector3 (PhotonNetwork.room.CustomProperties ["RedFlagPos"].ToString ());
					}
				}
			}
		}
	}

	void SendSyncInitGameRequest(){
		if(bool.Parse(PhotonNetwork.room.CustomProperties["GameInit"].ToString()) ==true){
			IsMasterInitGame = true;
		}
	}

	public void InstantiatePlayer(string name, Vector3 StartPos, Quaternion StartRot, int group){
		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, StartPos, StartRot, 0);

		mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		mechBuilder.Build (m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);

		cam = player.transform.Find("Camera").GetComponent<Camera>();
		hud = GameObject.Find("Canvas").GetComponent<HUD>();
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
		h.Add ("BlueFlagHolder", -1);
		h.Add ("RedFlagHolder", -1);
		h.Add ("BlueFlagPos", new Vector3 (SpawnPoints [0].position.x,0,SpawnPoints [0].position.z));
		h.Add ("RedFlagPos", new Vector3 (SpawnPoints [1].position.x,0,SpawnPoints [1].position.z));

		PhotonNetwork.room.SetCustomProperties (h);
		SyncTime();

		//Instantiate flags
		if(GameInfo.GameMode.Contains("Capture")){
			InstantiateFlags ();
		}

		IsMasterInitGame = true;
	}

	void InstantiateFlags(){
		GameObject BlueFlag = PhotonNetwork.InstantiateSceneObject ("BlueFlag", new Vector3(SpawnPoints [0].position.x , 0 , SpawnPoints [0].position.z), Quaternion.Euler(Vector3.zero), 0, null);
		GameObject RedFlag = PhotonNetwork.InstantiateSceneObject ("RedFlag", new Vector3(SpawnPoints [1].position.x , 0 , SpawnPoints [1].position.z), Quaternion.Euler(Vector3.zero), 0, null);


		BlueFlag.GetComponent<PhotonView> ().TransferOwnership (PhotonNetwork.masterClient);
		RedFlag.GetComponent<PhotonView> ().TransferOwnership (PhotonNetwork.masterClient);
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
		if (!GameOver ()) {
			Cursor.lockState = CursorLockMode.Locked;
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
			gameEnding = true;
			Cursor.lockState = CursorLockMode.None;
			hud.ShowText(cam, cam.transform.position + new Vector3(0,0,0.5f), "GameOver");
			Scoreboard.SetActive(true);
			StartCoroutine(ExecuteAfterTime(3));
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

		//Final stage

		// Code to execute after the delay
		Cursor.visible = true;
		PhotonNetwork.LoadLevel("GameLobby");
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
			if (currentTimer <= 0) {
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

		if(bool.Parse(player.CustomProperties["isHoldFlag"].ToString())){
			print("yeah he hold it.");
		}
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
			if(player.NickName == BlueFlagHolderName){
					BlueFlagHolder = null;
					BlueFlag.transform.parent = null;
					BlueFlag.transform.position = new Vector3 (BlueFlag.transform.position.x, 0, BlueFlag.transform.position.z);
					BlueFlag.transform.rotation = Quaternion.Euler (Vector3.zero);
					BlueFlag.GetComponent<Flag> ().isGrounded = true;

				if (PhotonNetwork.isMasterClient) {
					BlueFlag.GetComponent<PhotonView> ().TransferOwnership (PhotonNetwork.masterClient);

					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("BlueFlagHolder", -1);
					h.Add ("BlueFlagPos", BlueFlag.transform.position);
					PhotonNetwork.room.SetCustomProperties (h);
				}

			}else if(player.NickName == RedFlagHolderName){
					RedFlagHolder = null;
					RedFlag.transform.parent = null;
					RedFlag.transform.position = new Vector3 (RedFlag.transform.position.x, 0, RedFlag.transform.position.z);
					RedFlag.transform.rotation = Quaternion.Euler (Vector3.zero);
					RedFlag.GetComponent<Flag> ().isGrounded = true;

				if (PhotonNetwork.isMasterClient) {
					RedFlag.GetComponent<PhotonView> ().TransferOwnership (PhotonNetwork.masterClient);

					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("RedFlagHolder", -1);
					h.Add ("RedFlagPos", RedFlag.transform.position);
					PhotonNetwork.room.SetCustomProperties (h);
				}

			}
		}
	}


	[PunRPC]
	void GetFlagRequest(int player_viewID, int flag){//flag 0 : blue team ;  1 : red
		//this is always received by master

		PhotonPlayer player = PhotonView.Find (player_viewID).owner;
		//check the current holder
		if(flag == 0){
			if(BlueFlagHolder!=null&&bool.Parse(BlueFlagHolder.CustomProperties["isHoldFlag"].ToString())){
				return;
			}else{
				//RPC all to give this flag to the player
				photonView.RPC("playerHoldFlag",PhotonTargets.All, player_viewID, flag);
				Debug.Log (player + " has the blue flag.");

				ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
				h.Add ("isHoldFlag", true);
				player.SetCustomProperties (h);
			}
		}else{
			if(RedFlagHolder!=null&&bool.Parse(RedFlagHolder.CustomProperties["isHoldFlag"].ToString())){
				return;
			}else{
				//RPC all to give this flag to the player
				photonView.RPC("playerHoldFlag",PhotonTargets.All, player_viewID, flag);
				Debug.Log (player + " has the red flag.");

				ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
				h.Add ("isHoldFlag", true);
				player.SetCustomProperties (h);
			}
		}
	}

	[PunRPC]
	void playerHoldFlag(int player_viewID, int flag){
		if(player_viewID == -1){//put the flag to the base
			if(flag == 0){
				if(PhotonNetwork.isMasterClient){
					ExitGames.Client.Photon.Hashtable h;
					if(BlueFlagHolder!=null){
					h = new ExitGames.Client.Photon.Hashtable () ;
					h.Add ("isHoldFlag", false);
					BlueFlagHolder.SetCustomProperties (h);
					}

					h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("BlueFlagHolder", -1);
					h.Add ("BlueFlagPos", new Vector3 (SpawnPoints [0].position.x, 0, SpawnPoints [0].position.z));
					PhotonNetwork.room.SetCustomProperties (h);
				}
				BlueFlag.transform.parent = null;
				BlueFlag.transform.position = new Vector3 (SpawnPoints [0].position.x, 0, SpawnPoints [0].position.z);
				BlueFlag.transform.rotation = Quaternion.Euler (Vector3.zero);
				BlueFlag.GetComponent<Flag> ().isGrounded = true;
				BlueFlagHolder = null;
				BlueFlagHolderName = "";
				BlueFlag.GetComponent<Flag> ().isOnBase = true;
			}else{
				if(PhotonNetwork.isMasterClient){
					ExitGames.Client.Photon.Hashtable h;

					if(RedFlagHolder!=null){
					h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("isHoldFlag", false);
					RedFlagHolder.SetCustomProperties (h);
					}

					h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("RedFlagHolder", -1);
					h.Add ("RedFlagPos", new Vector3 (SpawnPoints [1].position.x, 0, SpawnPoints [1].position.z));
					PhotonNetwork.room.SetCustomProperties (h);
				}
				RedFlag.transform.parent = null;
				RedFlag.transform.position = new Vector3 (SpawnPoints [1].position.x, 0, SpawnPoints [1].position.z);
				RedFlag.transform.rotation = Quaternion.Euler (Vector3.zero);
				RedFlag.GetComponent<Flag> ().isGrounded = true;
				RedFlagHolder = null;
				RedFlagHolderName = "";
				RedFlag.GetComponent<Flag> ().isOnBase = true;
			}
		
		}else{
			PhotonView pv = PhotonView.Find (player_viewID);
			if(flag == 0){
				BlueFlag.GetComponent<Flag> ().isGrounded = false;
				BlueFlag.transform.SetParent (pv.transform.Find ("CurrentMech/metarig/hips/spine/chest/neck"));
				BlueFlag.transform.localPosition = Vector3.zero;
				BlueFlag.transform.localRotation = Quaternion.Euler (new Vector3(-30,0,0));
				BlueFlagHolder = pv.owner;
				BlueFlagHolderName = pv.owner.NickName;
				BlueFlag.GetComponent<Flag> ().isOnBase = false;

				if (PhotonNetwork.isMasterClient) {
					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("BlueFlagHolder", pv.viewID);
					PhotonNetwork.room.SetCustomProperties (h);
				}
			}else{
				RedFlag.GetComponent<Flag> ().isGrounded = false;
				RedFlag.transform.SetParent (pv.transform.Find ("CurrentMech/metarig/hips/spine/chest/neck"));
				RedFlag.transform.localPosition = Vector3.zero;
				RedFlag.transform.localRotation = Quaternion.Euler (new Vector3(-30,0,0));
				RedFlagHolder = pv.owner;
				RedFlagHolderName = pv.owner.NickName;
				RedFlag.GetComponent<Flag> ().isOnBase = false;

				if (PhotonNetwork.isMasterClient) {
					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("RedFlagHolder", pv.viewID);
					PhotonNetwork.room.SetCustomProperties (h);
				}
			}

		}
	}
	[PunRPC]
	void DropFlag(int player_viewID, int flag, Vector3 pos){

		if (PhotonNetwork.isMasterClient) {
			PhotonView pv = PhotonView.Find (player_viewID);

			ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
			h.Add ("isHoldFlag", false);
			pv.owner.SetCustomProperties (h);

			h = new ExitGames.Client.Photon.Hashtable ();
			h.Add ((flag == 0) ? "BlueFlagHolder" : "RedFlagHolder", -1);
			if (flag == 0) {
				h.Add ("BlueFlagPos", new Vector3 (SpawnPoints [0].position.x, 0, SpawnPoints [0].position.z));
			}else{
				h.Add ("RedFlagPos", new Vector3 (SpawnPoints [1].position.x, 0, SpawnPoints [1].position.z));
			}
			PhotonNetwork.room.SetCustomProperties (h);
		}

		if(flag == 0){
			BlueFlag.transform.parent = null;
			BlueFlag.transform.position = pos;
			BlueFlag.transform.rotation = Quaternion.Euler (Vector3.zero);
			BlueFlagHolder = null;
			BlueFlagHolderName = "";
			BlueFlag.GetComponent<Flag> ().isGrounded = true;

			//when disabling player , flag's renderer gets turn off
			Renderer[] renderers = BlueFlag.GetComponentsInChildren<Renderer> ();
			foreach(Renderer renderer in renderers){
				renderer.enabled = true;
			}
		}else{
			RedFlag.transform.parent = null;
			RedFlag.transform.position = pos;
			RedFlag.transform.rotation = Quaternion.Euler (Vector3.zero);
			RedFlagHolder = null;
			RedFlagHolderName = "";
			RedFlag.GetComponent<Flag> ().isGrounded = true;

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
			if ( (BlueFlagHolder != null && bool.Parse (BlueFlagHolder.CustomProperties ["isHoldFlag"].ToString ())) || !bool.Parse (RedFlagHolder.CustomProperties ["isHoldFlag"].ToString ())) {
				return;
			}else{
				photonView.RPC ("RegisterScore", PhotonTargets.All, player_viewID);

				//send back the flag
				photonView.RPC ("playerHoldFlag", PhotonTargets.All, -1, 1);
			}
		}else{//Redteam : blue flag holder
			if (!bool.Parse (BlueFlagHolder.CustomProperties ["isHoldFlag"].ToString ()) || (RedFlagHolder != null && bool.Parse (RedFlagHolder.CustomProperties ["isHoldFlag"].ToString ()))) {
				return;
			}else{
				photonView.RPC ("RegisterScore", PhotonTargets.All, player_viewID);
				photonView.RPC ("playerHoldFlag", PhotonTargets.All, -1, 0);
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
	public static Vector3 StringToVector3(string sVector){
		if(sVector.StartsWith("(") && sVector.EndsWith(")")){
			sVector = sVector.Substring(1,sVector.Length-2);
		}

		string[] sArray = sVector.Split(',');

		Vector3 result = new Vector3(float.Parse(sArray[0]),float.Parse(sArray[1]),float.Parse(sArray[2]));

		return result;
	}
}
