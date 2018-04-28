using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashDetector : MonoBehaviour {

	[SerializeField]private MechCamera cam;
	[SerializeField]private BoxCollider boxCollider;
	[SerializeField]private MechController mctrl;
	[SerializeField]private LayerMask playerLayer;
	private List<Transform> Target = new List<Transform>();
	private float clamped_cam_angle_x;
	private GameObject User;
	private float clampAngle = 45;
	private float mech_Midpoint = 5;
	private float clamp_angle_coeff = 0.3f;//how much the cam angle affecting the y pos of box collider
	private Vector3 inair_c = new Vector3(0,0.6f,3.6f),inair_s = new Vector3(10,18,15);
	private Vector3 onground_c = new Vector3(0,0.6f,-1.4f), onground_s = new Vector3(10,11,5);

	void Start(){
		GetUser ();
	}

	void GetUser(){
		User = transform.root.gameObject;
	}

	void Update(){
		if (!mctrl.grounded) {
			clamped_cam_angle_x = Mathf.Clamp (cam.GetCamAngle (), -clampAngle, clampAngle);
			transform.localPosition = new Vector3 (transform.localPosition.x, mech_Midpoint + clamped_cam_angle_x * clamp_angle_coeff, transform.localPosition.z);

			//set collider size
			SetCenter (inair_c);
			SetSize (inair_s);
		}else{
			transform.localPosition = new Vector3 (transform.localPosition.x, mech_Midpoint , transform.localPosition.z);

			SetCenter (onground_c);
			SetSize (onground_s);
		}
	}

	void OnTriggerEnter(Collider target){
		if (target.gameObject != User && target.tag[0]!='S') {//in player layer but not shield => player
			if(GameManager.isTeamMode){
				if ( target.tag == "Drone" || target.GetComponent<PhotonView>().owner.GetTeam() == PhotonNetwork.player.GetTeam())
					return;
			}
			Target.Add (target.transform);
		}
	}
	 
	void OnTriggerExit(Collider target){
		if(target.gameObject != User && target.tag[0]!='S'){
			if(GameManager.isTeamMode){
				if ( target.tag == "Drone" || target.GetComponent<PhotonView>().owner.GetTeam() == PhotonNetwork.player.GetTeam())
					return;
			}
			Target.Remove (target.transform);
		}	
	}

	void SetCenter(Vector3 v){
		boxCollider.center = v;
	}

	void SetSize(Vector3 v){
		boxCollider.size = v;
	}

	public List<Transform> getCurrentTargets(){
		return Target;
	}

	public void EnableDetector(bool b){
		boxCollider.enabled = b;
		enabled = b;
	}
}
