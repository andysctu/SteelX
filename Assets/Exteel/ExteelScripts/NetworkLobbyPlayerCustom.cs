using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkLobbyPlayerCustom : NetworkLobbyPlayer {
//	public override void OnClientEnterLobby ()
//	{	
//		name = UserData.myData.User.PilotName;
//		base.OnClientEnterLobby ();
//	}
	void OnGUI()
	{
//		if (!ShowLobbyGUI)
//			return;
//
//		var lobby = NetworkManager.singleton as NetworkLobbyManager;
//		if (lobby)
//		{
//			if (!lobby.showLobbyGUI)
//				return;
//
//			string loadedSceneName = SceneManager.GetSceneAt(0).name;
//			if (loadedSceneName != lobby.lobbyScene)
//				return;
//		}
//
		Rect rec = new Rect(100 + slot * 100, 200, 90, 20);
//
		if (isLocalPlayer)
		{
			string youStr;
			if (readyToBegin)
			{
				youStr = "(1Ready)";
			}
			else
			{
				youStr = "(1Not Ready)";
			}
			GUI.Label(rec, youStr);

			if (readyToBegin)
			{
				rec.y += 25;
				if (GUI.Button(rec, "1STOP"))
				{
					SendNotReadyToBeginMessage();
				}
			}
			else
			{
				rec.y += 25;
				if (GUI.Button(rec, "1START"))
				{
					SendReadyToBeginMessage();
				}

				rec.y += 25;
				if (GUI.Button(rec, "1Remove"))
				{
					ClientScene.RemovePlayer(GetComponent<NetworkIdentity>().playerControllerId);
				}
			}
		}
		else
		{
			GUI.Label(rec, "1Player [" + netId + "]");
			rec.y += 25;
			GUI.Label(rec, "1Ready [" + readyToBegin + "]");
		}
	}
}
