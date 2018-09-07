using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Crosshair : MonoBehaviour {
    [SerializeField] private BuildMech bm;
    [SerializeField] private PhotonView pv;
    [SerializeField] private GameObject LockedImg;
    [SerializeField] private CrosshairImage crosshairImage;
    [SerializeField] private LayerMask playerlayer, Terrainlayer;
    [SerializeField] private Sounds Sounds;
    [SerializeField] private MechCombat MechCombat;
    [SerializeField] private SkillController SkillController;

    private List<GameObject> TargetsToRemove = new List<GameObject>();
    public List<GameObject> Targets = new List<GameObject>();//control by checkisrendered.cs

    private WeaponData[] weaponScripts;
    private Transform targetL, targetR;
    private Camera cam;
    private Coroutine coroutine = null;
    //private Vector2 CamMidpoint = new Vector2(0.5f, 0.5f);

    private const float SendMsgDeltaTime = 0.3f; //If the target is the same, this is the time between two msgs.
    private float screenCoeff;
    private float TimeOfLastSend;
    private float CrosshairRadiusL, CrosshairRadiusR;
    private int LastLockTargetID = 0, weaponOffset = 0, Marksmanship = 0;//avoid sending lock message too often
    private bool LockL = false, LockR = false, foundTargetL = false, foundTargetR = false;
    private bool isOnLocked = false, onSkill = false;
    private bool isTeamMode, isTargetAllyL = false, isTargetAllyR = false;
    private bool isRocket = false, isRectifier_L = false, isRectifier_R = false;
    private const float LockedMsgDuration = 0.5f;//when receiving a lock message , the time it'll last
    public const float CAM_DISTANCE_TO_MECH = 15f;

    private float DistanceCoeff = 0.008f;
    private float MaxDistanceL, MaxDistanceR, MinDistanceL, MinDistanceR;

    private void Awake() {
        RegisterOnWeaponBuilt();
        RegisterOnWeaponSwitched();
        RegisterOnMechEnabled();
        RegisterOnSkill();
    }

    private void RegisterOnWeaponBuilt() {
        bm.OnMechBuilt += OnMechBuilt;
    }

    private void RegisterOnWeaponSwitched() {
        MechCombat.OnWeaponSwitched += UpdateCrosshair;
    }

    private void RegisterOnMechEnabled() {
        MechCombat.OnMechEnabled += EnableCrosshair;
    }


    private void RegisterOnSkill() {
        if (SkillController != null) SkillController.OnSkill += OnSkill;
    }

    private void Start() {
        if (!pv.isMine) {//Enable local
            enabled = false;
            crosshairImage.gameObject.SetActive(false);
            return;
        }

        GetGameVars();
        initComponent();
        UpdateCrosshair();
    }

    private void GetGameVars() {
        screenCoeff = (float)Screen.height / Screen.width;
        isTeamMode = GameManager.isTeamMode;
    }

    private void initComponent() {
        cam = GetComponent<Camera>();
    }

    private void OnMechBuilt() {
        weaponScripts = bm.WeaponDatas;
        Marksmanship = bm.MechProperty.Marksmanship;
    }

    public void UpdateCrosshair() {
        weaponOffset = MechCombat.GetCurrentWeaponOffset();

        if (weaponScripts[weaponOffset] == null) {
            CrosshairRadiusL = 0;
            MaxDistanceL = 0;
            MinDistanceL = 0;
        } else {
            CrosshairRadiusL = weaponScripts[weaponOffset].radius * (1 + Marksmanship / 100.0f);
            MaxDistanceL = weaponScripts[weaponOffset].Range;
            MinDistanceL = weaponScripts[weaponOffset].minRange;
        }

        if (weaponScripts[weaponOffset + 1] == null) {
            CrosshairRadiusR = 0;
            MaxDistanceR = 0;
            MinDistanceR = 0;
        } else {
            CrosshairRadiusR = weaponScripts[weaponOffset + 1].radius * (1 + Marksmanship / 100.0f);
            MaxDistanceR = weaponScripts[weaponOffset + 1].Range;
            MinDistanceR = weaponScripts[weaponOffset + 1].minRange;
        }

        isRectifier_L = (weaponScripts[weaponOffset] != null && weaponScripts[weaponOffset].GetWeaponType() == typeof(Rectifier));
        isRectifier_R = (weaponScripts[weaponOffset + 1] != null && weaponScripts[weaponOffset + 1].GetWeaponType() == typeof(Rectifier));
        isRocket = (weaponScripts[weaponOffset] != null && weaponScripts[weaponOffset].GetWeaponType() == typeof(Rocket));

        isTargetAllyL = isRectifier_L;
        isTargetAllyR = isRectifier_R;

        crosshairImage.SetRadius(CrosshairRadiusL, CrosshairRadiusR);

        //first turn all off
        crosshairImage.CloseAllCrosshairs_L();

        if (CrosshairRadiusL != 0) {
            if (isRocket) {
                crosshairImage.SetCurrentLImage((int)Ctype.RCL_0);
            } else if (!isRectifier_L) {//ENG does not have crosshair
                crosshairImage.SetCurrentLImage((int)Ctype.N_L0);
            } else {
                crosshairImage.SetCurrentLImage((int)Ctype.ENG);
            }
        }

        crosshairImage.CloseAllCrosshairs_R();

        if (CrosshairRadiusR != 0) {
            if (!isRectifier_R) {
                crosshairImage.SetCurrentRImage((int)Ctype.N_R0);
            } else {
                crosshairImage.SetCurrentRImage((int)Ctype.ENG);
            }
        }

        targetL = null;
        targetR = null;

        //enable middle cross
        if (isRectifier_L && isRectifier_R)
            crosshairImage.middlecross.enabled = false;
        else
            crosshairImage.middlecross.enabled = true;

        crosshairImage.targetMark.gameObject.SetActive(false); //targetMark has a children
        crosshairImage.EngTargetMark.enabled = false;
    }

    public Transform DectectTarget(float crosshairRadius, float range, float minimunRange, bool isTargetAlly) {
        if (crosshairRadius > 0) {
            foreach (GameObject target in Targets) {
                if (target == null) {
                    TargetsToRemove.Add(target);
                    continue;
                }

                if(target.layer == 0)//onskill
                    continue;

                PhotonView targetpv = target.GetComponent<PhotonView>();
                if (targetpv.viewID == pv.viewID)
                    continue;

                if (isTeamMode) {
                    if (!isTargetAlly) {
                        if (targetpv.owner.GetTeam() == pv.owner.GetTeam()) {
                            continue;
                        }
                    } else {
                        if (targetpv.owner.GetTeam() != pv.owner.GetTeam()) {
                            continue;
                        }
                    }
                } else {
                    //if not team mode , ignore eng
                    if (isTargetAlly)
                        continue;
                }

                //check distance
                if (Vector3.Distance(target.transform.position, transform.root.position) > range || Vector3.Distance(target.transform.position, transform.root.position) < minimunRange)
                    continue;

                Vector3 targetLocInCam = cam.WorldToViewportPoint(target.transform.position + new Vector3(0, 5, 0));
                Vector3 rayStartPoint = transform.root.position + new Vector3(0, 5, 0); //rayStartpoint should not inside terrain => not detect
                Vector2 targetLocOnScreen = new Vector2(targetLocInCam.x, (targetLocInCam.y - 0.5f) * screenCoeff + 0.5f);
                if (Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * crosshairRadius && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * crosshairRadius) {
                    //check if Terrain block the way
                    RaycastHit hit;
                    if (Physics.Raycast(rayStartPoint, (target.transform.position + new Vector3(0, 5, 0) - rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, target.transform.position + new Vector3(0, 5, 0)), Terrainlayer)) {
                        if (hit.collider.gameObject.layer == 10) {
                            continue;
                        }
                    }

                    if (!isTargetAlly)
                        SendLockedMessage(targetpv.viewID, target.name);

                    return target.transform;
                }
            }
        }
        return null;
    }

    public Transform[] DectectMultiTargets(float crosshairRadius, float range, bool isTargetAlly) {
        if (crosshairRadius > 0) {
            List<Transform> targets_in_range = new List<Transform>();

            foreach (GameObject target in Targets) {
                if (target == null) {
                    TargetsToRemove.Add(target);
                    continue;
                }

                if (target.layer == 0)//onskill
                    continue;

                PhotonView targetpv = target.GetComponent<PhotonView>();
                if (targetpv.viewID == pv.viewID)
                    continue;

                if (isTeamMode) {
                    if (target.GetComponent<Collider>().tag == "Drone") {
                        continue;
                    }
                    if (!isTargetAlly) {
                        if (targetpv.owner.GetTeam() == pv.owner.GetTeam()) {
                            continue;
                        }
                    } else {
                        if (targetpv.owner.GetTeam() != pv.owner.GetTeam()) {
                            continue;
                        }
                    }
                } else {
                    //if not team mode , ignore eng
                    if (isTargetAlly)
                        continue;
                }

                //check distance
                if (Vector3.Distance(target.transform.position, transform.root.position) > range)
                    continue;

                Vector3 targetLocInCam = cam.WorldToViewportPoint(target.transform.position + new Vector3(0, 5, 0));
                Vector3 rayStartPoint = transform.root.position + new Vector3(0, 10, 0); //rayStartpoint should not inside terrain => not detect
                Vector2 targetLocOnScreen = new Vector2(targetLocInCam.x, (targetLocInCam.y - 0.5f) * screenCoeff + 0.5f);
                if (Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * crosshairRadius && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * crosshairRadius) {
                    //check if Terrain block the way
                    RaycastHit hit;
                    if (Physics.Raycast(rayStartPoint, (target.transform.position + new Vector3(0, 5, 0) - rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, target.transform.position + new Vector3(0, 5, 0)), Terrainlayer)) {
                        if (hit.collider.gameObject.layer == 10) {
                            continue;
                        }
                    }
                    targets_in_range.Add(target.transform);
                }
            }
            return targets_in_range.ToArray();
        }
        return null;
    }

    private void Update() {
        if (onSkill) return;

        if (CrosshairRadiusL > 0) {// TODO : remove this
            if ((targetL = DectectTarget(CrosshairRadiusL, MaxDistanceL, MinDistanceL, isTargetAllyL)) != null) {
                crosshairImage.OnTargetL(true);

                if (!LockL) {
                    Sounds.PlayLock();
                    LockL = true;
                }
                foundTargetL = true;
            }
            if (!foundTargetL) {
                crosshairImage.OnTargetL(false);
                targetL = null;
                LockL = false;
            } else {
                foundTargetL = false;
            }
        }
        if (CrosshairRadiusR > 0) {
            if ((targetR = DectectTarget(CrosshairRadiusR, MaxDistanceR, MinDistanceR, isTargetAllyR)) != null) {
                crosshairImage.OnTargetR(true);

                if (!LockR) {
                    Sounds.PlayLock();
                    LockR = true;
                }
                foundTargetR = true;
            }
            if (!foundTargetR) {
                crosshairImage.OnTargetR(false);
                targetR = null;
                LockR = false;
            } else {
                foundTargetR = false;
            }
        }

        foreach (GameObject g in TargetsToRemove) {//remove null target
            Targets.Remove(g);
        }
        TargetsToRemove.Clear();

        MarkTarget();
    }

    //TODO : improve detection & generalize the CAM_DISTANCE_TO_MECH
    public Transform getCurrentTargetL() {
        if (isRocket)
            return null;

        if (targetL != null && !isTargetAllyL) {
            //cast a ray to check if hitting shield
            //Debug.DrawRay (cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), ((targetL.transform.root.position + new Vector3 (0, 5, 0)) - cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH))*100f, Color.red, 3f);
            RaycastHit[] hitpoints;
            hitpoints = Physics.RaycastAll(cam.transform.TransformPoint(0, 0, CAM_DISTANCE_TO_MECH), (targetL.transform.root.position + new Vector3(0, 5, 0)) - cam.transform.TransformPoint(0, 0, CAM_DISTANCE_TO_MECH), MaxDistanceL, playerlayer).OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in hitpoints) {
                if (isTeamMode) {
                    PhotonView targetpv = hit.transform.root.GetComponent<PhotonView>();
                    if (targetpv.owner.GetTeam() != pv.owner.GetTeam()) {
                        return hit.collider.transform;
                    }
                } else {
                    PhotonView targetpv = hit.transform.root.GetComponent<PhotonView>();
                    if (targetpv.viewID != pv.viewID) {
                        return hit.collider.transform;
                    }
                }
            }
        }
        return targetL;
    }

    public Transform getCurrentTargetR() {
        if (isRocket)
            return null;

        if (targetR != null && !isTargetAllyR) {
            //cast a ray to check if hitting shield
            RaycastHit[] hitpoints;
            hitpoints = Physics.RaycastAll(cam.transform.TransformPoint(0, 0, CAM_DISTANCE_TO_MECH), (targetR.transform.root.position + new Vector3(0, 5, 0)) - cam.transform.TransformPoint(0, 0, CAM_DISTANCE_TO_MECH), MaxDistanceR, playerlayer).OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in hitpoints) {
                if (isTeamMode) {
                    PhotonView targetpv = hit.transform.root.GetComponent<PhotonView>();
                    if (targetpv.owner.GetTeam() != pv.owner.GetTeam()) {
                        return hit.collider.transform;
                    }
                } else {
                    PhotonView targetpv = hit.transform.root.GetComponent<PhotonView>();
                    if (targetpv.viewID != pv.viewID) //if not mine
                        return hit.collider.transform;
                }
            }
        }
        return targetR;
    }

    private void MarkTarget() {
        if (isRectifier_L) {
            if (targetL != null) {
                crosshairImage.EngTargetMark.transform.position = cam.WorldToScreenPoint(targetL.transform.position + new Vector3(0, 5, 0));
            }
        } else {
            if (targetL != null) {
                crosshairImage.targetMark.transform.position = cam.WorldToScreenPoint(targetL.transform.position + new Vector3(0, 5, 0));
            }
        }

        if (isRectifier_R) {
            if (targetR != null) {
                crosshairImage.EngTargetMark.transform.position = cam.WorldToScreenPoint(targetR.transform.position + new Vector3(0, 5, 0));
            }
        } else {
            if (targetR != null) {
                crosshairImage.targetMark.transform.position = cam.WorldToScreenPoint(targetR.transform.position + new Vector3(0, 5, 0));
            }
        }
        if ((!isRectifier_L && targetL != null) || (!isRectifier_R && targetR != null)) {
            crosshairImage.middlecross.enabled = false;
        } else {
            crosshairImage.middlecross.enabled = true;
        }

        crosshairImage.EngTargetMark.enabled = ((isRectifier_L && targetL != null) || (isRectifier_R && targetR != null));
        crosshairImage.targetMark.gameObject.SetActive((!isRectifier_L && targetL != null) || (!isRectifier_R && targetR != null));
    }

    private void SendLockedMessage(int id, string Name) {
        PhotonView target_pv = PhotonView.Find(id);
        if (target_pv == null || target_pv.tag == "Drone") return;

        if (id == LastLockTargetID) {
            if (Time.time - TimeOfLastSend >= SendMsgDeltaTime) {
                target_pv.RPC("OnLocked", PhotonTargets.All);
                TimeOfLastSend = Time.time;
            }
        } else {
            target_pv.RPC("OnLocked", PhotonTargets.All);
            TimeOfLastSend = Time.time;
            LastLockTargetID = id;
        }
    }

    public void ShowLocked() {
        if (isOnLocked) {
            StopCoroutine(coroutine);
            coroutine = StartCoroutine("HideLockedAfterTime", LockedMsgDuration);
        } else {
            isOnLocked = true;
            coroutine = StartCoroutine("HideLockedAfterTime", LockedMsgDuration);
            Sounds.PlayOnLocked();
        }
    }

    private void OnSkill(bool b) {
        onSkill = b;

        //turn crosshair to green
        if (b) {
            if (CrosshairRadiusL > 0) {
                crosshairImage.OnTargetL(false);
                targetL = null;
                LockL = false;
            }
            if (CrosshairRadiusR > 0) {
                crosshairImage.OnTargetR(false);
                targetR = null;
                LockR = false;
            }

            //remove the target mark
            MarkTarget();
        }
    }

    private IEnumerator HideLockedAfterTime(float time) {
        LockedImg.SetActive(true);
        yield return new WaitForSeconds(time);
        LockedImg.SetActive(false);
        isOnLocked = false;
    }

    public void CallShakingEffect(int hand) {
        crosshairImage.ShakingEffect(hand);
    }

    public void EnableCrosshair(bool b) {
        if(!pv.isMine)return;

        enabled = b;
        crosshairImage.gameObject.SetActive(b);

        if (!b) {
            ShutDownAllCrosshairs();
        }
    }

    private void ShutDownAllCrosshairs() {
        if (crosshairImage != null) {
            crosshairImage.CloseAllCrosshairs_L();
            crosshairImage.CloseAllCrosshairs_R();

            targetL = null;
            targetR = null;

            crosshairImage.targetMark.gameObject.SetActive(false);
            crosshairImage.middlecross.enabled = false;
            crosshairImage.EngTargetMark.enabled = false;
        }
    }
}