using System.Collections.Generic;
using UnityEngine;
using System;

public class RCLBulletTrace : MonoBehaviour {

	[SerializeField]private GameObject bulletImpact_onShield, bulletImpact;
    [SerializeField]private LayerMask playerLayer, terrain;
    [SerializeField]private int bulletdmg = 450;
    [SerializeField]private float bulletSpeed = 200, impact_radius = 6;

    private ParticleSystem ps;
    private PhotonView shooter_pv, bullet_pv;
    private HUD hud;
    private Camera cam;
    private GameObject shooter;
    private Transform target;
    private int shooter_viewID;

	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[1] ;	
	private bool isCollided = false, hasCalledPlayImpact = false;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0);

	void Start () {
        InitComponents();        
        InitVelocity();

        ps.Play();
        Destroy(gameObject, 2f);
	}

    void InitComponents() {
        ps = GetComponent<ParticleSystem>();
        bullet_pv = GetComponent<PhotonView>();
    }

    void InitVelocity() {
        GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;
    }

    public void SetShooterInfo(GameObject shooter, HUD hud, Camera cam) {
        this.shooter = shooter;
        this.hud = hud;
        this.cam = cam;
        shooter_pv = shooter.GetComponent<PhotonView>();
        shooter_viewID = shooter_pv.viewID;
    }

	void OnParticleCollision(GameObject other){//player do the logic ; master destroy this 
		if (isCollided || shooter_pv==null || other == shooter)
			return;
		
		if(GameManager.isTeamMode){
			if (((1<<other.layer)&playerLayer) != 0) {//check if other.layer equals player layer
				if (other.tag == "Drone" || other.transform.root.GetComponent<PhotonView> ().owner.GetTeam () == shooter_pv.owner.GetTeam ())
					return;
			}
		}	


        isCollided = true;
        GetComponent<ParticleSystem>().GetCollisionEvents(other, collisionEvents);
        Vector3 impact_point = collisionEvents[0].intersection;

        Collider[] hitColliders = Physics.OverlapSphere(impact_point, impact_radius, playerLayer); // get overlap targets
		//sort the distance to increasing order
		Array.Sort (hitColliders, delegate (Collider A, Collider B) {
			Vector3 A_midpoint,B_midpoint;

			A_midpoint = (A.tag!="Shield")? A.transform.position + MECH_MID_POINT : A.transform.position;
			B_midpoint = (B.tag!="Shield")? B.transform.position + MECH_MID_POINT : B.transform.position;

			return (Vector3.Distance(A_midpoint,impact_point) >= Vector3.Distance(B_midpoint,impact_point))? 1 : -1;
		});

		List<int> colliderViewIds = new List<int> ();

		for (int i=0;i < hitColliders.Length;i++)
		{
			//check duplicated
			PhotonView colliderPV = hitColliders [i].transform.root.GetComponent<PhotonView> ();
			if(colliderViewIds.Contains(colliderPV.viewID) || colliderPV.viewID == shooter_viewID) {
				continue;
            } 

            //check team & drone
			if(GameManager.isTeamMode){
				if (other.tag == "Drone" || colliderPV.owner.GetTeam () == shooter_pv.owner.GetTeam ())
					continue;
			}
			colliderViewIds.Add (colliderPV.viewID);

            if (hitColliders [i].tag == "Shield") {//TODO : check if shield overheat

				if (hitColliders [i].transform.root.GetComponent<Combat> ().CurrentHP () - bulletdmg / 2 <= 0) {
					hud.ShowText (cam, hitColliders [i].transform.position, "Kill");
				} else {
					hud.ShowText (cam, hitColliders [i].transform.position, "Defense");
				}
                int hand = hitColliders[i].transform.parent.GetComponent<ShieldUpdater>().GetHand();
                colliderPV.RPC ("ShieldOnHit", PhotonTargets.All, bulletdmg/2, shooter_viewID, hand, "RCL");

                bullet_pv.RPC("CallPlayImpact", PhotonTargets.All, hitColliders[i].transform.position,cam.transform.position, true);
			} else {
				if (hitColliders [i].gameObject.GetComponent<Combat> ().CurrentHP () - bulletdmg <= 0) {
					hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Kill");
				} else {
					hud.ShowText (cam, hitColliders [i].transform.position + new Vector3 (0, 5f, 0), "Hit");
				}
                colliderPV.RPC ("OnHit", PhotonTargets.All, bulletdmg, shooter_viewID, "RCL", true);
                colliderPV.RPC("KnockBack", PhotonTargets.All, transform.forward, 5f);

                bullet_pv.RPC("CallPlayImpact", PhotonTargets.All, hitColliders[i].transform.position + MECH_MID_POINT, cam.transform.position, false);
            }            
		}

        if (colliderViewIds.Count == 0) {//if no target is hit , then play impact on ground
            bullet_pv.RPC("CallPlayImpact", PhotonTargets.All, impact_point, cam.transform.position, false);
        }
    }

	
	[PunRPC]
	void CallPlayImpact(Vector3 collisionHitLoc, Vector3 camPos, bool isShield){
        if (!hasCalledPlayImpact) {
            hasCalledPlayImpact = true;
            ps.Stop(true);
            ps.Clear(true);

            if (PhotonNetwork.isMasterClient)
                Invoke("DestroyThis", 5f);
        }

		GameObject impact = Instantiate ((isShield)? bulletImpact_onShield : bulletImpact, collisionHitLoc, Quaternion.identity);
        impact.transform.LookAt(camPos);
        impact.GetComponent<ParticleSystem> ().Play ();//play bullet impact
	}

    void DestroyThis() {
        PhotonNetwork.Destroy(gameObject);
    }
	
}
