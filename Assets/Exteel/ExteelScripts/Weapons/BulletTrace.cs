using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour {

	public GameObject bulletImpact;
	private Rigidbody rb;

	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[4];
	private bool isfollow = false;
	private Transform Target;
	private float bulletSpeed = 120;

	void Start () {
		ParticleSystem ps = GetComponent<ParticleSystem>();
		ps.Play();

		if(gameObject.transform.parent == null){//no target => move directly
			GetComponent<Rigidbody> ().velocity = transform.forward * bulletSpeed;
			Destroy(gameObject, 2f);
		}else{//target exists
			Target = gameObject.transform.parent.GetComponent<Transform>();
			if(Target == null){
				print ("Fatal error : Can not find the target's transform.");
			}
			isfollow = true;
		}
	}

	void Update(){
		if(isfollow==false){
			return;
		}else{
			Vector3 dir = -(transform.position - Target.position - new Vector3(0,5,0)).normalized;
			GetComponent<Rigidbody> ().velocity = bulletSpeed*dir;
		}

	}

	void OnParticleCollision(GameObject other){

		if ( other.layer != 8  || (Target!=null && other.name == Target.gameObject.name) ) {
				GetComponent<ParticleSystem> ().Stop ();

				int numCollisionEvents = GetComponent<ParticleSystem> ().GetCollisionEvents (other, collisionEvents);
				int i = 0;
				while (i < numCollisionEvents) {
					Vector3 collisionHitLoc = collisionEvents [i].intersection;

					//play impact
					GameObject temp = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
					temp.GetComponent<ParticleSystem> ().Play ();
					i++;
				}
			Destroy (gameObject, 0.5f);
			}
	}
}
