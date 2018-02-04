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

	[SerializeField]
	private Sounds Sounds;
	private Transform targetL,targetR;
	private bool LockL = false, LockR = false;
	public const float CAM_DISTANCE_TO_MECH = 20f;
	// Use this for initialization
	void Start () {
		weaponScripts = bm.weaponScripts;
		camera = transform.GetComponent<Camera>();
		crosshairImage = GetComponentInChildren<CrosshairImage> ();
		crosshairImage.SetRadius (CrosshairRadiusL,CrosshairRadiusR);
		updateCrosshair (0,1);
	}

	public void NoCrosshair() { // this is called only when disable player
		if (crosshairImage != null) {
			crosshairImage.NoCrosshairL ();
			crosshairImage.NoCrosshairR ();

			targetL = null;	
			targetR = null;
		}
	}
	public void updateCrosshair(int L, int R){
		CrosshairRadiusL = weaponScripts [L].radius;
		CrosshairRadiusR = weaponScripts [R].radius;
		MaxDistanceL = weaponScripts [L].Range;
		MaxDistanceR = weaponScripts [R].Range;

		crosshairImage.SetRadius (CrosshairRadiusL,CrosshairRadiusR);
	
		if(CrosshairRadiusL!=0){ // = not using melee weapon
			crosshairImage.SetCurrentLImage (0);
		}else{
			crosshairImage.NoCrosshairL ();
		}
		if (CrosshairRadiusR != 0) {
			crosshairImage.SetCurrentRImage (0);
		} else{
			crosshairImage.NoCrosshairR ();
		}

		if(weaponScripts[L].Animation == "ShootRCL"){
			crosshairImage.RCLcrosshair ();
		}
		targetL = null;
		targetR = null; 
	}

	void Update () {
		RaycastHit hit;
		//Debug.DrawLine (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH) + camera.transform.forward * 20);
		if (CrosshairRadiusL > 0) {
			if (Physics.SphereCast (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadiusL, camera.transform.forward, out hit, MaxDistanceL, layerMask)) {
				crosshairImage.SetCurrentLImage (1);
				targetL = hit.transform;
				if (LockL == false) {
					Sounds.PlayLock ();
					LockL = true;
				}
			} else {
				crosshairImage.SetCurrentLImage (0);
				targetL = null;
				LockL = false;
			}
		}
		if (CrosshairRadiusR > 0) {
			if (Physics.SphereCast (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadiusR, camera.transform.forward, out hit, MaxDistanceR, layerMask)) {
				crosshairImage.SetCurrentRImage (1);
				targetR = hit.transform;
				if (LockR == false) {
					Sounds.PlayLock ();
					LockR = true;
				}
			} else {
				crosshairImage.SetCurrentRImage (0);
				targetR = null;
				LockR = false;
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
