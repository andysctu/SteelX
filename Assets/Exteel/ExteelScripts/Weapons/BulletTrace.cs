using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour {

	public GameObject bulletImpact;
	public HUD HUD;
	public Camera cam;
	public LayerMask layerMask = 8, Terrain = 10;
	private Rigidbody rb;
	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[1];
	private Transform Target;

	public string ShooterName;
	public bool isTargetShield;
	public bool isLMG = false; //if it's LMG , then show Multiple Hit messages
	private bool isfollow = false;
	private float bulletSpeed = 300; // too high will cause collision problem (like 320)
	private bool isCollided = false;

	void Start () {
		ParticleSystem ps = GetComponent<ParticleSystem>();
		ps.Play();

		if(gameObject.transform.parent == null){//no target => move directly
			GetComponent<Rigidbody> ().velocity = transform.forward * bulletSpeed;
			Destroy(gameObject, 2f);
		}else{//target exists
			Target = gameObject.transform.parent.GetComponent<Transform>();
			
			if (Target.tag == "Shield") {
				isTargetShield = true;
			} else
				isTargetShield = false;
			
			isfollow = true;
			Destroy(gameObject, 2f);
		}
	}

	void Update(){
		if(isfollow==false){
			return;
		}else{
			Vector3 dir = (isTargetShield)? -(transform.position - Target.position).normalized :-(transform.position - Target.position - new Vector3(0,5,0)).normalized;
			GetComponent<Rigidbody> ().velocity = bulletSpeed*dir;
		}
	}



	void OnParticleCollision(GameObject other){
		if (isCollided)
			return;

		if(isfollow){
			if (other.name == Target.gameObject.name) {
				isCollided = true;
				GetComponent<ParticleSystem> ().Stop ();

				if(isLMG==true&&PhotonNetwork.playerName == ShooterName){
					if(isTargetShield==false)
						HUD.ShowText (cam, other.transform.position + new Vector3(0,5f,0), "Hit");
					else 
						HUD.ShowText (cam, other.transform.position + new Vector3(0,5f,0), "Defense");
				}

				GetComponent<ParticleSystem> ().GetCollisionEvents (other, collisionEvents);
				Vector3 collisionHitLoc = collisionEvents [0].intersection;

				GameObject temp = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
				temp.GetComponent<ParticleSystem> ().Play ();

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
