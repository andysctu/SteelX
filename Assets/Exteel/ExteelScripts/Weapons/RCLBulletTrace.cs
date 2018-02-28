using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCLBulletTrace : MonoBehaviour {

	public GameObject bulletImpact;

	public HUD hud;
	public Camera cam;
	public GameObject Shooter;

	private int ShooterID; // for efficiency
	[SerializeField]
	private LayerMask PlayerlayerMask;
	[SerializeField]
	private PhotonView pv;
	private int bulletdmg = 450;

	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[1] ;
	private Transform Target;
	private float bulletSpeed = 200;
	private bool isCollided = false;
	ParticleSystem ps ;


	void Start () {
		ps = GetComponent<ParticleSystem>();
		ps.Play();
		ShooterID = Shooter.GetComponent<PhotonView> ().viewID;
		GetComponent<Rigidbody> ().velocity = transform.forward * bulletSpeed;
		Destroy(gameObject, 2f);
	}

	void OnParticleCollision(GameObject other){
		if (isCollided || other == Shooter)
			return;
		
		if(GameManager.isTeamMode){
			if (other.layer == 8) {
				print (other.transform.root.GetComponent<PhotonView> ().owner.GetTeam () +" "+ Shooter.GetComponent<PhotonView> ().owner.GetTeam ());
				if (other.tag == "Drone" || other.transform.root.GetComponent<PhotonView> ().owner.GetTeam () == Shooter.GetComponent<PhotonView> ().owner.GetTeam ())
					return;
			}
		}	

		isCollided = true;
		ps.Stop ();
		GetComponent<ParticleSystem> ().GetCollisionEvents (other, collisionEvents);
		Vector3 collisionHitLoc = collisionEvents[0].intersection;

		//pv.RPC ("CallPlayImpact", PhotonTargets.All, collisionHitLoc);
		GameObject temp = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
		temp.GetComponent<ParticleSystem> ().Play ();//play bullet impact

		Collider[] hitColliders = Physics.OverlapSphere(collisionHitLoc, 6f, PlayerlayerMask); // get overlap targets
		List<int> colliderViewIds = new List<int> ();

		for (int i=0;i < hitColliders.Length;i++)
		{
			//check duplicated
			PhotonView colliderPV = hitColliders [i].transform.root.GetComponent<PhotonView> ();
			if(colliderViewIds.Contains(colliderPV.viewID)){
				continue;
			}else{
				if(GameManager.isTeamMode){
					//PhotonView pv = other.GetComponent<PhotonView> ();
					if (other.tag == "Drone" || colliderPV.owner.GetTeam () == Shooter.GetComponent<PhotonView> ().owner.GetTeam ())
						continue;
				}

				colliderViewIds.Add (colliderPV.viewID);
			}

			if(colliderPV.viewID!=ShooterID){

				if (hitColliders [i].tag == "Shield") {
					
					if (Shooter.GetComponent<PhotonView> ().isMine) {
						if (hitColliders [i].transform.root.GetComponent<Combat> ().CurrentHP () - bulletdmg / 2 <= 0) {
							hud.ShowText (cam, hitColliders [i].transform.position, "Kill");
						} else {
							hud.ShowText (cam, hitColliders [i].transform.position, "Defense");
						}
						int hand = (hitColliders[i].transform.parent.name [hitColliders[i].transform.parent.name.Length - 1] == 'L') ? 0 : 1;
						hitColliders [i].transform.root.GetComponent<PhotonView> ().RPC ("ShieldOnHit", PhotonTargets.All, bulletdmg/2, ShooterID, hand); 
					}

				} else {
					//hitColliders [i].transform.root.GetComponent<Transform> ().position += transform.forward * 5f;  
					//colliderPV.RPC ("ForceMove", PhotonTargets.All, transform.forward, 5f);

					if (Shooter.GetComponent<PhotonView> ().isMine) {

						//colliderPV.RPC ("ForceMove", PhotonTargets.All, transform.forward, 5f);

						if (hitColliders [i].gameObject.GetComponent<Combat> ().CurrentHP () - bulletdmg <= 0) {
							hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Kill");
						} else {
							hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Hit");
						}
						hitColliders [i].GetComponent<PhotonView> ().RPC ("OnHit", PhotonTargets.All, bulletdmg, ShooterID, 0.3f); 

					}else if(colliderPV.isMine){
						colliderPV.RPC ("ForceMove", PhotonTargets.All, transform.forward, 3f);
					}
				}
			}
		}
	}

	/*
	[PunRPC]
	void CallPlayImpact(Vector3 collisionHitLoc){
		GameObject temp = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
		temp.GetComponent<ParticleSystem> ().Play ();//play bullet impact
	}
	*/
}
