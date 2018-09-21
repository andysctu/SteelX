using System.Collections.Generic;
using UnityEngine;
using System;

public class RocketBullet : Bullet {
    private Rigidbody Rigidbody;
    private PhotonView bulletPv;
    private GameObject shooter;//For checking not hitting shooter
    private LayerMask PlayerLayerMask, TerrainLayerMask;

    //bullet info
    private int bulletDmg = 450, shooterWeapPos;
    private float impact_radius = 6;
    private bool isCollided = false, hasCalledPlayImpact = false;

    protected override void Awake() {
        base.Awake();

        InitComponents();
        InitGameVariables();
	}

    private void InitComponents() {
        bulletPv = GetComponent<PhotonView>();
        AttachRigidbody();
    }

    public override void InitBullet(Camera cam, PhotonView shooter_pv, Vector3 startDirection, Transform target, Weapon shooterWeapon,Weapon targetWeapon){
        base.InitBullet(cam, shooter_pv, startDirection, target, shooterWeapon, targetWeapon);

        Rocket rocket = shooterWeapon as Rocket;
        if (rocket != null){
            SetBulletProperties(rocket.GetRawDamage(), rocket.GetBulletSpeed(), rocket.GetImpactRadius());
        }
    }

    private void InitVelocity() {//Master update position
        Rigidbody.velocity = transform.forward * bulletSpeed;
        transform.LookAt(transform.forward * 9999);
    }

    protected void InitGameVariables() {
        TerrainLayerMask = LayerMask.GetMask("Terrain");
        PlayerLayerMask = LayerMask.GetMask("PlayerLayer");
    }

    private void Start() {
        bullet_ps.Play();

        if(!PhotonNetwork.isMasterClient)return;

        shooter = shooter_pv.gameObject;
        InitVelocity();        
    }

    protected override void LateUpdate(){
    }

    public void SetBulletProperties(int bulletDmg, float bulletSpeed, float impact_radius) {
        this.bulletDmg = bulletDmg;
        this.bulletSpeed = bulletSpeed;
        this.impact_radius = impact_radius;
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

	    if (PhotonNetwork.isMasterClient && ((1 << other.layer) & TerrainLayerMask) != 0) {
	        bulletPv.RPC("PlayImpact", PhotonTargets.All, impact_point);
	    }

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

                bulletPv.RPC("OnHitTarget", PhotonTargets.All, bulletDmg, shooter_pv.viewID, shooterWeapPos, colliderPV.viewID, targetWeapPos);
			} else {
                bulletPv.RPC("OnHitTarget", PhotonTargets.All, bulletDmg, shooter_pv.viewID, shooterWeapPos, colliderPV.viewID,  -1);
            }
        }

        if (colliderViewIds.Count == 0) {//if no target is hit , then play impact on ground
            bulletPv.RPC("PlayImpact", PhotonTargets.All, impact_point);
        }
    }

    [PunRPC]
    private void OnHitTarget(int bulletDmg, int shooterPvID, int shooterWeapPos, int targetPvID, int targetWeapPos){

        PhotonView targetPv = PhotonView.Find(targetPvID), shooterPv = PhotonView.Find(shooterPvID);
        if (targetPv == null) { Debug.LogWarning("Can't find pv"); return; }

        GameObject targetMech = targetPv.gameObject;
        Combat shooterCbt = (shooterPv == null)? null : shooterPv.GetComponent<Combat>();
        targetCbt = targetMech.GetComponent<Combat>();
        if (targetCbt == null){Debug.LogError("target cbt is null");return;}

        targetCbt.OnHit(bulletDmg, shooter_pv.viewID, shooterWeapPos, targetWeapPos);

        if (targetWeapPos != -1) {
            targetWeapon = targetCbt.GetWeapon(targetWeapPos);
            targetWeapon.PlayOnHitEffect();

            if (shooterCbt != null && shooterPv.isMine) {
                if (targetCbt.CurrentHP - targetCbt.ProcessDmg(bulletDmg, Weapon.AttackType.Rocket, targetWeapon) <= 0) {
                    targetMech.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, shooterCbt.GetCamera());
                } else {
                    targetMech.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, shooterCbt.GetCamera());
                }
            }
        } else{
            PlayImpact(targetMech.transform.position + MECH_MID_POINT);

            if (shooterCbt != null && shooterPv.isMine){
                if (targetCbt.CurrentHP - targetCbt.ProcessDmg(bulletDmg, Weapon.AttackType.Rocket, null) <= 0){
                    targetMech.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, shooterCbt.GetCamera());
                } else{
                    targetMech.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, shooterCbt.GetCamera());
                    targetCbt.KnockBack(transform.forward, 5f);
                }
            }
        }
    }

    [PunRPC]
	protected override void PlayImpact(Vector3 collisionHitLoc){
        if (!hasCalledPlayImpact) {
            hasCalledPlayImpact = true;
            Stop();
            if (PhotonNetwork.isMasterClient)Invoke("DestroyThis", 5f);
        }

        GameObject impact = Instantiate(ImpactPrefab.gameObject, collisionHitLoc, Quaternion.identity);
        BulletImpact BI = impact.GetComponent<BulletImpact>();
        BI.Play(collisionHitLoc);
    }

    private void DestroyThis() {
        PhotonNetwork.Destroy(gameObject);
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
