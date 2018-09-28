using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons.Bullets;

public class Skill_Bullet_Controller : MonoBehaviour, RequireSkillInfo {
    //this script controll all the bullets & Muz in skill
    [SerializeField] private int bullet_num = 0;
    [SerializeField] private float interval = 1;

    [Tooltip("Does this bullet follow target?")]
    [SerializeField] private bool onTarget = false;
    [SerializeField] private bool multiTarget = false, onBooster = false, playGunShotSound = false;

    [SerializeField] private bool hasMuz = true;
    private ParticleSystem Muz;

    [Tooltip("Directly play impact on target")]
    [SerializeField] private bool onlyImpact = false;
    [Tooltip("Use the current gun's Bullet")]
    [SerializeField] private bool autoBullet = true;
    [SerializeField] private GameObject Bullet;

    [Tooltip("Choose false if no player animation")]
    [SerializeField] private bool usingSkillCam = true;

    [SerializeField] private bool showHit = true, showHitOnBulletCollision = false, displayKill = false;//displayKill : if the target hp <= 0 , show "kill"  not "hit"

    private Transform target, Effect_End;
    private Transform[] targets;
    private List<Transform> booster_bulletStartTranforms;
    private PhotonView player_pv;
    private Camera cam;
    private MechCombat mechCombat;
    private BuildMech bm;
    private Sounds Sounds;
    private int hand = 0, weaponOffset = 0;
    private bool isWeapPosInit = false;//debug use

    private void Awake() {
        InitComponent();
    }

    private void InitComponent() {
        bm = transform.root.GetComponent<BuildMech>();
        cam = (usingSkillCam) ? transform.root.GetComponent<SkillController>().GetSkillCamera() : transform.root.GetComponent<SkillController>().GetCamera();
        player_pv = transform.root.GetComponent<PhotonView>();
        mechCombat = transform.root.GetComponent<MechCombat>();
        Transform CurrentMech = transform.root.Find("CurrentMech");
        Sounds = CurrentMech.GetComponent<Sounds>();
        Effect_End = (bm.Weapons[weaponOffset+hand] == null) ? null : TransformExtension.FindDeepChild(bm.Weapons[weaponOffset + hand].GetWeapon().transform, "EffectEnd");

        if (onBooster) {
            booster_bulletStartTranforms = new List<Transform>();
            Transform booster_transform = transform.root.GetComponentInChildren<BoosterController>().transform;

            Transform[] booster_childs = booster_transform.GetComponentsInChildren<Transform>();
            foreach (Transform g in booster_childs) {
                if (g.name.Contains("Muz_EffectEnd"))
                    booster_bulletStartTranforms.Add(g.transform);
            }
        }

        if (!onlyImpact) {
            if (!isWeapPosInit && !onBooster) Debug.LogError("WeapPos is not Init before Awake()");

            if (autoBullet)
                FindBulletPrefab();
            else {
                if (Bullet == null) {
                    Debug.LogError("not auto find Bullet but Bullet is not assigned");
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
        Bullet = ((RangedWeaponData)bm.WeaponDatas[weaponOffset + hand]).bulletPrefab;
    }

    private void OnEnable() {
        StartCoroutine(InstantiateBullets());
    }

    private IEnumerator InstantiateBullets() {
        for (int i = 0; i < bullet_num; i++) {
            InstantiateBullet();
            PlayMuz();
            CallPlayShotSound();
            yield return new WaitForSeconds(interval);
        }
    }

    private void InstantiateBullet() {
        if (!onlyImpact) {
            if (multiTarget) {
                if (!onTarget) {
                    GameObject g = Instantiate(Bullet, Effect_End.position, Quaternion.identity);
                    MultiBullets bulletTrace = g.GetComponent<MultiBullets>();
                    //bulletTrace.SetStartDirection(-EffectEnd.transform.right);
                }

                int i = 0;

                foreach (Transform t in targets) {
                    if (onTarget) {
                        GameObject g;
                        if (onBooster && booster_bulletStartTranforms.Count > 0) {
                            g = Instantiate(Bullet, booster_bulletStartTranforms.ToArray()[i % booster_bulletStartTranforms.Count].position, Quaternion.identity);
                            i++;
                        } else {
                            g = Instantiate(Bullet, Effect_End.position, Quaternion.identity);
                        }

                        MultiBullets bulletTrace = g.GetComponent<MultiBullets>();

                        //bulletTrace.SetTarget(t, false);
                        //bulletTrace.interactWithTerrainWhenOnTarget = false;
                        if (showHit)
                            ShowHitMsg(t, bulletTrace);
                    } else {
                        if (showHit)
                            ShowHitMsg(t);
                    }
                }
            } else {
                //not multi-Target
                GameObject g = Instantiate(Bullet, Effect_End.position, Quaternion.identity);
                MultiBullets bulletTrace = g.GetComponent<MultiBullets>();
                //bulletTrace.interactWithTerrainWhenOnTarget = false;
                if (onTarget) {
                    //bulletTrace.SetTarget(target, false);
                } else {
                    //bulletTrace.SetStartDirection(EffectEnd.forward);
                }

                ShowHitMsg(target, bulletTrace);
            }
        } else {
            //only impact
            if (multiTarget) {
                foreach (Transform t in targets) {
                    Instantiate(Bullet, t.position + new Vector3(0, 5, 0), Quaternion.identity, t);

                    ShowHitMsg(t);
                }
            } else {
                GameObject g = Instantiate(Bullet, target.position, Quaternion.identity, target);
                g.transform.localPosition = Vector3.zero;

                ShowHitMsg(target);
            }
        }
    }

    public void SetWeapPos(int hand, int weaponOffset) {
        isWeapPosInit = true;
        this.hand = hand;
        this.weaponOffset = weaponOffset;
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }

    public void SetTargets(Transform[] targets) {
        this.targets = targets;
    }

    private void CallPlayShotSound() {
       // if (playGunShotSound)
            //Sounds.PlayShot(hand);
    }

    private void PlayMuz() {
        if (hasMuz) {
            if (Muz != null) {
                Muz.Play();
            } else {
                Debug.Log("Muz is null");
            }
        }
    }

    private void ShowHitMsg(Transform target, MultiBullets bulletTrace = null) {
        if (!player_pv.isMine)
            return;

        if (!showHitOnBulletCollision) {
            if (displayKill) {
                MechCombat target_mcbt = target.GetComponent<MechCombat>();
                if (target_mcbt == null) {//Drone
                    if (target.GetComponent<DroneCombat>().CurrentHP <= 0) {
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, cam);
                    } else {
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
                    }
                } else {
                    if (target_mcbt.CurrentHP <= 0) {
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, cam);
                    } else {
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
                    }
                }
            } else {
                target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
            }
        } else {
            //bulletTrace.SetShooter(playerPv);
            //bulletTrace.ShowHitOnBulletCollision(displayKill);
            //bulletTrace.SetCamera(cam);
        }
    }
}

public interface RequireSkillInfo {
    void SetWeapPos(int hand, int weaponOffset);
    void SetTarget(Transform target);
    void SetTargets(Transform[] targets);
}