using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour {
    [SerializeField] private GameObject bulletImpact_onShield, bulletImpact;
    [SerializeField] private LayerMask Terrain;

    [Tooltip("Does this bullet go straight?")]
    [SerializeField] private bool go_straight = true;

    private ParticleSystem ps;
    private Camera cam;
    private Rigidbody rb;
    private PhotonView shooter_pv;
    private Transform target, startTransform;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0), startDirection, dir, destination;

    private bool isTargetShield;
    private bool isfollow = false;
    private float bulletSpeed = 350, otherDirSpeed = 80;
    private bool isCollided = false;
    private bool hasSlowdown = false, showHitOnBulletCollision = false, displayKill = false;

    void Start() {
        initComponents();
        initVelocity();
        ps.Play();
        Destroy(gameObject, 2f);
    }

    void initComponents() {
        ps = GetComponent<ParticleSystem>();
        rb = GetComponent<Rigidbody>();
    }

    void initVelocity() {
        if (target == null) {//no target => move directly
            GetComponent<Rigidbody>().velocity = startDirection * bulletSpeed;
            transform.LookAt(startDirection * 9999);
        } else {//target exists
            transform.LookAt(target);
        }
    }

    public void SetSpeed(float speed) {
        bulletSpeed = speed;
    }

    public void SetStartDirection(Vector3 startDirection) {
        this.startDirection = startDirection.normalized;
    }

    //this is for multi-target skill
    public void SetStartTransform(Transform startTransform) {
        this.startTransform = startTransform;
    }

    public void SetShooter(PhotonView shooter_pv) {
        this.shooter_pv = shooter_pv;
    }

    public void SetTarget(Transform target, bool isTargetShield) {
        this.target = target;
        this.isTargetShield = isTargetShield;
        isfollow = (target != null);
    }

    public void SetCamera(Camera cam) {
        this.cam = cam;
    }

    public void ShowHitOnBulletCollision(bool displayKill) {
        showHitOnBulletCollision = true;
        this.displayKill = displayKill;
    }

    void Update() {
        if (!isfollow) {
            return;
        } else {
            if (!isCollided) {
                if (isTargetShield) {
                    dir = (target.position - transform.position).normalized;
                    transform.LookAt(target.position);
                    destination = target.position;
                } else {
                    dir = (target.position + MECH_MID_POINT - transform.position).normalized;
                    transform.LookAt(target.position + MECH_MID_POINT);
                    destination = target.position + MECH_MID_POINT;
                }

                if (Vector3.Distance(transform.position, destination) < bulletSpeed * Time.deltaTime) {
                    isCollided = true;
                    PlayImpact(transform.position);
                    ps.Stop();
                    ps.Clear();

                    //show hit msg
                    if (showHitOnBulletCollision && shooter_pv.isMine) {
                        MechCombat mcbt = target.transform.root.GetComponent<MechCombat>();
                        if(mcbt == null) {
                            //drone
                            if(target.transform.root.GetComponent<DroneCombat>().CurrentHP() <= 0)
                                target.transform.root.GetComponent<HUD>().DisplayKill(cam);
                            else
                                target.transform.root.GetComponent<HUD>().DisplayHit(cam);
                        } else {
                            if (mcbt.CurrentHP() <= 0)
                                target.transform.root.GetComponent<HUD>().DisplayKill(cam);
                            else
                                target.transform.root.GetComponent<HUD>().DisplayHit(cam);
                        }
                    }

                    Destroy(gameObject);
                }
                if (go_straight)
                    rb.velocity = bulletSpeed * dir;
                else {
                    rb.velocity = bulletSpeed * dir + Vector3.up * otherDirSpeed;
                    otherDirSpeed -= Time.deltaTime * 100;
                }
            }
        }
    }

    void OnParticleCollision(GameObject other) {
        if (isCollided)
            return;

        isCollided = true;
        PlayImpact(transform.position);
        Destroy(gameObject);
    }

    void PlayImpact(Vector3 impactPoint) {
        GameObject impact;
        Transform bulletCollector = transform.parent;
        if (!isTargetShield) {
            impact = Instantiate(bulletImpact, impactPoint, Quaternion.identity, bulletCollector);
        } else {
            impact = Instantiate(bulletImpact_onShield, target.position - target.forward * 2f, Quaternion.identity, bulletCollector);
            impact.transform.rotation = Quaternion.LookRotation(target.transform.forward);
        }
        impact.GetComponent<ParticleSystem>().Play();
    }
}
