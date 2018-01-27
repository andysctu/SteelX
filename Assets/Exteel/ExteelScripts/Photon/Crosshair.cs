using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour {

	public float CrosshairRadius = 10f;
	public float MaxDistance = 100f;

	private CrosshairImage crosshairImage;
	private Camera camera;
	public LayerMask layerMask = 8;

	[SerializeField]
	private Sounds Sounds;
	private Transform target;
	private bool Lock = false;
	public const float CAM_DISTANCE_TO_MECH = 20f;
	// Use this for initialization
	void Start () {
		CrosshairRadius = 10f;
		camera = transform.GetComponent<Camera>();
		crosshairImage = GetComponentInChildren<CrosshairImage> ();
		if (crosshairImage == null)
			print ("crosshairImage is null.");
		crosshairImage.SetRadius (CrosshairRadius);
		crosshairImage.SetCurrentImage (0);
	}

	public void NoCrosshair() {
		crosshairImage.noCrosshair = true;
	}

	void Update () {
		RaycastHit hit;
		//Debug.DrawLine (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH) + camera.transform.forward * 20);

		if (Physics.SphereCast (camera.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), CrosshairRadius, camera.transform.forward, out hit, MaxDistance, layerMask)) {
			crosshairImage.SetCurrentImage (1);
			target = hit.transform;
			if(Lock == false){
				Sounds.PlayLock ();
				Lock = true;
			}
			//play Lock sound
		} else {
			crosshairImage.SetCurrentImage (0);
			target = null;
			Lock = false;
		}
	}

	public Transform getCurrentTarget(){
		return target;
	}
}
