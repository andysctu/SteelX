using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashDetector : MonoBehaviour {

	public GameObject User;
	[SerializeField]
	private MechCamera cam;
	[SerializeField]
	private BoxCollider boxCollider;
	[SerializeField]
	private MechController mctrl;
	private List<Transform> Target = new List<Transform>();
	private float clamped_cam_angle_x;
	public bool is_lookingUp;
	void Start(){
		
	}

	void Update(){
		clamped_cam_angle_x = Mathf.Clamp (cam.GetFollowAngle_x(), -45f, 45f);
		transform.localPosition = new Vector3(transform.localPosition.x, 5f + clamped_cam_angle_x*0.2f ,transform.localPosition.z);
		if (clamped_cam_angle_x >= 0){
			is_lookingUp = true;
			if(!mctrl.grounded){
				boxCollider.size =new Vector3(boxCollider.size.x,18f, 15f);
				boxCollider.center = new Vector3(boxCollider.center.x,4.1f, 3.6f);
			}else{
				boxCollider.size =new Vector3(boxCollider.size.x,11f, 5f);
				boxCollider.center = new Vector3(boxCollider.center.x,0.6f, -1.4f);
			}
		}else{
			is_lookingUp = false;
			if(!mctrl.grounded){
				boxCollider.size =new Vector3(boxCollider.size.x,18f, 15f);
				boxCollider.center = new Vector3(boxCollider.center.x,-2.9f, 3.6f);
			}else{
				boxCollider.size =new Vector3(boxCollider.size.x,11f, 5f);
				boxCollider.center = new Vector3(boxCollider.center.x,0.6f, -1.4f);
			}
		}
	}

	void OnTriggerEnter(Collider target){
		if (target.gameObject != User && (target.tag == "Drone" || target.tag == "Player" )) {

			if(GameManager.isTeamMode){
				if ( target.tag == "Drone" || target.transform.root.GetComponent<PhotonView>().owner.GetTeam() == PhotonNetwork.player.GetTeam())
					return;
			}
			print ("add target : " + target);
			Target.Add (target.transform);
		}
	}
	 
	void OnTriggerExit(Collider target){
		if(target.gameObject != User &&(target.tag == "Drone" || target.tag == "Player" ) ){

			if(GameManager.isTeamMode){
				if ( target.tag == "Drone" || target.transform.root.GetComponent<PhotonView>().owner.GetTeam() == PhotonNetwork.player.GetTeam())
					return;
			}

			Target.Remove (target.transform);
		}	
	}
	public List<Transform> getCurrentTargets(){
		return Target;
	} 
}
