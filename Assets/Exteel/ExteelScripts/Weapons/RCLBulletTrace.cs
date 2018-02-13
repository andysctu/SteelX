using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCLBulletTrace : MonoBehaviour {

	public GameObject bulletImpact;

	public HUD hud;
	public Camera cam;
	public GameObject Shooter;

	private string ShooterName;
	private int ShooterID; // for efficiency
	[SerializeField]
	private LayerMask PlayerlayerMask;
	private int bulletdmg = 100;

	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[1] ;
	private Transform Target;
	private float bulletSpeed = 200;
	private bool isCollided = false;
	ParticleSystem ps ;


	void Start () {
		ps = GetComponent<ParticleSystem>();
		ps.Play();
		ShooterName = Shooter.name;
		ShooterID = Shooter.GetComponent<PhotonView> ().viewID;
		print ("ini ShooterName : " + ShooterName + " ID : " + ShooterID);
		GetComponent<Rigidbody> ().velocity = transform.forward * bulletSpeed;
		Destroy(gameObject, 2f);
	}

	void OnParticleCollision(GameObject other){
		if (isCollided == true || other == Shooter)
			return;
		
		isCollided = true;
		ps.Stop ();
			
		GetComponent<ParticleSystem> ().GetCollisionEvents (other, collisionEvents);
		Vector3 collisionHitLoc = collisionEvents[0].intersection;

		GameObject temp = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
		temp.GetComponent<ParticleSystem> ().Play ();//play bullet impact

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 6f, PlayerlayerMask); // get overlap targets

		List<int> colliderViewIds = new List<int> ();
		for (int i=0;i < hitColliders.Length;i++)
		{
			//check duplicated
			PhotonView colliderPV = hitColliders [i].transform.root.GetComponent<PhotonView> ();
			if(colliderViewIds.Contains(colliderPV.viewID)){
				print("already contains : "+(int)colliderPV.viewID);
				continue;
			}else{
				print ("added : " + colliderPV.viewID);
				colliderViewIds.Add (colliderPV.viewID);
			}

			print ("collider id :" + colliderPV.viewID + " SHooterID : " + ShooterID);
			if(colliderPV.viewID!=ShooterID){

				if (hitColliders [i].tag == "Shield") {
					
					if (Shooter.GetComponent<PhotonView> ().isMine) {
						if (hitColliders [i].transform.root.GetComponent<Combat> ().CurrentHP () - bulletdmg / 2 <= 0) {
							hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Kill");
						} else {
							hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Defense");
						}
						hitColliders [i].transform.root.GetComponent<PhotonView> ().RPC ("OnHit", PhotonTargets.All, bulletdmg, ShooterName, 0f); 
					}

				} else {
					hitColliders [i].transform.root.GetComponent<Transform> ().position += transform.forward * 8f;  

					if (Shooter.GetComponent<PhotonView> ().isMine) {
						if (hitColliders [i].gameObject.GetComponent<Combat> ().CurrentHP () - bulletdmg <= 0) {
							hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Kill");
						} else {
							hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Hit");
						}
						hitColliders [i].GetComponent<PhotonView> ().RPC ("OnHit", PhotonTargets.All, bulletdmg, ShooterName, 0f); 

					}
				}
			}
		}
	}

}
