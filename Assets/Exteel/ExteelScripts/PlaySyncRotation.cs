using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class PlaySyncRotation : NetworkBehaviour {

	[SyncVar (hook = "OnPlayerRotSynced")] private float syncPlayerRotation;
	[SyncVar (hook = "OnCamRotSynced")] private float syncCamRotation;

	//[SyncVar (hook = "OnCamGlobalRotSynced")] private float syncCamGlobalRotation;

	[SerializeField] private Transform playerTransform;
	[SerializeField] private Transform camTransform;
	private float lerpRate = 20;

	private float prevPlayerRot;
	private float prevCamRot;
	//private float prevCamGlobalRot;

	private float threshold = 1f;

	private List<float> syncPlayerRotList = new List<float> ();
	private List<float> syncCamRotList = new List<float>();
	//private List<float> syncCamGlobalRotList = new List<float> ();
	private float closeEnough = 0.4f;
	[SerializeField] private bool useHistoricalInterpolation;
	void Update (){
		LerpRotations ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		TransmitRotations ();
	}

	void LerpRotations(){
		if (!isLocalPlayer) {
			//playerTransform.rotation = Quaternion.Lerp (playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);
			//camTransform.rotation = Quaternion.Lerp (camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);

			if (useHistoricalInterpolation){
				HistoricalInterpolation ();
			} else {
				NormalLerping ();
			}
		}
	}

	void NormalLerping(){
		LerpPlayerRotation (syncPlayerRotation);
		LerpCamRotation (syncCamRotation);
		//LerpCamGlobalRotation(syncCamGlobalRotation);

	}

	void LerpPlayerRotation(float rotAngle){
		Vector3 playerNewRot = new Vector3 (0, rotAngle, 0);
		playerTransform.rotation = Quaternion.Lerp (playerTransform.rotation, Quaternion.Euler (playerNewRot), Time.deltaTime * lerpRate);
	}

	void LerpCamRotation(float rotAngle){
		Vector3 camNewRot = new Vector3 (rotAngle, 0, 0);
		camTransform.localRotation = Quaternion.Lerp (camTransform.rotation, Quaternion.Euler (camNewRot), Time.deltaTime * lerpRate);
	}

//	void LerpCamGlobalRotation(float globalRotAngle){
//		Vector3 camNewGlobalRot = new Vector3 (globalRotAngle, 0, 0);
//		camTransform.rotation = Quaternion.Lerp (camTransform.rotation, Quaternion.Euler (camNewGlobalRot), Time.deltaTime * lerpRate);
//	}

	void HistoricalInterpolation(){
		if (syncPlayerRotList.Count > 0) {
			LerpPlayerRotation (syncPlayerRotList[0]);
			if (Mathf.Abs (playerTransform.localEulerAngles.y - syncPlayerRotList[0]) < closeEnough){
				syncPlayerRotList.RemoveAt (0);
			}
			//Debug.Log (syncPlayerRotList.Count.ToString () + "sync player rot list count");
		}

		if (syncCamRotList.Count > 0) {
			LerpCamRotation(syncCamRotList[0]);
			if (Mathf.Abs (camTransform.localEulerAngles.x - syncCamRotList[0]) < closeEnough){
				syncCamRotList.RemoveAt(0);
			}
			//Debug.Log (syncCamRotList.Count.ToString () + "sync cam rot list count");
		}
//
//		if (syncCamGlobalRotList.Count > 0) {
//			LerpCamGlobalRotation(syncCamGlobalRotList[0]);
//			if (Mathf.Abs (camTransform.eulerAngles.x - syncCamGlobalRotList[0]) < closeEnough){
//				syncCamGlobalRotList.RemoveAt(0);
//			}
//			Debug.Log (syncCamGlobalRotList.Count.ToString () + "sync cam global rot list count");
//		}
	}
//	[Command]
//	void CmdProvideRotationsToServer(Quaternion playerRot, Quaternion camRot){
//		syncPlayerRotation = playerRot;
//		syncCamRotation = camRot;
//		//Debug.Log ("Angle C");
//	}

	[Command]
	void CmdProvideRotationsToServer(float playerRot, float camRot){//, float camGlobalRot){
		syncPlayerRotation = playerRot;
		syncCamRotation = camRot;
		//syncCamGlobalRotation = camGlobalRot;
		//Debug.Log ("Angle C");
	}

	[Client]
	void TransmitRotations(){
		if (isLocalPlayer && 
				    (CheckIfBeyondThreshold(playerTransform.localEulerAngles.y, prevPlayerRot) 
				 || CheckIfBeyondThreshold(camTransform.localEulerAngles.x, prevCamRot) 
				 //|| CheckIfBeyondThreshold(camTransform.eulerAngles.x, prevCamGlobalRot)
				 )
			) {

					//(Quaternion.Angle(playerTransform.rotation, prevPlayerRot) > threshold || Quaternion.Angle (camTransform.rotation, prevCamRot) > threshold)) {
//			CmdProvideRotationsToServer(playerTransform.rotation, camTransform.rotation);
//			prevPlayerRot = playerTransform.rotation;
//			prevCamRot = camTransform.rotation;

			prevPlayerRot = playerTransform.localEulerAngles.y;
			prevCamRot = camTransform.localEulerAngles.x;
			//prevCamGlobalRot = camTransform.eulerAngles.x;
			CmdProvideRotationsToServer(prevPlayerRot, prevCamRot);//, prevCamGlobalRot);
		}
	}

	[Client]
	void OnPlayerRotSynced(float latestPlayerRot){
		syncPlayerRotation = latestPlayerRot;
		syncPlayerRotList.Add (syncPlayerRotation);
	}

	[Client]
	void OnCamRotSynced(float latestCamRot){
		syncCamRotation = latestCamRot;
		syncCamRotList.Add (syncCamRotation);
	}

//	[Client]
//	void OnCamGlobalRotSynced(float latestCamGlobalRot){
//		syncCamGlobalRotation = latestCamGlobalRot;
//		syncCamGlobalRotList.Add (syncCamGlobalRotation);
//	}

	bool CheckIfBeyondThreshold(float rot1, float rot2){
		return Mathf.Abs (rot1-rot2) > threshold;
	}
}
