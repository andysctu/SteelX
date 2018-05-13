using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crosshair : MonoBehaviour {
	[SerializeField]private BuildMech bm;
	[SerializeField]private PhotonView pv;
	[SerializeField]private GameObject LockedImg;
	[SerializeField]private CrosshairImage crosshairImage;
	[SerializeField]private LayerMask playerlayer,Terrainlayer;
	[SerializeField]private Sounds Sounds;
	[SerializeField]private MechCombat mcbt;
	private List<GameObject> TargetsToRemove = new List<GameObject> ();
	public List<GameObject> Targets = new List<GameObject> ();//control by checkisrendered.cs

	private Weapon[] weaponScripts;
	private Transform targetL,targetR;
	private Camera cam;
	private RaycastHit hit;
	private Coroutine coroutine = null;
	private Vector2 CamMidpoint = new Vector2 (0.5f, 0.5f);

	private const float SendMsgDeltaTime = 0.3f; //If the target is the same, this is the time between two msgs.
	private float screenCoeff;
	private float TimeOfLastSend;
	private float CrosshairRadiusL ;
	private float CrosshairRadiusR ;
	private int LastLockTargetID = 0 ;//avoid sending lock message too often
	private bool LockL = false, LockR = false , foundTargetL=false, foundTargetR=false;
	private bool isOnLocked = false;
	private bool isTeamMode;
	private bool isTargetAllyL = false, isTargetAllyR = false;
	private bool isRCL = false, isENG_L = false, isENG_R = false;
	private const float LockedMsgDuration = 0.5f;//when receiving a lock message , the time it'll last
	public const float CAM_DISTANCE_TO_MECH = 15f;

	private float SphereRadiusCoeff = 0.04f;
	private float DistanceCoeff = 0.008f;

	public float MaxDistanceL ;
	public float MaxDistanceR ;


	void Start () {
		GetGameVars ();
		initComponent ();
		UpdateCrosshair ();
	}

	void GetGameVars(){
		screenCoeff = (float)Screen.height / Screen.width;
		isTeamMode = GameManager.isTeamMode;
	}

	void initComponent(){
		weaponScripts = bm.weaponScripts;
		cam = GetComponent<Camera> ();
	}
		
	public void NoAllCrosshairs() {//called when disabling player
		if (crosshairImage != null) {
			crosshairImage.CloseAllCrosshairs_L ();
			crosshairImage.CloseAllCrosshairs_R ();

			targetL = null;	
			targetR = null;

			crosshairImage.targetMark.gameObject.SetActive (false);
			crosshairImage.middlecross.enabled = false;
			crosshairImage.EngTargetMark.enabled = false;
		}
	}
	public void UpdateCrosshair(){
		weaponScripts = bm.weaponScripts;


		CrosshairRadiusL = (weaponScripts[mcbt.weaponOffset] == null)? 0 : weaponScripts [mcbt.weaponOffset].radius;
		CrosshairRadiusR = (weaponScripts[mcbt.weaponOffset + 1] == null)? 0 : weaponScripts [mcbt.weaponOffset+1].radius;
		MaxDistanceL = (weaponScripts[mcbt.weaponOffset] == null) ? 0 : weaponScripts[mcbt.weaponOffset].Range;
		MaxDistanceR = (weaponScripts[mcbt.weaponOffset + 1] == null) ? 0 : weaponScripts [mcbt.weaponOffset+1].Range;

		//isENG_L = (weaponScripts [mcbt.weaponOffset].Animation == "ENGShoot");
		//isENG_R = (weaponScripts [mcbt.weaponOffset + 1].Animation == "ENGShoot");
		//isRCL = (weaponScripts [mcbt.weaponOffset].Animation == "RCLShoot");

		isTargetAllyL = isENG_L;
		isTargetAllyR = isENG_R;

		crosshairImage.SetRadius (CrosshairRadiusL,CrosshairRadiusR);

		//first turn all off
		crosshairImage.CloseAllCrosshairs_L ();

		if(CrosshairRadiusL != 0){
			if(isRCL){
				crosshairImage.SetCurrentLImage ((int)Ctype.RCL_0);
			}else if(!isENG_L){//ENG does not have crosshair
				crosshairImage.SetCurrentLImage ((int)Ctype.N_L0);
			}else{
				crosshairImage.SetCurrentLImage ((int)Ctype.ENG);
			}
		}

		crosshairImage.CloseAllCrosshairs_R ();

		if (CrosshairRadiusR != 0) {
			if(!isENG_R){
				crosshairImage.SetCurrentRImage ((int)Ctype.N_R0);
			}else{
				crosshairImage.SetCurrentRImage ((int)Ctype.ENG);
			}		
		}

		targetL = null;
		targetR = null; 

		//enable middle cross
		if (isENG_L && isENG_R)
			crosshairImage.middlecross.enabled = false;
		else 
			crosshairImage.middlecross.enabled = true;
		
		crosshairImage.targetMark.gameObject.SetActive(false); //targetMark has a children
		crosshairImage.EngTargetMark.enabled = false;
	}

	void Update () {
		if (CrosshairRadiusL > 0) {
			foreach(GameObject target in Targets){
				if (target == null) {//if target is disconnected => target is null
					TargetsToRemove.Add (target);
					continue;
				}

				PhotonView targetpv = target.GetComponent<PhotonView> ();
				if (targetpv.viewID == pv.viewID)
					continue;

				if(isTeamMode){
					if (target.GetComponent<Collider>().tag == "Drone"){
						continue;
					}
					if (!isTargetAllyL) {
						if (targetpv.owner.GetTeam () == pv.owner.GetTeam ()) {
							continue;
						}
					}else{
						if (targetpv.owner.GetTeam () != pv.owner.GetTeam ()) {
							continue;
						}
					}
				}else{
					//if not team mode , ignore eng
					if (isTargetAllyL)
						continue;
				}
				//check distance
				if (!(Vector3.Distance (target.transform.position, transform.root.position) < MaxDistanceL))
					continue;
				
				Vector3 targetLocInCam = cam.WorldToViewportPoint (target.transform.position + new Vector3 (0, 5, 0));
				Vector3 rayStartPoint = transform.root.position+new Vector3(0,10,0); //rayStartpoint should not inside terrain => not detect
				Vector2 targetLocOnScreen = new Vector2 (targetLocInCam.x, (targetLocInCam.y - 0.5f) * screenCoeff + 0.5f);
				if(Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * CrosshairRadiusL && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * CrosshairRadiusL){
					//check if Terrain block the way
					RaycastHit hit;
					if (Physics.Raycast (rayStartPoint,(target.transform.position + new Vector3(0,5,0)- rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, target.transform.position + new Vector3(0,5,0)), Terrainlayer)) {
						if(hit.collider.gameObject.layer == 10){
							continue;
						}
					}
					crosshairImage.OnTargetL(true);
					targetL = target.transform;

					if (!LockL) {
						Sounds.PlayLock ();
						LockL = true;
					}
					foundTargetL = true;
					if(!isTargetAllyL)
						SendLockedMessage (targetpv.viewID, target.name);

					break;
				} 
			}
			if (!foundTargetL) {
				crosshairImage.OnTargetL(false);		
				targetL = null;
				LockL = false;
			}else{
				foundTargetL = false;
			}
		}
		if (CrosshairRadiusR > 0) {
			foreach (GameObject target in Targets) {
				if (target == null) {
					TargetsToRemove.Add (target);
					continue;
				}
					
				PhotonView targetpv = target.transform.GetComponent<PhotonView> ();
				if (targetpv.viewID == pv.viewID)
					continue;

				if(isTeamMode){
					if (target.GetComponent<Collider>().tag == "Drone"){
						continue;
					}
					if (!isTargetAllyR) {
						if (targetpv.owner.GetTeam () == pv.owner.GetTeam ()) {
							continue;
						}
					}else{
						if (targetpv.owner.GetTeam () != pv.owner.GetTeam ()) {
							continue;
						}
					}
				}else{
					//if not team mode , ignore eng
					if (isTargetAllyR)
						continue;
				}
				//check distance
				if (!(Vector3.Distance (target.transform.position, transform.root.position) < MaxDistanceR))
					continue;
				
				Vector3 targetLocInCam = cam.WorldToViewportPoint (target.transform.position + new Vector3 (0, 5, 0));
				Vector3 rayStartPoint = transform.root.position+new Vector3(0,10,0);;
				Vector2 targetLocOnScreen = new Vector2 (targetLocInCam.x, (targetLocInCam.y - 0.5f) * screenCoeff + 0.5f);

				if(Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * CrosshairRadiusR && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * CrosshairRadiusR){
					RaycastHit hit;
					if (Physics.Raycast (rayStartPoint,(target.transform.position + new Vector3(0,5,0)- rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, target.transform.position + new Vector3(0,5,0)), Terrainlayer)) {
						if(hit.collider.gameObject.layer == 10){
							continue;
						}
					}
					crosshairImage.OnTargetR(true);
					targetR = target.transform;

					if (!LockR) {
						Sounds.PlayLock ();
						LockR = true;
					}
					foundTargetR = true;

					if(!isTargetAllyR)
						SendLockedMessage (targetpv.viewID, target.transform.gameObject.name);

					break;
				}
			}
			if (!foundTargetR) {
				crosshairImage.OnTargetR(false);
				targetR = null;
				LockR = false;
			}else{
				foundTargetR = false;
			}
		}


		foreach (GameObject g in TargetsToRemove) {//remove null target
			Targets.Remove (g);
		}
		TargetsToRemove.Clear ();

		MarkTarget ();
		//crosshairImage.targetMark.enabled = !(targetL==null&&targetR==null);
	}

	public Transform getCurrentTargetL(){
		if (isRCL)
			return null;
		
		if (targetL != null && !isTargetAllyL) {
			//cast a ray to check if hitting shield
			//Debug.DrawRay (cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), ((targetL.transform.root.position + new Vector3 (0, 5, 0)) - cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH))*100f, Color.red, 3f);
			RaycastHit[] hitpoints;
			hitpoints = Physics.RaycastAll (cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), (targetL.transform.root.position+new Vector3 (0,5,0))-cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), MaxDistanceL, playerlayer).OrderBy(h=>h.distance).ToArray();
			foreach(RaycastHit hit in hitpoints){
				if(isTeamMode){
					PhotonView targetpv = hit.transform.root.GetComponent<PhotonView> ();
					if(targetpv.owner.GetTeam()!=pv.owner.GetTeam()){
						return hit.collider.transform;
					}
				}else{
					PhotonView targetpv = hit.transform.root.GetComponent<PhotonView> ();
					if (targetpv.viewID != pv.viewID) {
						return hit.collider.transform;
					}
				}
			}
		}

		return targetL;
	}
	public Transform getCurrentTargetR(){
		if (isRCL)
			return null;
		
		if (targetR != null && !isTargetAllyR) {
			//cast a ray to check if hitting shield
			RaycastHit[] hitpoints;
			hitpoints = Physics.RaycastAll (cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), (targetR.transform.root.position + new Vector3 (0,5,0)) - cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), MaxDistanceR, playerlayer).OrderBy (h => h.distance).ToArray ();
			foreach(RaycastHit hit in hitpoints){
				if(isTeamMode){
					PhotonView targetpv = hit.transform.root.GetComponent<PhotonView> ();
					if(targetpv.owner.GetTeam()!=pv.owner.GetTeam()){
						return hit.collider.transform;
					}
				}else{
					PhotonView targetpv = hit.transform.root.GetComponent<PhotonView> ();
					if(targetpv.viewID != pv.viewID) //if not mine
						return hit.collider.transform;
				}
			}
		}

		return targetR;
	}

	private void MarkTarget(){
		if(isENG_L){
			if(targetL!=null){
				crosshairImage.EngTargetMark.transform.position = cam.WorldToScreenPoint (targetL.transform.position + new Vector3 (0, 5, 0));
			}
		}else{
			if(targetL!=null){
				crosshairImage.targetMark.transform.position = cam.WorldToScreenPoint (targetL.transform.position + new Vector3 (0, 5, 0));
			}
		}

		if(isENG_R){
			if(targetR!=null){
				crosshairImage.EngTargetMark.transform.position = cam.WorldToScreenPoint (targetR.transform.position + new Vector3 (0, 5, 0));
			}
		}else{
			if(targetR!=null){
				crosshairImage.targetMark.transform.position = cam.WorldToScreenPoint (targetR.transform.position + new Vector3 (0, 5, 0));
			}
		}
		if((!isENG_L && targetL != null) || (!isENG_R && targetR != null)){
			crosshairImage.middlecross.enabled = false;
		}else{
			crosshairImage.middlecross.enabled = true;
		}

		crosshairImage.EngTargetMark.enabled = ((isENG_L && targetL != null) || (isENG_R && targetR != null));
		crosshairImage.targetMark.gameObject.SetActive((!isENG_L && targetL != null) || (!isENG_R && targetR != null));
	}

	void SendLockedMessage(int id, string Name){
		if (id == LastLockTargetID) {
			if (Time.time - TimeOfLastSend >= SendMsgDeltaTime) {
				pv.RPC ("OnLocked", PhotonTargets.All, Name);
				TimeOfLastSend = Time.time;
			}
		} else {
			pv.RPC ("OnLocked", PhotonTargets.All, Name);
			TimeOfLastSend = Time.time;
			LastLockTargetID = id;
		}
	}

	public void ShowLocked(){
		if(isOnLocked){
			StopCoroutine (coroutine);
			coroutine = StartCoroutine ("HideLockedAfterTime",LockedMsgDuration);
		}else{
			isOnLocked = true;
			coroutine = StartCoroutine ("HideLockedAfterTime",LockedMsgDuration);
			Sounds.PlayOnLocked ();
		}

	}

	IEnumerator HideLockedAfterTime(float time){
		LockedImg.SetActive (true);
		yield return new WaitForSeconds (time);
		LockedImg.SetActive (false);
		isOnLocked = false;
	}
}
