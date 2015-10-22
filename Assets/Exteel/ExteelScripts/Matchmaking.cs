﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class Matchmaking : MonoBehaviour
{
	List<MatchDesc> matchList = new List<MatchDesc>();
	bool matchCreated;
	NetworkMatch networkMatch;
	
	void Awake()
	{
		networkMatch = gameObject.AddComponent<NetworkMatch>();
	}
	
	void OnGUI()
	{
		// You would normally not join a match you created yourself but this is possible here for demonstration purposes.
		if(GUILayout.Button("Create Room"))
		{
			CreateMatchRequest create = new CreateMatchRequest();
			create.name = "NewRoom";
			create.size = 4;
			create.advertise = true;
			create.password = "";

			Debug.Log ("Creating room ... ");
			networkMatch.CreateMatch(create, OnMatchCreate);
		}
		
		if (GUILayout.Button("List rooms"))
		{
			Debug.Log ("Listing rooms ... ");
			networkMatch.ListMatches(0, 20, "", OnMatchList);
		}
		
		if (matchList.Count > 0)
		{
			Debug.Log ("Count > 0");
			GUILayout.Label("Current rooms");
		}

		int i = 0;
		foreach (var match in matchList)
		{
			Debug.Log ("match " + i);
			if (GUILayout.Button(match.name))
			{	
				Debug.Log ("match " + i + " clicked");
				networkMatch.JoinMatch(match.networkId, "", OnMatchJoined);
			}
		}
	}
	
	public void OnMatchCreate(CreateMatchResponse matchResponse)
	{
		if (matchResponse.success)
		{
			Debug.Log("Create match succeeded");
			matchCreated = true;
			Utility.SetAccessTokenForNetwork(matchResponse.networkId, new NetworkAccessToken(matchResponse.accessTokenString));
			NetworkServer.Listen(new MatchInfo(matchResponse), 9000);
		}
		else
		{
			Debug.LogError ("Create match failed");
		}
	}
	
	public void OnMatchList(ListMatchResponse matchListResponse)
	{	
		matchList = matchListResponse.matches;
		Debug.Log ("OnMatchList");
		if (matchListResponse.success && matchListResponse.matches != null)
		{
			Debug.Log ("success && matches != null");
			//networkMatch.JoinMatch(matchListResponse.matches[0].networkId, "", OnMatchJoined);
		}
	}
	
	public void OnMatchJoined(JoinMatchResponse matchJoin)
	{
		if (matchJoin.success)
		{
			Debug.Log("Join match succeeded");
			if (matchCreated)
			{
				Debug.LogWarning("Match already set up, aborting...");
				return;
			}
			Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessTokenString));
			NetworkClient myClient = new NetworkClient();
			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			myClient.Connect(new MatchInfo(matchJoin));
		}
		else
		{
			Debug.LogError("Join match failed");
		}
	}
	
	public void OnConnected(NetworkMessage msg)
	{
		Debug.Log("Connected!");
	}
}