using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour {
    [SerializeField] private GameObject ImpactPrefab;
    private Camera cam;
    private PhotonView shooter_pv;    

    private ParticleSystem bullet_ps;
    private BulletImpact bulletImpact;
    private ParticleSystem.Particle[] particles;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    //Targets components
    private DisplayHitMsg displayHitMsg;
    private Combat cbt;
    private Transform target;
    private Weapon targetWeapon;
    private bool isTargetShield, isfollow = false;

    //Bullet variables
    private float interval = 1, timeOfLastSpawn, bulletSpeed;
    private int maxBulletNum, numParticlesAlive, totalSpawnedBulletNum;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0);

    private void Awake() {
        InitComponents();
    }

    private void InitComponents() {
        bullet_ps = GetComponent<ParticleSystem>();
        bulletImpact = Instantiate(ImpactPrefab).GetComponent<BulletImpact>();
        bulletSpeed = bullet_ps.main.startSpeed.constant;
    }

    public void InitBulletTrace(Camera cam, PhotonView shooter_pv) {
        this.cam = cam;
        this.shooter_pv = shooter_pv;
    }

    public void SetParticleSystem(int maxBulletNum, float interval) {
        this.interval = interval;
        this.maxBulletNum = maxBulletNum;
        particles = new ParticleSystem.Particle[maxBulletNum];
    }

    public void SetTarget(Transform TargetPlayer, Weapon TargetWeapon) {
        this.targetWeapon = TargetWeapon;

        isTargetShield = (TargetWeapon != null && TargetWeapon.IsShield());
        isfollow = (TargetPlayer != null);

        if (TargetPlayer != null) {
            displayHitMsg = TargetPlayer.GetComponent<DisplayHitMsg>();
            cbt = TargetPlayer.GetComponent<Combat>();
        }

        if (isTargetShield) {
            this.target = TargetWeapon.GetWeapon().transform;
        } else {
            this.target = TargetPlayer;
        }
    }

    private void LateUpdate() {
        if (isfollow) {
            if (target == null) { Stop(); return; }
        }

        if (totalSpawnedBulletNum >= maxBulletNum) {
            if (numParticlesAlive == 0) Stop();
        } else {
            if (Time.time - timeOfLastSpawn > interval) {
                transform.LookAt(cam.transform.forward * 9999);
                timeOfLastSpawn = Time.time;
                totalSpawnedBulletNum++;
                bullet_ps.Emit(1);

                //Show hit when the bullet is spawned
                if (isfollow) ShowHitMsg(target);
            }
        }

        numParticlesAlive = bullet_ps.GetParticles(particles);

        if (isfollow) {
            for (int i = 0; i < numParticlesAlive; i++) {
                if (Vector3.Distance(particles[i].position, target.position) <= bulletSpeed * Time.deltaTime) {
                    PlayImpact(particles[i].position);
                    particles[i].remainingLifetime = 0;
                } else {
                    particles[i].velocity = ((isTargetShield) ? (target.position - particles[i].position) : (target.position + MECH_MID_POINT - particles[i].position)).normalized * bulletSpeed;
                }
            }
        }

        bullet_ps.SetParticles(particles, numParticlesAlive);
    }

    public void Play() {
        totalSpawnedBulletNum = 0;
        enabled = true;
    }

    public void Stop() {
        enabled = false;
        Destroy(gameObject, 2);
    }

    private void ShowHitMsg(Transform target) {
        if (shooter_pv.isMine) {
            if (displayHitMsg == null || cbt == null) return;

            if (cbt.CurrentHP <= 0)
                displayHitMsg.Display(DisplayHitMsg.HitMsg.KILL, cam);
            else {
                if (isTargetShield) {
                    displayHitMsg.Display(DisplayHitMsg.HitMsg.DEFENSE, cam);
                } else {
                    displayHitMsg.Display(DisplayHitMsg.HitMsg.HIT, cam);
                }
            }
        }
    }

    private void OnParticleCollision(GameObject other) {
        int numCollisionEvents = bullet_ps.GetCollisionEvents(other, collisionEvents);
        int i = 0;
        while (i < numCollisionEvents) {
            Vector3 collisionHitLoc = collisionEvents[i].intersection;
            bulletImpact.Play(collisionHitLoc);
            i++;
        }
    }

    private void PlayImpact(Vector3 impactPoint) {
        if (target == null) return;

        if (!isTargetShield) {
            bulletImpact.Play(impactPoint);
        } else {
            if (isTargetShield) targetWeapon.PlayOnHitEffect();
            else Debug.LogError("targetWeapon is null");
        }
    }

    private void OnDestroy() {
        if (bulletImpact != null) {
            Destroy(bulletImpact.gameObject);
        }
    }
}