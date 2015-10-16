using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

[NetworkSettings (channel = 0, sendInterval = 0.1f)]
public class PlayerSyncPosition : NetworkBehaviour {

	public float x = 10f;
	public float y = 2f;

	[SyncVar (hook = "SyncPositionValues")] private Vector3 syncPos;

	private Vector3 prevPos;
	private float threshold = 0.5f;

	[SerializeField] Transform myTransform;
	[SerializeField] bool useHistoricalLerping = false;
	private float lerpRate;
	private float normalLerpRate = 20f;
	private float fasterLerpRate = 30f;

	private NetworkClient nClient;
	private int latency;
	private Text latencyText;
	private float closeEnough = 0.1f;

	private List<Vector3> syncPosList = new List<Vector3>();
	void Start(){
		nClient = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().client;
		latencyText = GameObject.Find ("Latency Text").GetComponent<Text> ();
		lerpRate = normalLerpRate;
	}

	void Update(){
		LerpPosition ();  
		ShowLatency ();
	}

	void FixedUpdate(){
		TransmitPosition ();
	}
	
	void LerpPosition(){
		if (!isLocalPlayer) {
			if (useHistoricalLerping) {
				HistoricalLerping ();
			} else {
				NormalLerping ();
			}
		}
	}

	[Client]
	void SyncPositionValues(Vector3 latestPos){
		syncPos = latestPos;
		syncPosList.Add (syncPos); 
	}

	void HistoricalLerping(){
		if (syncPosList.Count > 0) {
			myTransform.position = Vector3.Lerp (myTransform.position, syncPosList [0], Time.deltaTime * lerpRate);

			if (Vector3.Distance (myTransform.position, syncPosList [0]) < closeEnough) {
				syncPosList.RemoveAt (0);
			}

			//lerpRate = x + (syncPosList.Count * y);
			if (syncPosList.Count > 10) {
				lerpRate = fasterLerpRate;
			} else {
				lerpRate = normalLerpRate;
			}

			//Debug.Log (syncPosList.Count.ToString ());
		}
	}

	void NormalLerping(){
		myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate);
	}

	[Command]
	void CmdProvidePositionToServer(Vector3 pos){
		syncPos = pos;
		//Debug.Log ("c");
	}

	[Client]
	void TransmitPosition(){
		if (isLocalPlayer && Vector3.Distance(myTransform.position, prevPos) > threshold) {
			CmdProvidePositionToServer (myTransform.position);
			prevPos = myTransform.position;
		}
	}

	void ShowLatency(){
		if (isLocalPlayer) {
			latency = nClient.GetRTT ();
			latencyText.text  = latency.ToString();
		}
	}
}
