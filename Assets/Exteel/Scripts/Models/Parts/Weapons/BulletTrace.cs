using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour {

	public GameObject bulletImpact;
	private GameObject bulletImpact_onShield;
	public HUD HUD;
	public Camera cam;
	public LayerMask layerMask = 8, Terrain = 10;
	public Vector3 direction;
	public Transform Shield;
	private Rigidbody rb;
	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[1];
	private Transform Target;

	public string ShooterName;
	public bool isTargetShield;
	public bool isLMG = false; //if it's LMG , then show Multiple Hit messages
	private bool isfollow = false;
	private float bulletSpeed = 400;
	private bool isCollided = false;
	private bool hasSlowdown = false;

	void Awake(){
		bulletImpact_onShield = Resources.Load ("HitShieldEffect") as GameObject;
	}
	void Start () {
		bulletImpact_onShield.GetComponent<BulletImpact> ().ImpactSound = bulletImpact.GetComponent<BulletImpact> ().ImpactSound;
		ParticleSystem ps = GetComponent<ParticleSystem>();
		ps.Play();

		if(gameObject.transform.parent == null){//no target => move directly
			GetComponent<Rigidbody> ().velocity = direction * bulletSpeed;
			transform.LookAt (direction*9999);
			Destroy(gameObject, 2f);
		}else{//target exists
			Target = transform.parent;
			transform.LookAt (Target);
			isfollow = true;
			Destroy(gameObject, 2f);
		}
	}

	void Update(){
		if(!isfollow){
			return;
		}else{
			if(!hasSlowdown){
				if(Vector3.Distance(transform.position, Target.position) < 20f){
					bulletSpeed = 280f;
					hasSlowdown = true;
				}
			}
			Vector3 dir = -(transform.position - Target.position - new Vector3(0,5,0)).normalized;
			GetComponent<Rigidbody> ().velocity = bulletSpeed*dir;
			transform.LookAt (Target);
		}
	}



	void OnParticleCollision(GameObject other){
		if (isCollided)
			return;
		if(isfollow){
			if (other.transform.root.name == Target.gameObject.name) {
				isCollided = true;
				GetComponent<ParticleSystem> ().Stop ();

				if(isLMG&&PhotonNetwork.playerName == ShooterName){
					if (!isTargetShield)
						HUD.ShowText (cam, other.transform.position + new Vector3 (0, 5f, 0), "Hit");
					else {
						HUD.ShowText (cam, other.transform.position, "Defense");
					}
				}

				GetComponent<ParticleSystem> ().GetCollisionEvents (other, collisionEvents);
				Vector3 collisionHitLoc = collisionEvents [0].intersection;

				GameObject BI;
				if (!isTargetShield) {
					BI = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
				}else{
					BI = Instantiate (bulletImpact_onShield, Shield.position, Quaternion.identity);
				}
				BI.transform.LookAt (cam.transform);

				BI.GetComponent<ParticleSystem> ().Play ();

				Destroy (gameObject, 0.5f);
			}
		}else{
			if(other.layer==Terrain){
				isCollided = true;
				GetComponent<ParticleSystem> ().Stop ();
				GetComponent<ParticleSystem> ().GetCollisionEvents (other, collisionEvents);
				Vector3 collisionHitLoc = collisionEvents [0].intersection;
				GameObject temp = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
				temp.GetComponent<ParticleSystem> ().Play ();

				Destroy (gameObject, 0.5f);
			}
		}
	}
}
