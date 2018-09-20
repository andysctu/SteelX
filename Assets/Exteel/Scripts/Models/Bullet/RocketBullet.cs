using System.Collections.Generic;
using UnityEngine;
using System;

public class RocketBullet : Bullet {
    private Rigidbody Rigidbody;
    private PhotonView bullet_pv;
    private GameObject shooter;//For checking not hitting shooter
    private LayerMask PlayerLayerMask, TerrainLayerMask;

    //bullet info
    private int bulletdmg = 450, shooterWeapPos;
    private float impact_radius = 6;
    private bool isCollided = false, hasCalledPlayImpact = false;
    private int SPincreaseAmount=0;

    protected override void Awake() {
        base.Awake();

        InitComponents();
        InitGameVariables();
	}

    private void InitComponents() {
        bullet_pv = GetComponent<PhotonView>();
        AttachRigidbody();
    }

    private void InitVelocity() {//Master update position
        Rigidbody.velocity = startDirection * bulletSpeed;
        transform.LookAt(startDirection * 9999);
    }

    protected void InitGameVariables() {
        TerrainLayerMask = LayerMask.GetMask("Terrain");
        PlayerLayerMask = LayerMask.GetMask("PlayerLayer");
    }

    private void Start() {
        bullet_ps.Play();

        if (!PhotonNetwork.isMasterClient)return;

        shooter = shooter_pv.gameObject;
        InitVelocity();        
    }

    public void SetBulletPropertis(int bulletdmg, int bulletSpeed, int impact_radius) {
        this.bulletdmg = bulletdmg;
        this.bulletSpeed = bulletSpeed;
        this.impact_radius = impact_radius;
    }

    public void SetSPIncreaseAmount(int amount) {
        SPincreaseAmount = amount;
    }

	protected override void OnParticleCollision(GameObject other){//Master do the logic & destroy
        if(!PhotonNetwork.isMasterClient)return;

		if (isCollided || shooter_pv==null || other == shooter)return;
		
		if(GameManager.isTeamMode){
			if (((1<<other.layer)&PlayerLayerMask) != 0) {//check if other.layer equals player layer
				if (other.transform.root.GetComponent<PhotonView> ().owner.GetTeam () == shooter_pv.owner.GetTeam ())return;
			}
		}	

        isCollided = true;

        bullet_ps.GetCollisionEvents(other, collisionEvents);
        Vector3 impact_point = collisionEvents[0].intersection;

        Collider[] hitColliders = Physics.OverlapSphere(impact_point, impact_radius, PlayerLayerMask); // get overlap targets

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
			if(colliderViewIds.Contains(colliderPV.viewID) || colliderPV.viewID == shooter_pv.viewID) {
				continue;
            } 

            //check team & drone
			if(GameManager.isTeamMode){
				if (other.tag == "Drone" || colliderPV.owner.GetTeam () == shooter_pv.owner.GetTeam ())
					continue;
			}
			colliderViewIds.Add (colliderPV.viewID);

            if (hitColliders [i].tag == "Shield") {				
                ShieldActionReceiver shieldUpdater = hitColliders[i].transform.parent.GetComponent<ShieldActionReceiver>();
                int targetWeapPos = shieldUpdater.GetPos();
                
                colliderPV.RPC ("OnHit", PhotonTargets.All, bulletdmg, shooter_pv.owner, shooter_pv.viewID, shooterWeapPos, targetWeapPos);
                bullet_pv.RPC("CallPlayImpact", PhotonTargets.All, bulletdmg, hitColliders[i].transform.position, colliderPV.owner, colliderPV.viewID, targetWeapPos, true);
			} else {			
                colliderPV.RPC("OnHit", PhotonTargets.All, bulletdmg, shooter_pv.owner, shooter_pv.viewID, shooterWeapPos, -1);
                bullet_pv.RPC("CallPlayImpact", PhotonTargets.All, bulletdmg, hitColliders[i].transform.position + MECH_MID_POINT, colliderPV.owner, colliderPV.viewID, -1, false);
            }

            //increase SP
            shooter.GetComponent<SkillController>().IncreaseSP(SPincreaseAmount);
        }

        if (colliderViewIds.Count == 0) {//if no target is hit , then play impact on ground
            bullet_pv.RPC("CallPlayImpact", PhotonTargets.All, impact_point, cam.transform.position, false);
        }
    }

    [PunRPC]
	private void OnHitTarget(Vector3 direction, int target_pvIDs) {

        if (PhotonNetwork.isMasterClient) {

        }

    }
    //if (hitColliders [i].transform.root.GetComponent<Combat> ().CurrentHP - bulletdmg / 2 <= 0) {
    //                colliderPV.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, shooter_cam);
    //} else {
    //                colliderPV.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, shooter_cam);
    //}

    [PunRPC]
	void CallPlayImpact(int bulletdmg, Vector3 collisionHitLoc, PhotonPlayer target, int target_pvID,int targetWeapPos, bool isShield){
        if (!hasCalledPlayImpact) {
            hasCalledPlayImpact = true;
            Stop();
            if (PhotonNetwork.isMasterClient)Invoke("DestroyThis", 3f);
        }

        
    //     if (hitColliders [i].gameObject.GetComponent<Combat> ().CurrentHP - bulletdmg <= 0) {
    //                colliderPV.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, cam);
				//} else {
    //                colliderPV.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
				//}
                

        //colliderPV.RPC("KnockBack", PhotonTargets.All, transform.forward, 5f);
        if (isShield) {

        } else {
            GameObject impact = Instantiate(ImpactPrefab.gameObject, collisionHitLoc, Quaternion.identity);
            BulletImpact BI = impact.GetComponent<BulletImpact>();
            BI.Play(collisionHitLoc);
        }
        
    }

    void DestroyThis() {
        PhotonNetwork.Destroy(gameObject);
    }

    protected override void LateUpdate() {

    }

    public override void Play() {
        if(isfollow) {
            if (target == null) return;
            transform.LookAt(target);
            bullet_ps.Play();
        } else {
            transform.LookAt(startDirection * 9999);
            Rigidbody.velocity = startDirection * bulletSpeed;
            bullet_ps.Play();
        }
    }

    private void AttachRigidbody() {
        Rigidbody = gameObject.AddComponent<Rigidbody>();
        Rigidbody.useGravity = false;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public override void Stop() {
        bullet_ps.Stop(true);
        Destroy(gameObject);
        enabled = false;
    }
}
