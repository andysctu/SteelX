using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour {
    [SerializeField] private GameObject bulletImpact_onShield, bulletImpact;
    [SerializeField] private LayerMask Terrain;
    private ParticleSystem ps;
    private Rigidbody rb;
    private Camera cam;
    private Transform target;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0), dir, destination;

    private string ShooterName;
    private bool isTargetShield;
    private bool isfollow = false;
    private float bulletSpeed = 350;
    private bool isCollided = false;
    private bool hasSlowdown = false;

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
            GetComponent<Rigidbody>().velocity = cam.transform.forward * bulletSpeed;
            transform.LookAt(cam.transform.forward * 9999);
        } else {//target exists
            transform.LookAt(target);
        }
    }

    public void SetShooterName(string name) {
        ShooterName = name;
    }

    public void SetCamera(Camera cam) {
        this.cam = cam;
    }

    public void SetTarget(Transform target, bool isTargetShield) {
        this.target = target;
        this.isTargetShield = isTargetShield;
        isfollow = (target != null);
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
                    Destroy(gameObject);
                }
                rb.velocity = bulletSpeed * dir;
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
