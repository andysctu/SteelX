using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerCustom : NetworkManager {
	/*public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
//		if (playerPrefab == null)
//		{
//			if (LogFilter.logError) { Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object."); }
//			return;
//		}
//
//		if (playerPrefab.GetComponent<NetworkIdentity>() == null)
//		{
//			if (LogFilter.logError) { Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab."); }
//			return;
//		}

		if (playerControllerId < conn.playerControllers.Count  && conn.playerControllers[playerControllerId].IsValid && conn.playerControllers[playerControllerId].gameObject != null)
		{
			if (LogFilter.logError) { Debug.LogError("There is already a player at that playerControllerId for this connections."); }
			return;
		}
			
		Mech mech = UserData.data.Mech;
		List<string> parts = new List<string>();
		parts.Add(mech.Arms);
		parts.Add(mech.Legs);
		parts.Add(mech.Head);
		MechCreator mc = new MechCreator(mech.Core, parts);

		GameObject player;
		GameObject model;
		Transform startPos = GetStartPosition();
		model = mc.CreatePlayerMech();

		if (startPos != null)
		{
			
			player = (GameObject)Instantiate(playerPrefab, startPos.position, startPos.rotation);
			Debug.Log(player.transform.rotation.x + ", " + player.transform.rotation.y + ", " + player.transform.rotation.z);
		}
		else
		{
			player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		}

		model.transform.parent = player.transform;
		player.GetComponent<NetworkAnimator>().animator = model.GetComponent<Animator>();

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);//, NetworkHash128.Parse("This is a text string string"));

	}*/

//	public GameObject SpawnPlayerObject(Vector3 position, NetworkHash128 hash){
//		Mech mech = UserData.data.Mech;
//		List<string> parts = new List<string>();
//		parts.Add(mech.Arms);
//		parts.Add(mech.Legs);
//		parts.Add(mech.Head);
//		MechCreator mc = new MechCreator(mech.Core, parts);
//
//		GameObject player;
//		return mc.CreatePlayerMech();
//	}
//
//	public void UnspawnPlayerObject(GameObject unspawnObject)
//	{
//		Debug.Log("Custom player unspawn!");
//		Destroy(unspawnObject);
//	}
//
//	public override void OnClientConnect(NetworkConnection conn)
//	{
//		Debug.Log("New player connected!");
//		// ClientScene.RegisterPrefab(testPrefab);
//		ClientScene.RegisterSpawnHandler(NetworkHash128.Parse("This is a text string string"), SpawnPlayerObject, UnspawnPlayerObject);
//		base.OnClientConnect(conn);
//	}

//	public override void OnStartClient(NetworkClient client){
//		if (playerPrefab == null) {
//			playerPrefab = GameObject.Find("Lobby Main Camera").GetComponent<MechCreator>().mechToSpawn;
//		}
//	}
//	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
//	{
//		Debug.Log("HI");
//		OnServerAddPlayer(conn, playerControllerId, extraMessageReader);
//	}


//	public override void OnServerConnect(NetworkConnection conn){
//		Debug.Log("PlayerConnected");
//
//	}

//	private bool first = true;
//	public void StartupHost(){
//		SetPort ();
//		NetworkManager.singleton.StartHost ();
//	}
//
//	public void JoinGame(){
//		SetIPAddress ();
//		SetPort ();
//		NetworkManager.singleton.StartClient ();
//	}
//
//	void SetPort(){
//		NetworkManager.singleton.networkPort = 7777;
//	}
//
//	void SetIPAddress(){
//		string ipAddress = GameObject.Find ("InputFieldIPAddress").transform.FindChild ("Text").GetComponent<Text> ().text;
//		NetworkManager.singleton.networkAddress = ipAddress;
//	}
//
//	void OnLevelWasLoaded(int level){
//		if (level == 1 && !first)  { // Home
////			Debug.Log ("lvl 1");
//			StartCoroutine(SetupMenuSceneButtons());
//		} else if (level == 3) { // Game
////			Debug.Log ("lvl 2");
//			SetupOtherSceneButtons ();
//		}
//		first = false;
//	}
//
//	IEnumerator SetupMenuSceneButtons(){
//		yield return new WaitForSeconds (0.2f);
//		GameObject.Find ("ButtonStartHost").GetComponent<Button> ().onClick.RemoveAllListeners ();
//		GameObject.Find ("ButtonStartHost").GetComponent<Button> ().onClick.AddListener(StartupHost);
//
//		GameObject.Find ("ButtonJoinGame").GetComponent<Button> ().onClick.RemoveAllListeners ();
//		GameObject.Find ("ButtonJoinGame").GetComponent<Button> ().onClick.AddListener(JoinGame);
//	}
//
//	void SetupOtherSceneButtons(){
//		GameObject.Find ("ButtonDisconnect").GetComponent<Button> ().onClick.RemoveAllListeners ();
//		GameObject.Find ("ButtonDisconnect").GetComponent<Button> ().onClick.AddListener(NetworkManager.singleton.StopHost);
//	}
}
