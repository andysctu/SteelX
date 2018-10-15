using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Weapons.Crosshairs;

public class CrosshairController : MonoBehaviour {
    [SerializeField] private BuildMech bm;
    [SerializeField] private GameObject LockedImg;
    [SerializeField] private Sounds Sounds;
    [SerializeField] private SkillController SkillController;
    [SerializeField] private Transform crosshairParent;

    private MechCombat _mechCombat;
    private PhotonView _playerPv;

    public List<GameObject> Targets = new List<GameObject>();//Update by checkisrendered.cs
    private Transform _targetL, _targetR;

    private PhotonView _lastLockedTargetPv;
    private Coroutine _lockingTargetCoroutine = null;
    private float _timeOfLastSend;
    private bool _isOnLocked = false, _lockedL = false, _lockedR = false;
    private const float SendLockMsgDeltaTime = 0.3f, LockMsgDuration = 0.5f;

    //Game vars
    private float _screenCoeff;
    private int _playerLayerMask, _terrainLayerMask;
    private bool _isTeamMode;

    //Cam infos
    private Camera _cam;
    public const float CamDistanceToMech = 15f, DistanceCoeff = 0.008f;

    private float _crosshairRadiusL, _crosshairRadiusR;
    private float _maxDistanceL, _maxDistanceR, _minDistanceL, _minDistanceR;
    private int _weaponOffset = 0;//avoid sending lock message too often
    private bool _isTargetAllyL = false, _isTargetAllyR = false;

    //Player state
    private bool _onSkill = false;

    private Crosshair[] _crosshairs = new Crosshair[4];

    private void Awake() {
        InitComponents();
        if (_playerPv == null || !_playerPv.isMine) { enabled = false; return; }

        RegisterOnWeaponBuilt();
        RegisterOnWeaponSwitched();
        RegisterOnMechEnabled();
        RegisterOnSkill();
    }

    private void RegisterOnWeaponBuilt() {
        bm.OnMechBuilt += InitCrosshairs;
    }

    private void InitCrosshairs() {
        //Destroy previous crosshairs
        for (int i = 0; i < _crosshairs.Length; i++) {
            if (_crosshairs[i] != null) _crosshairs[i].Destroy();
        }

        //Init new crosshairs
        for (int i = 0; i < bm.WeaponDatas.Length; i++) {
            _crosshairs[i] = (bm.WeaponDatas[i] == null) ? null : bm.WeaponDatas[i].GetCrosshair();

            if (_crosshairs[i] != null) _crosshairs[i].Init(crosshairParent, _cam, i % 2);
        }
    }

    private void RegisterOnWeaponSwitched() {
        _mechCombat.OnWeaponSwitched += UpdateCrosshair;
    }

    private void RegisterOnMechEnabled() {
        _mechCombat.OnMechEnabled += EnableCrosshair;
    }

    private void RegisterOnSkill() {
        if (SkillController != null) SkillController.OnSkill += OnSkill;
    }

    private void Start() {
        if (_playerPv == null || !_playerPv.isMine) { return; }

        GetGameVars();
    }

    private void GetGameVars() {
        _screenCoeff = (float)Screen.height / Screen.width;
        _isTeamMode = GameManager.IsTeamMode;

        _terrainLayerMask = LayerMask.GetMask("Terrain");
        _playerLayerMask = LayerMask.GetMask("PlayerLayer");
    }

    private void InitComponents() {
        _playerPv = bm.GetComponent<PhotonView>();
        if (_playerPv == null || !_playerPv.isMine) return;

        _mechCombat = bm.GetComponent<MechCombat>();
        _cam = GetComponent<Camera>();
    }

    public void UpdateCrosshair() {
        int Marksmanship = bm.MechProperty.Marksmanship;

        //Update current weapon offset
        _weaponOffset = _mechCombat.GetCurrentWeaponOffset();

        //Disable/Enable crosshairs
        for (int i = 0; i < bm.WeaponDatas.Length; i++) {
            bool b = (i == _weaponOffset || i == _weaponOffset + 1);
            if (_crosshairs[i] != null) {
                _crosshairs[i].SetRadius(bm.WeaponDatas[i].Radius * (1 + Marksmanship / 100.0f));
                _crosshairs[i].EnableCrosshair(b);

                if (b) {
                    if (i % 2 == 0) {
                        _crosshairRadiusL = bm.WeaponDatas[i].Radius * (1 + Marksmanship / 100.0f);
                        _maxDistanceL = bm.WeaponDatas[i].Range;
                        _minDistanceL = bm.WeaponDatas[i].MinRange;
                    } else {
                        _crosshairRadiusR = bm.WeaponDatas[i].Radius * (1 + Marksmanship / 100.0f);
                        _maxDistanceR = bm.WeaponDatas[i].Range;
                        _minDistanceR = bm.WeaponDatas[i].MinRange;
                    }
                }
            } else {
                if (b) {
                    if (i % 2 == 0) {
                        _crosshairRadiusL = 0;
                        _maxDistanceL = 0;
                        _minDistanceL = 0;
                    } else {
                        _crosshairRadiusR = 0;
                        _maxDistanceR = 0;
                        _minDistanceR = 0;
                    }
                }
            }
        }

        _isTargetAllyL = (bm.WeaponDatas[_weaponOffset] != null && bm.WeaponDatas[_weaponOffset].IsTargetAlly);
        _isTargetAllyR = (bm.WeaponDatas[_weaponOffset + 1] != null && bm.WeaponDatas[_weaponOffset + 1].IsTargetAlly);

        _targetL = null;
        _targetR = null;
    }

    public Transform DectectTarget(float crosshairRadius, float range, float minimunRange, bool detectAlly) {
        if (crosshairRadius > 0) {
            for (int i = 0; i < Targets.Count; i++) {
                if (Targets[i] == null) {
                    Targets.RemoveAt(i);
                    continue;
                }

                if (Targets[i].layer == 0) continue;//the target is on skill or dead

                PhotonView targetPv = Targets[i].GetComponent<PhotonView>();
                if (targetPv.viewID == _playerPv.viewID) continue;

                if (_isTeamMode) {
                    if (!detectAlly) {
                        if (targetPv.owner.GetTeam() == _playerPv.owner.GetTeam()) continue;
                    } else {
                        if (targetPv.owner.GetTeam() != _playerPv.owner.GetTeam()) continue;
                    }
                } else {
                    if (detectAlly) continue;//Not team mode => no ally
                }

                //check distance
                if (Vector3.Distance(Targets[i].transform.position, transform.root.position) > range || Vector3.Distance(Targets[i].transform.position, transform.root.position) < minimunRange) continue;

                Vector3 targetLocInCam = _cam.WorldToViewportPoint(Targets[i].transform.position + new Vector3(0, 5, 0));
                Vector3 rayStartPoint = transform.root.position + new Vector3(0, 5, 0); //rayStartpoint should not inside terrain => not detect
                Vector2 targetLocOnScreen = new Vector2(targetLocInCam.x, (targetLocInCam.y - 0.5f) * _screenCoeff + 0.5f);

                if (Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * crosshairRadius && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * crosshairRadius) {
                    //check if Terrain block the way
                    RaycastHit hit;
                    if (Physics.Raycast(rayStartPoint, (Targets[i].transform.position + new Vector3(0, 5, 0) - rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, Targets[i].transform.position + new Vector3(0, 5, 0)), _terrainLayerMask)) {
                        if (hit.collider.gameObject.layer == 10) continue;
                    }

                    if (!detectAlly) SendLockedMessage(targetPv);

                    return Targets[i].transform;
                }
            }
        }
        return null;
    }

    public Transform[] DectectMultiTargets(float crosshairRadius, float range, bool detectAlly) {
        if (crosshairRadius > 0) {
            List<Transform> targets_in_range = new List<Transform>();

            for (int i = 0; i < Targets.Count; i++) {
                if (Targets[i] == null) {
                    Targets.RemoveAt(i);
                    continue;
                }

                if (Targets[i].layer == 0) continue;//On skill or dead

                PhotonView targetpv = Targets[i].GetComponent<PhotonView>();
                if (targetpv.viewID == _playerPv.viewID) continue;

                if (_isTeamMode) {
                    if (!detectAlly) {
                        if (targetpv.owner.GetTeam() == _playerPv.owner.GetTeam()) continue;
                    } else {
                        if (targetpv.owner.GetTeam() != _playerPv.owner.GetTeam()) continue;
                    }
                } else {
                    if (detectAlly) continue;//if not team mode , ignore eng
                }

                //check distance
                if (Vector3.Distance(Targets[i].transform.position, transform.root.position) > range) continue;

                Vector3 targetLocInCam = _cam.WorldToViewportPoint(Targets[i].transform.position + new Vector3(0, 5, 0));
                Vector3 rayStartPoint = transform.root.position + new Vector3(0, 10, 0); //rayStartpoint should not inside terrain => not detect
                Vector2 targetLocOnScreen = new Vector2(targetLocInCam.x, (targetLocInCam.y - 0.5f) * _screenCoeff + 0.5f);
                if (Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * crosshairRadius && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * crosshairRadius) {
                    //check if Terrain block the way
                    RaycastHit hit;
                    if (Physics.Raycast(rayStartPoint, (Targets[i].transform.position + new Vector3(0, 5, 0) - rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, Targets[i].transform.position + new Vector3(0, 5, 0)), _terrainLayerMask)) {
                        if (hit.collider.gameObject.layer == 10) {
                            continue;
                        }
                    }
                    targets_in_range.Add(Targets[i].transform);
                }
            }
            return targets_in_range.ToArray();
        }
        return null;
    }

    private void Update() {
        if (_onSkill) return;

        if (_crosshairs[_weaponOffset] != null) {
            _crosshairs[_weaponOffset].Update();

            if ((_targetL = DectectTarget(_crosshairRadiusL, _maxDistanceL, _minDistanceL, _isTargetAllyL)) != null) {
                _crosshairs[_weaponOffset].OnTarget(true);
                MarkTarget(_weaponOffset, _targetL);

                if (!_lockedL) { Sounds.PlayLock(); _lockedL = true; }
            } else {
                _crosshairs[_weaponOffset].OnTarget(false);
                _lockedL = false;
            }
        }

        if (_crosshairs[_weaponOffset + 1] != null) {
            _crosshairs[_weaponOffset + 1].Update();

            if ((_targetR = DectectTarget(_crosshairRadiusR, _maxDistanceR, _minDistanceR, _isTargetAllyR)) != null) {
                _crosshairs[_weaponOffset + 1].OnTarget(true);
                MarkTarget(_weaponOffset + 1, _targetR);

                if (!_lockedR) { Sounds.PlayLock(); _lockedR = true; }
            } else {
                _crosshairs[_weaponOffset + 1].OnTarget(false);
                _lockedR = false;
            }
        }
    }

    //TODO : improve detection & generalize the CAM_DISTANCE_TO_MECH
    public Transform GetCurrentTargetL() {
        if (_targetL != null && !_isTargetAllyL) {
            //cast a ray to check if hitting shield
            //Debug.DrawRay (cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH), ((targetL.transform.root.position + new Vector3 (0, 5, 0)) - cam.transform.TransformPoint (0, 0, CAM_DISTANCE_TO_MECH))*100f, Color.red, 3f);
            RaycastHit[] hitpoints;
            hitpoints = Physics.RaycastAll(_cam.transform.TransformPoint(0, 0, CamDistanceToMech), (_targetL.transform.root.position + new Vector3(0, 5, 0)) - _cam.transform.TransformPoint(0, 0, CamDistanceToMech), _maxDistanceL, _playerLayerMask).OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in hitpoints) {
                PhotonView targetPv = hit.transform.root.GetComponent<PhotonView>();
                if (_isTeamMode) {
                    if (targetPv.owner.GetTeam() != _playerPv.owner.GetTeam())
                        return hit.collider.transform;
                } else {
                    if (targetPv.viewID != _playerPv.viewID)
                        return hit.collider.transform;
                }
            }
        }
        return _targetL;
    }

    public Transform GetCurrentTargetR() {
        if (_targetR != null && !_isTargetAllyR) {
            RaycastHit[] hitpoints;
            hitpoints = Physics.RaycastAll(_cam.transform.TransformPoint(0, 0, CamDistanceToMech), (_targetR.transform.root.position + new Vector3(0, 5, 0)) - _cam.transform.TransformPoint(0, 0, CamDistanceToMech), _maxDistanceR, _playerLayerMask).OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in hitpoints) {
                if (_isTeamMode) {
                    PhotonView targetpv = hit.transform.root.GetComponent<PhotonView>();
                    if (targetpv.owner.GetTeam() != _playerPv.owner.GetTeam()) {
                        return hit.collider.transform;
                    }
                } else {
                    PhotonView targetpv = hit.transform.root.GetComponent<PhotonView>();
                    if (targetpv.viewID != _playerPv.viewID) //if not mine
                        return hit.collider.transform;
                }
            }
        }
        return _targetR;
    }

    private void MarkTarget(int weapPos, Transform target) {
        _crosshairs[weapPos].MarkTarget(target);
    }

    private void SendLockedMessage(PhotonView targetPv) {
        if (targetPv == null || targetPv.tag == "Drone") return;

        if (targetPv == _lastLockedTargetPv) {
            if (Time.time - _timeOfLastSend >= SendLockMsgDeltaTime) {
                targetPv.RPC("OnLocked", PhotonTargets.All);
                _timeOfLastSend = Time.time;
            }
        } else {
            targetPv.RPC("OnLocked", PhotonTargets.All);
            _timeOfLastSend = Time.time;
            _lastLockedTargetPv = targetPv;
        }
    }

    public void OnShootAction(int WeapPos) {
        if (_crosshairs[WeapPos] != null) {
            _crosshairs[WeapPos].OnShootAction();
        }
    }

    public void ShowLocked() {//TODO : move this part to HUD
        if (_isOnLocked) {
            StopCoroutine(_lockingTargetCoroutine);
            _lockingTargetCoroutine = StartCoroutine("HideLockedAfterTime", LockMsgDuration);
        } else {
            _isOnLocked = true;
            _lockingTargetCoroutine = StartCoroutine("HideLockedAfterTime", LockMsgDuration);
            Sounds.PlayOnLocked();
        }
    }

    private IEnumerator HideLockedAfterTime(float time) {
        LockedImg.SetActive(true);
        yield return new WaitForSeconds(time);
        LockedImg.SetActive(false);
        _isOnLocked = false;
    }

    private void OnSkill(bool b) {
        _onSkill = b;

        //turn crosshair to green
        if (b) {
            if (_crosshairs[_weaponOffset] != null) {
                _crosshairs[_weaponOffset].OnTarget(false);
                _targetL = null;
                _lockedL = false;
            }
            if (_crosshairs[_weaponOffset + 1] != null) {
                _crosshairs[_weaponOffset + 1].OnTarget(false);
                _targetR = null;
                _lockedR = false;
            }
        }
    }

    public void EnableCrosshair(bool b) {
        if (!_playerPv.isMine) return;

        enabled = b;

        if (!b) DisableAllCrosshairs();
    }

    private void DisableAllCrosshairs() {
        for (int i = 0; i < _crosshairs.Length; i++) {
            if (_crosshairs[i] != null)
                _crosshairs[i].EnableCrosshair(false);
        }
    }
}