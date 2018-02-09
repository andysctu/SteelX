using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour {

	private float CrosshairRadiusL ;
	private float CrosshairRadiusR ;

	public float MaxDistanceL ;
	public float MaxDistanceR ;

	[SerializeField]
	private BuildMech bm;
	private Weapon[] weaponScripts;
	private CrosshairImage crosshairImage;
	private Camera camera;
	public LayerMask layerMask = 8;

	public float SphereRadiusCoeff;
	public float DistanceCoeff;

	[SerializeField]
	private Sounds Sounds;
	private Transform targetL,targetR;
	private RaycastHit hit;
	private bool LockL = false, LockR = false , foundTargetL=false, foundTargetR=false;
	public const float CAM_DISTANCE_TO_MECH = 20f;

	void Start () {
		weaponScripts = bm.weaponScripts;
		camera = transform.GetComponent<Camera>();
		crosshairImage = GetComponentInChildren<CrosshairImage> ();
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
}
