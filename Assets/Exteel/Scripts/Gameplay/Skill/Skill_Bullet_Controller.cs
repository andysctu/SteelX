using System.Collections;
using UnityEngine;

public class Skill_Bullet_Controller : MonoBehaviour, RequireSkillInfo {
    //this script controll all the bullets & Muz in skill

    [SerializeField] private int bullet_num = 0;
    [SerializeField] private float interval = 1;

    [Tooltip("Does this bullet follow target?")]
    [SerializeField] private bool onTarget = false;
    [SerializeField] private bool multiTarget = false, playGunShotSound = false;

    //Muz
    [SerializeField] private bool hasMuz = true;
    private ParticleSystem Muz;

    [Tooltip("Directly play impact on target")]
    [SerializeField] private bool onlyImpact = false;
    [Tooltip("Use the current gun's Bullet")]
    [SerializeField] private bool autoBullet = true;
    [SerializeField] private GameObject Bullet;

    [Tooltip("Choose false if no player animation")]
    [SerializeField]private bool usingSkillCam = true;

    [SerializeField]private bool showHit = true, showHitOnBulletCollision = false, displayKill = false;//displayKill : if the target hp <= 0 , show "kill"  not "hit"

    private Transform target, bulletStart, Effect_End;
    private Transform[] targets;
    private PhotonView player_pv;
    private Camera cam;
    private MechCombat mechCombat;
    private BuildMech bm;
    private Sounds Sounds;
    private int hand = 0, weaponOffset = 0;
    private bool isWeapPosInit = false;//debug use

    void Awake() {
        bm = transform.root.GetComponent<BuildMech>();
        cam = (usingSkillCam)? transform.root.GetComponentInChildren<SkillCam>().GetComponent<Camera>() : transform.root.GetComponent<SkillController>().GetCamera();
        player_pv = transform.root.GetComponent<PhotonView>();
        mechCombat = transform.root.GetComponent<MechCombat>();
        Transform CurrentMech = transform.root.Find("CurrentMech");
        Sounds = CurrentMech.GetComponent<Sounds>();
        Effect_End = mechCombat.GetEffectEnd(weaponOffset + hand);

        if (!onlyImpact) {
            if (!isWeapPosInit) Debug.LogError("WeapPos is not Init before Awake()");
            if(autoBullet)
                FindBulletPrefab();
            else {
                if(Bullet == null) {
                    Debug.LogError("not auto find Bullet but Bullet is null");
                }
            }
        }
        if (hasMuz) FindMuz();
    }

    private void FindMuz() {
        Transform Muz_transform = Effect_End.Find("Muz");
        if (Muz_transform != null) {
            Muz = Muz_transform.GetComponent<ParticleSystem>();
        } else {
            Debug.Log("Can't find Muz");
        }
    }

    private void FindBulletPrefab() {
        Bullet = ((RangedWeapon)bm.weaponScripts[weaponOffset + hand]).bulletPrefab;
    }

    private void OnEnable() {
        StartCoroutine(InstantiateBullets());
    }

    IEnumerator InstantiateBullets() {
        for(int i = 0; i < bullet_num; i++) {
            InstantiateBullet(i==0);
            yield return new WaitForSeconds(interval);
        }
    }

    private void InstantiateBullet(bool is_the_first_call) {
        //debug
        if (bulletStart == null && Effect_End == null) {
            Debug.LogError("Can't have bulletStart and gun_End both null.");
            return;
        }

        if (!onlyImpact) {
            if (multiTarget) {
                foreach(Transform t in targets) {
                    GameObject g = Instantiate(Bullet, (bulletStart == null) ? Effect_End.position : bulletStart.position, Quaternion.identity);
                    BulletTrace bulletTrace = g.GetComponent<BulletTrace>();

                    if(bulletTrace == null) {
                        Debug.LogError("can't find bulletTrace.");
                        return;
                    } else {
                        if (onTarget) {
                            bulletTrace.SetTarget(t, false);
                            bulletTrace.SetStartTransform((bulletStart == null) ? Effect_End : bulletStart);
                        } else {
                            bulletTrace.SetStartDirection(Effect_End.forward);
                        }
                    }

                    //show hit msg
                    if (showHit && player_pv.isMine) {
                        if (!showHitOnBulletCollision) {
                            if (displayKill) {
                                MechCombat target_mcbt = t.GetComponent<MechCombat>();
                                if (target_mcbt == null) {
                                    if (t.GetComponent<DroneCombat>().CurrentHP() <= 0) {
                                        if(is_the_first_call) t.GetComponent<HUD>().DisplayKill(cam);
                                    } else {
                                        t.GetComponent<HUD>().DisplayHit(cam);
                                    }
                                } else {
                                    if (target_mcbt.CurrentHP() <= 0) {
                                        if (is_the_first_call) t.GetComponent<HUD>().DisplayKill(cam);
                                    } else {
                                        t.GetComponent<HUD>().DisplayHit(cam);
                                    }
                                }
                            } else {
                                t.GetComponent<HUD>().DisplayHit(cam);                                
                            }
                        } else {
                            bulletTrace.ShowHitOnBulletCollision(displayKill);
                        }
                    }
                }
            } else {
                //not multi-Target
                GameObject g = Instantiate(Bullet, (bulletStart == null) ? Effect_End.position : bulletStart.position, Quaternion.identity);
                BulletTrace bulletTrace = g.GetComponent<BulletTrace>();

                if (bulletTrace == null) {
                    Debug.LogError("can't find bulletTrace.");
                    return;
                } else {
                    if (onTarget) {
                        bulletTrace.SetTarget(target, false);
                    } else {
                        bulletTrace.SetStartDirection(Effect_End.forward);
                    }
                }
                //show hit msg
                if (showHit && player_pv.isMine) {
                    if (!showHitOnBulletCollision) {
                        if (displayKill) {
                            MechCombat target_mcbt = target.GetComponent<MechCombat>();
                            if (target_mcbt == null) {
                                if (target.GetComponent<DroneCombat>().CurrentHP() <= 0) {
                                    if (is_the_first_call) target.GetComponent<HUD>().DisplayKill(cam);
                                } else {
                                    target.GetComponent<HUD>().DisplayHit(cam);
                                }
                            } else {
                                if (target_mcbt.CurrentHP() <= 0) {
                                    if (is_the_first_call) target.GetComponent<HUD>().DisplayKill(cam);
                                } else {
                                    target.GetComponent<HUD>().DisplayHit(cam);
                                }
                            }
                        } else {
                            target.GetComponent<HUD>().DisplayHit(cam);
                        }
                    } else {
                        bulletTrace.ShowHitOnBulletCollision(displayKill);
                        bulletTrace.SetCamera(cam);
                    }
                }
            }

        } else {
            //only impact
            if (multiTarget) {
                foreach(Transform t in targets) {
                    GameObject g = Instantiate(Bullet, t.position + new Vector3(0,5,0), Quaternion.identity, t);

                    //show hit msg
                    if (showHit && player_pv.isMine) {
                        if (displayKill) {
                            MechCombat target_mcbt = t.GetComponent<MechCombat>();
                            if (target_mcbt == null) {
                                if (t.GetComponent<DroneCombat>().CurrentHP() <= 0) {
                                    if (is_the_first_call) t.GetComponent<HUD>().DisplayKill(cam);
                                } else {
                                    t.GetComponent<HUD>().DisplayHit(cam);
                                }
                            } else {
                                if (target_mcbt.CurrentHP() <= 0) {
                                    if (is_the_first_call) t.GetComponent<HUD>().DisplayKill(cam);
                                } else {
                                    t.GetComponent<HUD>().DisplayHit(cam);
                                }
                            }
                        } else {
                            t.GetComponent<HUD>().DisplayHit(cam);
                        }
                    }
                }
            } else {
                GameObject g = Instantiate(Bullet, target.position, Quaternion.identity, target);
                g.transform.localPosition = Vector3.zero;

                //show hit msg
                if (showHit && player_pv.isMine) {
                    if (displayKill) {
                        MechCombat target_mcbt = target.GetComponent<MechCombat>();
                        if (target_mcbt == null) {
                            if (target.GetComponent<DroneCombat>().CurrentHP() <= 0) {
                                if (is_the_first_call) target.GetComponent<HUD>().DisplayKill(cam);
                            } else {
                                target.GetComponent<HUD>().DisplayHit(cam);
                            }
                        } else {
                            if (target_mcbt.CurrentHP() <= 0) {
                                if (is_the_first_call) target.GetComponent<HUD>().DisplayKill(cam);
                            } else {
                                target.GetComponent<HUD>().DisplayHit(cam);
                            }
                        }
                    } else {
                        target.GetComponent<HUD>().DisplayHit(cam);
                    }
                }
            }
        }


        //play Muz & sound
        if (hasMuz) {
            if(Muz != null) {
                Muz.Play();
            } else {
                Debug.Log("Muz is null");
            }
        }
        
        if(playGunShotSound)
            Sounds.PlayShot(hand);
    }

    public void SetWeapPos(int hand, int weaponOffset) {
        isWeapPosInit = true;
        this.hand = hand;
        this.weaponOffset = weaponOffset;
    }

    //this is called when casting skill
    public void SetTarget(Transform target) {
        Debug.Log("called set target : "+gameObject.name);
        this.target = target;
    }

    public void SetTargets(Transform[] targets) {
        this.targets = targets;
    }

    public void AssignBulletStart(Transform bulletStart) {//if not assigned = > use gun's end
        this.bulletStart = bulletStart;
    }
}

public interface RequireSkillInfo {
    void SetWeapPos(int hand, int weaponOffset);
    void SetTarget(Transform target);
    void SetTargets(Transform[] targets);
    void AssignBulletStart(Transform bulletStart);
}
