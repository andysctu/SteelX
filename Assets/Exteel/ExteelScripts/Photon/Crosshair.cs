using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour {
	private const float SendMsgDeltaTime = 0.3f;
	private float TimeOfLastSend;
	private float CrosshairRadiusL ;
	private float CrosshairRadiusR ;
	private int LastLockTargetID = 0 ;
	private bool LockL = false, LockR = false , foundTargetL=false, foundTargetR=false;
	private bool isOnLocked = false;
	private const float LockedMsgDuration = 0.5f;
	private Coroutine coroutine = null;
	public const float CAM_DISTANCE_TO_MECH = 20f;

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
	private Weapon[] weaponScripts;
	[SerializeField]
	private CrosshairImage crosshairImage;
	private Camera camera;
	public LayerMask layerMask = 8;


	[SerializeField]
	private Sounds Sounds;
	private Transform targetL,targetR;
	private RaycastHit hit;

	void Start () {
		weaponScripts = bm.weaponScripts;
		camera = transform.GetComponent<Camera>();
		crosshairImage.SetRadius (CrosshairRadiusL,CrosshairRadiusR);
		updateCrosshair (0);

		SphereRadiusCoeff = 0.04f;
		DistanceCoeff = 0.008f;
	}

	public void NoCrosshair() {
		if (crosshairImage != null) {
			crosshairImage.NoCrosshairL ();
			crosshairImage.NoCrosshairR ();

			targetL = null;	
			targetR = null;
		}
	}
	public void updateCrosshair(int offset){
		if (weaponScripts == null)
			weaponScripts = bm.weaponScripts;//sometimes it's null, don't know why
		CrosshairRadiusL = weaponScripts [offset].radius;
		CrosshairRadiusR = weaponScripts [offset+1].radius;
		MaxDistanceL = weaponScripts [offset].Range;
		MaxDistanceR = weaponScripts [offset+1].Range;

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
	}

	void Update () {
		if (CrosshairRadiusL > 0) {
			RaycastHit[] targets = Physics.SphereCastAll (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadiusL*MaxDistanceL*SphereRadiusCoeff, camera.transform.forward,MaxDistanceL, layerMask);
			foreach(RaycastHit target in targets){
				if (target.collider.name == PhotonNetwork.playerName)
					continue;

				Vector2 targetLocInCam = new Vector2 (camera.WorldToViewportPoint (target.transform.position).x, camera.WorldToViewportPoint (target.transform.position).y*0.65f);
				Vector2 CamMidpoint = new Vector2 (0.5f, 0.5f * 0.65f); // due to wide screen

				if (Vector2.Distance (targetLocInCam, CamMidpoint) < DistanceCoeff *  CrosshairRadiusL) { 
					crosshairImage.SetCurrentLImage (1);
					targetL = target.transform;
					if (LockL == false) {
						Sounds.PlayLock ();
						LockL = true;
					}
					foundTargetL = true;
					print (target.transform.root.GetComponent<PhotonView> ().viewID);
					SendLockedMessage (target.transform.root.GetComponent<PhotonView> ().viewID, target.transform.root.gameObject.name);

					break;
				} 
			}
			if (foundTargetL == false) {
				crosshairImage.SetCurrentLImage (0);
				targetL = null;
				LockL = false;
			}else{
				foundTargetL = false;
			}
		}
		if (CrosshairRadiusR > 0) {
			RaycastHit[] targets = Physics.SphereCastAll (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadiusR * MaxDistanceR * SphereRadiusCoeff, camera.transform.forward, MaxDistanceR, layerMask);
			foreach (RaycastHit target in targets) {
				if (target.collider.name == PhotonNetwork.playerName)
					continue;

				Vector2 targetLocInCam = new Vector2 (camera.WorldToViewportPoint (target.transform.position).x, camera.WorldToViewportPoint (target.transform.position).y * 0.65f);
				Vector2 CamMidpoint = new Vector2 (0.5f, 0.5f * 0.65f);

				if (Vector2.Distance (targetLocInCam, CamMidpoint) < DistanceCoeff * CrosshairRadiusR) { 
					crosshairImage.SetCurrentRImage (1);
					targetR = target.transform;
					if (LockR == false) {
						Sounds.PlayLock ();
						LockR = true;
					}
					foundTargetR = true;
					SendLockedMessage (target.transform.root.GetComponent<PhotonView> ().viewID, target.transform.root.gameObject.name);

					break;
				}
			}
			if (foundTargetR == false) {
				crosshairImage.SetCurrentRImage (0);
				targetR = null;
				LockR = false;
			}else{
				foundTargetR = false;
			}
		}
	}

	public Transform getCurrentTargetL(){
		return targetL;
	}
	public Transform getCurrentTargetR(){
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
		if(isOnLocked==true){
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
