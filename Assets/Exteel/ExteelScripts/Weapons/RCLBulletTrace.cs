using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCLBulletTrace : MonoBehaviour {

	public GameObject bulletImpact;
	private Rigidbody rb;
	public HUD hud;
	public Camera cam;
	public GameObject Shooter;
	public string ShooterName;
	public LayerMask layerMask = 8;
	private int bulletdmg = 100;

	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[1];
	private Transform Target;
	private float bulletSpeed = 200;
	private bool isCollided = false;
	ParticleSystem ps ;


	void Start () {
		ps = GetComponent<ParticleSystem>();
		ps.Play();

		GetComponent<Rigidbody> ().velocity = transform.forward * bulletSpeed;
		Destroy(gameObject, 2f);
	}

	void OnParticleCollision(GameObject other){
		if (isCollided == true || other == Shooter)
			return;
		isCollided = true;
		ps.Stop ();
			
		GetComponent<ParticleSystem> ().GetCollisionEvents (other, collisionEvents);
		Vector3 collisionHitLoc = collisionEvents [0].intersection;
		GameObject temp = Instantiate (bulletImpact, collisionHitLoc, Quaternion.identity);
		temp.GetComponent<ParticleSystem> ().Play ();

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);
		for (int i=0;i < hitColliders.Length;i++)
		{
			if(hitColliders[i].gameObject.layer == layerMask  && hitColliders[i].gameObject.name!=ShooterName){
				hitColliders[i].GetComponent<Transform>().position += transform.forward*5f;
				if (PhotonNetwork.playerName == ShooterName) {
					if (hitColliders[i].gameObject.GetComponent<Combat>().CurrentHP() - bulletdmg<= 0) {
						hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Kill");
					} else {
						hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Hit");
					}
				}
				if(hitColliders[i].GetComponent<PhotonView>().isMine)	//avoid multi-calls
				{
					hitColliders[i].GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, bulletdmg, ShooterName); 
				}
			}
			print (hitColliders [i].gameObject.name);
		}
	}

}
