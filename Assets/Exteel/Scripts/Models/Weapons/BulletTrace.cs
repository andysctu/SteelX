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
    private Transform target;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0), startDirection, dir, destination;

    private bool isTargetShield;
    private bool isfollow = false;
    [SerializeField]private float bulletSpeed = 350, otherDirSpeed = 80;
    private bool isCollided = false;
    private bool showHitOnBulletCollision = false, displayKill = false;

    [HideInInspector]public bool interactWithTerrainWhenOnTarget = true;

    void Start() {
        initComponents();
        initVelocity();
        if(ps!=null)ps.Play();
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

    public void SetStartDirection(Vector3 startDirection) {
        this.startDirection = startDirection.normalized;
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
            if(target == null) {//target exit game
                Destroy(this);
                return;
            }


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

                if (go_straight)
                    rb.velocity = bulletSpeed * dir;
                else {
                    //TODO : improve this
                    rb.velocity = bulletSpeed * dir + Vector3.up * otherDirSpeed;
                    otherDirSpeed -= Time.deltaTime * 100;
                }

                if (Vector3.Distance(transform.position, destination) < bulletSpeed * Time.deltaTime) {
                    isCollided = true;
                    PlayImpact(transform.position);
                    if (ps != null) {
                        ps.Stop();
                        ps.Clear();
                    }
                    rb.velocity = Vector3.zero;
                    //show hit msg
                    ShowHitMsg(target);
                }
                
            }
        }
    }

    void ShowHitMsg(Transform target) {
        //show hit msg
        if (showHitOnBulletCollision && shooter_pv.isMine) {
            MechCombat mcbt = target.transform.root.GetComponent<MechCombat>();
            if (mcbt == null) {
                //drone
                if (target.transform.root.GetComponent<DroneCombat>().CurrentHP <= 0)
                    target.transform.root.GetComponent<HUD>().DisplayKill(cam);
                else
                    target.transform.root.GetComponent<HUD>().DisplayHit(cam);
            } else {
                if (mcbt.CurrentHP <= 0)
                    target.transform.root.GetComponent<HUD>().DisplayKill(cam);
                else
                    target.transform.root.GetComponent<HUD>().DisplayHit(cam);
            }
        }
    }

    void OnParticleCollision(GameObject other) {
        if (isCollided || !interactWithTerrainWhenOnTarget)
            return;

        isCollided = true;
        PlayImpact(transform.position);
        Destroy(gameObject);
    }

    void PlayImpact(Vector3 impactPoint) {
        GameObject impact = null;
        Transform bulletCollector = transform.parent;
        if (!isTargetShield) {
            impact = Instantiate(bulletImpact, impactPoint, Quaternion.identity, bulletCollector);

            impact.GetComponent<ParticleSystem>().Play();
        } else {
            if(target == null)return;

            ParticleSystem ps_onShield = target.GetComponent<ParticleSystem>();            
            if (ps_onShield != null)//ps_onShield sometimes is null ( target respawn too fast so the shield get destroyed first )
                ps_onShield.Play();
        }
    }
}
