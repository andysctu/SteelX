using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class Crosshair : MonoBehaviour {
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
	private const float LockedMsgDuration = 0.5f;//when receiving a lock message , the time it'll last
	public const float CAM_DISTANCE_TO_MECH = 17f;

	public float SphereRadiusCoeff;
	public float DistanceCoeff;

	public float MaxDistanceL ;
	public float MaxDistanceR ;

	[SerializeField]
	private BuildMech bm;
	[SerializeField]
	private PhotonView pv;
	[SerializeField]
	private GameObject LockedImg;
	[SerializeField]
	private CrosshairImage crosshairImage;
	[SerializeField]
	private LayerMask playerlayer;
	[SerializeField]
	private Sounds Sounds;

	private Weapon[] weaponScripts;
	//private Camera camera;
	private Transform targetL,targetR;
	private RaycastHit hit;
	private Coroutine coroutine = null;

	void Start () {
		screenCoeff = (float)Screen.height / Screen.width;
		weaponScripts = bm.weaponScripts;
		//camera = transform.GetComponent<Camera>();
		crosshairImage.SetRadius (CrosshairRadiusL,CrosshairRadiusR);
		updateCrosshair (0);
		isTeamMode = GameManager.isTeamMode;
		SphereRadiusCoeff = 0.04f;
		DistanceCoeff = 0.008f;
		crosshairImage.targetMark.enabled = false;
	}

	public void NoCrosshair() {
		if (crosshairImage != null) {
			crosshairImage.NoCrosshairL ();
			crosshairImage.NoCrosshairR ();

			targetL = null;	
			targetR = null;
			crosshairImage.targetMark.enabled = false;
		}
	}
	public void updateCrosshair(int offset){
		weaponScripts = bm.weaponScripts;//sometimes it's null, don't know why

		CrosshairRadiusL = weaponScripts [offset].radius;
		CrosshairRadiusR = weaponScripts [offset+1].radius;
		MaxDistanceL = weaponScripts [offset].Range;
		MaxDistanceR = weaponScripts [offset+1].Range;

		if(weaponScripts[offset].Animation == "ENGShoot"){
			isTargetAllyL = true;
		}else{
			isTargetAllyL = false;
		}
		if(weaponScripts[offset+1].Animation == "ENGShoot"){
			isTargetAllyR = true;
		}else{
			isTargetAllyR = false;
		}

		crosshairImage.SetRadius (CrosshairRadiusL,CrosshairRadiusR);

		if(CrosshairRadiusL!=0){
			crosshairImage.SetCurrentLImage (0);
		}else{
			crosshairImage.NoCrosshairL ();
		}
		if (CrosshairRadiusR != 0) {
			crosshairImage.SetCurrentRImage (0);
		} else{
			crosshairImage.NoCrosshairR ();
		}

		if(weaponScripts[offset].Animation == "ShootRCL"){
			crosshairImage.RCLcrosshair ();
		}
		targetL = null;
		targetR = null; 

		crosshairImage.targetMark.enabled = false;
	}

	void Update () {
		if (CrosshairRadiusL > 0) {
			RaycastHit[] targets = Physics.SphereCastAll (GetComponent<Camera>().transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH + CrosshairRadiusL*MaxDistanceL*SphereRadiusCoeff), CrosshairRadiusL*MaxDistanceL*SphereRadiusCoeff, GetComponent<Camera>().transform.forward,MaxDistanceL, playerlayer);
			foreach(RaycastHit target in targets){
				PhotonView targetpv = target.transform.root.GetComponent<PhotonView> ();
				if (targetpv.viewID == pv.viewID)
					continue;

				if(isTeamMode){
					if (target.collider.tag == "Drone"){
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
				//print ("crosshair target : " + target);
				Vector2 targetLocInCam = new Vector2 (GetComponent<Camera>().WorldToViewportPoint (target.transform.position).x, (GetComponent<Camera>().WorldToViewportPoint (target.transform.position + new Vector3 (0, 5, 0)).y -0.5f) * screenCoeff + 0.5f);
				Vector2 CamMidpoint = new Vector2 (0.5f, 0.5f);

				if (Vector2.Distance (targetLocInCam, CamMidpoint) < DistanceCoeff * CrosshairRadiusL) { 
					crosshairImage.SetCurrentLImage (1);
					targetL = target.transform;

					//move target mark

					crosshairImage.targetMark.transform.position = GetComponent<Camera>().WorldToScreenPoint (target.transform.root.position + new Vector3(0,5,0));

					if (!LockL) {
						Sounds.PlayLock ();
						LockL = true;
					}
					foundTargetL = true;
					if(!isTargetAllyL)
						SendLockedMessage (targetpv.viewID, target.transform.root.gameObject.name);

					break;
				} 
			}
			if (!foundTargetL) {
				crosshairImage.SetCurrentLImage (0);				
				targetL = null;
				LockL = false;
			}else{
				foundTargetL = false;
			}
		}
		if (CrosshairRadiusR > 0) {
			RaycastHit[] targets = Physics.SphereCastAll (GetComponent<Camera>().transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH + CrosshairRadiusR * MaxDistanceR * SphereRadiusCoeff), CrosshairRadiusR * MaxDistanceR * SphereRadiusCoeff, GetComponent<Camera>().transform.forward, MaxDistanceR, playerlayer);
			foreach (RaycastHit target in targets) {
				PhotonView targetpv = target.transform.root.GetComponent<PhotonView> ();
				if (targetpv.viewID == pv.viewID)
					continue;

				if(isTeamMode){
					if (target.collider.tag == "Drone"){
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

				Vector2 targetLocInCam = new Vector2 (GetComponent<Camera>().WorldToViewportPoint (target.transform.position).x, (GetComponent<Camera>().WorldToViewportPoint (target.transform.position + new Vector3 (0, 5, 0)).y - 0.5f) * screenCoeff + 0.5f);
				Vector2 CamMidpoint = new Vector2 (0.5f, 0.5f);

				if (Vector2.Distance (targetLocInCam, CamMidpoint) < DistanceCoeff * CrosshairRadiusR) { 
					crosshairImage.SetCurrentRImage (1);
					targetR = target.transform;

					//Debug.DrawRay (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), (targetR.transform.root.position + new Vector3 (0, 5, 0)) - camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), Color.red, 1f);

					//move target mark
					crosshairImage.targetMark.transform.position =  GetComponent<Camera>().WorldToScreenPoint (target.transform.root.position + new Vector3(0,5,0));

					if (!LockR) {
						Sounds.PlayLock ();
						LockR = true;
					}
					foundTargetR = true;

					if(!isTargetAllyR)
						SendLockedMessage (targetpv.viewID, target.transform.root.gameObject.name);

					break;
				}
			}
			if (!foundTargetR) {
				crosshairImage.SetCurrentRImage (0);
				targetR = null;
				LockR = false;
			}else{
				foundTargetR = false;
			}
		}

		crosshairImage.targetMark.enabled = !(targetL==null&&targetR==null);
	}

	public Transform getCurrentTargetL(){
		if (targetL != null && !isTargetAllyL) {
			//cast a ray to check if hitting shield
			RaycastHit[] hitpoints;
			hitpoints = Physics.RaycastAll (GetComponent<Camera>().transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), (targetL.transform.root.position+new Vector3 (0,5,0))-GetComponent<Camera>().transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), MaxDistanceL, playerlayer).OrderBy(h=>h.distance).ToArray();
			foreach(RaycastHit hit in hitpoints){
				if(isTeamMode){
					PhotonView targetpv = hit.transform.root.GetComponent<PhotonView> ();
					if(targetpv.owner.GetTeam()!=pv.owner.GetTeam()){
						return hit.transform;
					}
				}else{
					return hit.transform;
				}
			}
		}

		return targetL;
	}
	public Transform getCurrentTargetR(){
		if (targetR != null && !isTargetAllyR) {
			//cast a ray to check if hitting shield
			RaycastHit[] hitpoints;
			hitpoints = Physics.RaycastAll (GetComponent<Camera>().transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), (targetR.transform.root.position + new Vector3 (0,5,0)) - GetComponent<Camera>().transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), MaxDistanceR, playerlayer).OrderBy (h => h.distance).ToArray ();
			foreach(RaycastHit hit in hitpoints){
				if(isTeamMode){
					PhotonView targetpv = hit.transform.root.GetComponent<PhotonView> ();
					if(targetpv.owner.GetTeam()!=pv.owner.GetTeam()){
						return hit.transform;
					}
				}else{
					return hit.transform;
				}
			}
		}

		return targetR;
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
