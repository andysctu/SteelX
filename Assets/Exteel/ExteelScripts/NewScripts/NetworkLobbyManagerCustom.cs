using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;


public class NetworkLobbyManagerCustom : NetworkLobbyManager {

//	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
//		MechCreator mc = new MechCreator("", null);
//		GameObject core = mc.CreatePlayerMech();
//		NetworkServer.Spawn(core);
//		core.transform.parent = gamePlayer.transform;
//		return true;
//	}
//	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
//	{
//		if (Application.loadedLevelName != lobbyScene)
//		{
//			return;
//		}
//
//		// check MaxPlayersPerConnection
//		int numPlayersForConnection = 0;
//		foreach (var player in conn.playerControllers)
//		{
//			if (player.IsValid)
//				numPlayersForConnection += 1;
//		}
//
//		if (numPlayersForConnection >= maxPlayersPerConnection)
//		{
//			if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager no more players for this connection."); }
//
//			var errorMsg = new EmptyMessage();
//			conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
//			return;
//		}
//
//		byte slot = FindSlot();
//		if (slot == Byte.MaxValue)
//		{
//			if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager no space for more players"); }
//
//			var errorMsg = new EmptyMessage();
//			conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
//			return;
//		}
//
//		var newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
//		if (newLobbyGameObject == null)
//		{
//			newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
//		}
//
////		MechCreator mc = new MechCreator("", null);
////
////		GameObject core = mc.CreateLobbyMech();
////		core.transform.parent = newLobbyGameObject.transform;
//
//		var newLobbyPlayer = newLobbyGameObject.GetComponent<NetworkLobbyPlayer>();
//		newLobbyPlayer.slot = slot;
//		lobbySlots[slot] = newLobbyPlayer;
//
//		NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject, playerControllerId);
//	}

//	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
//	{
//		GameObject gamePlayer = SpawnPlayerObject(Vector3.zero, NetworkHash128.Parse("AndyTu"));
//		// get start position from base class
////		Transform startPos = GetStartPosition();
////		if (startPos != null)
////		{
////			gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
////		}
////		else
////		{
////			gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
////		
//		
//		Debug.Log("Here");
////		if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
////		{
////			return;
////		}
//
//		// replace lobby player with game player
//		NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, playerControllerId, NetworkHash128.Parse("AndyTu"));
//		return gamePlayer;
//	}
//
//	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
//		return false;
//	}
//
	public override void OnClientConnect(NetworkConnection conn)
	{
		Debug.Log("New player connected!");

//		UserData userData = GameObject.Find("UserData").GetComponent<UserData>() as UserData;
//		if (userData == null) Debug.Log("userdata obj null");
//		else {
//			Debug.Log("not null");
//			Data myD = UserData.myData;
////			if () Debug.Log("myd null");
////			else {
//			Debug.Log("myd not null");
//			userData.UpdatePlayerDict(conn.connectionId, UserData.myData);
//
//				Debug.Log(myD.User.PilotName);
////			}
//		}
//		ClientScene.RegisterSpawnHandler(NetworkHash128.Parse("AndyTu"), SpawnPlayerObject, UnspawnPlayerObject);
		base.OnClientConnect(conn);
		return;
	}
		
//
//	public GameObject SpawnPlayerObject(Vector3 position, NetworkHash128 hash)
//	{   
//		Debug.Log("Custom new player spawn!");
//
//		Data data = UserData.data;
//		string core = data.Mech.Core;
//
//		List<String> parts = new List<String>();
//		parts.Add(data.Mech.Arms);
//		parts.Add(data.Mech.Legs);
//		parts.Add(data.Mech.Head);
//
//		MechCreator mc = new MechCreator(core, parts);
//
//		GameObject gamePlayer = mc.CreatePlayerMech();
//		return gamePlayer;
//	}
//
//	public void UnspawnPlayerObject(GameObject unspawnObject)
//	{
//		Debug.Log("Custom player unspawn!");
//		Destroy(unspawnObject);
//	}
}
