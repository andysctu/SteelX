using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour {

	private float CrosshairRadiusL ;
	private float CrosshairRadiusR ;

	public float MaxDistance = 100f;

	[SerializeField]
	private BuildMech bm;
	private Weapon[] weaponScripts;
	private CrosshairImage crosshairImage;
	private Camera camera;
	public LayerMask layerMask = 8;

	[SerializeField]
	private Sounds Sounds;
	private Transform targetL,targetR;
	private bool Lock = false;
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
			if (Physics.SphereCast (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadiusL, camera.transform.forward, out hit, MaxDistance, layerMask)) {
				crosshairImage.SetCurrentLImage (1);
				targetL = hit.transform;
				if (Lock == false) {
					Sounds.PlayLock ();
					Lock = true;
				}
				//play Lock sound
			} else {
				crosshairImage.SetCurrentLImage (0);
				targetL = null;
				Lock = false;
			}
		}
		if (CrosshairRadiusR > 0) {
			if (Physics.SphereCast (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadiusR, camera.transform.forward, out hit, MaxDistance, layerMask)) {
				crosshairImage.SetCurrentRImage (1);
				targetR = hit.transform;
				if (Lock == false) {
					Sounds.PlayLock ();
					Lock = true;
				}
				//play Lock sound
			} else {
				crosshairImage.SetCurrentRImage (0);
				targetR = null;
				Lock = false;
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
