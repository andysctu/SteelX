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
    [SerializeField] private Transform crosshairParent;//to be attached on panelCanvas

    private Transform _panelCanvas;
    private MechCombat _mechCombat;

    private IDamageable _targetL, _targetR;
    private readonly List<IDamageable> _targets = new List<IDamageable>();
    //send by client todo : check this
    //private PhotonView _lastLockedTargetPv;
    private Coroutine _lockingTargetCoroutine;
    private float _timeOfLastSend;
    private bool _isOnLocked, _lockedL, _lockedR;
    private const float SendLockMsgDeltaTime = 0.3f, LockMsgDuration = 0.5f;

    //Game vars
    private float _screenCoeff;
    private int _playerLayerMask, _terrainLayerMask, _shieldLayerMask;

    //Cam infos
    private Camera _cam;
    public const float CamDistanceToMech = 15f, DistanceCoeff = 0.008f;

    private float _crosshairRadiusL, _crosshairRadiusR;
    private float _maxDistanceL, _maxDistanceR, _minDistanceL, _minDistanceR;
    private int _weaponOffset;
    private bool _isTargetAllyL, _isTargetAllyR;

    //Player state
    private bool _onSkill;

    private readonly Crosshair[] _crosshairs = new Crosshair[4];

    private void Awake() {
        InitComponents();

        RegisterOnWeaponBuilt();
        RegisterOnMechBuilt();
    }

    private void RegisterOnMechBuilt(){
        bm.OnMechBuilt += Init;
    }

    private void Init(){
        //enabled = bm.GetOwner().IsLocal;
        //if (!bm.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) {return; }
		//
        //RegisterOnWeaponSwitched();
        //RegisterOnMechEnabled();
        //RegisterOnSkill();
		//
        //if(bm.GetOwner().IsLocal){
        //    FindPanelCanvas();
        //    crosshairParent.SetParent(_panelCanvas);
        //}
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
        GetGameVars();
    }

    private void GetGameVars() {
        _screenCoeff = (float)Screen.height / Screen.width;//todo : check this
    }

    private void InitComponents() {
        _mechCombat = bm.GetComponent<MechCombat>();
        _cam = GetComponent<Camera>();
        _terrainLayerMask = LayerMask.GetMask("Terrain");
        _playerLayerMask = LayerMask.GetMask("PlayerLayer");
        _shieldLayerMask = LayerMask.GetMask("Shield");
    }

    public void UpdateCrosshair() {
        int marksmanship = bm.MechProperty.Marksmanship;

        //Update current weapon offset
        _weaponOffset = _mechCombat.GetCurrentWeaponOffset();

        //Disable/Enable crosshairs
        for (int i = 0; i < bm.WeaponDatas.Length; i++) {
            bool b = (i == _weaponOffset || i == _weaponOffset + 1);
            if (_crosshairs[i] != null) {
                _crosshairs[i].SetRadius(bm.WeaponDatas[i].Radius * (1 + marksmanship / 100.0f));
                //if(bm.GetOwner().IsLocal)_crosshairs[i].EnableCrosshair(b);

                if (b) {
                    if (i % 2 == 0) {
                        _crosshairRadiusL = bm.WeaponDatas[i].Radius * (1 + marksmanship / 100.0f);
                        _maxDistanceL = bm.WeaponDatas[i].Range;
                        _minDistanceL = bm.WeaponDatas[i].MinRange;
                    } else {
                        _crosshairRadiusR = bm.WeaponDatas[i].Radius * (1 + marksmanship / 100.0f);
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

    public IDamageable DetectTarget(float crosshairRadius, float minRange, float maxRange, bool isTargetAlly) {
        if (crosshairRadius > 0) {
            RaycastHit[] hits = Physics.SphereCastAll(_cam.transform.position, crosshairRadius * 3, _cam.transform.forward, maxRange, _playerLayerMask).OrderBy(h => h.distance).ToArray();//todo : improve this

            for (int i = 0; i < hits.Length; i++){
                IDamageable target;
                if ((target = hits[i].collider.GetComponent(typeof(IDamageable)) as IDamageable) == null){
                    Debug.Log(hits[i].collider.transform.parent.gameObject.name + "does not have IDamageable component and is on player layer");
                    continue;
                }

                //if (target.GetOwner() == null){
                //    Debug.Log("owner is null");//probably game not init
                //    continue;
                //}

                //it's me
                //if(target.GetOwner().IsLocal && target.GetPhotonView().tag != "Drone")continue;

                //if((isTargetAlly && target.IsEnemy(bm.GetOwner())) || (!isTargetAlly && !target.IsEnemy(bm.GetOwner())))continue;

                //check distance
                if (Vector3.Distance(hits[i].collider.transform.position, transform.root.position) > maxRange || Vector3.Distance(hits[i].collider.transform.position, transform.root.position) < minRange) continue;

                Vector3 targetLocInCam = _cam.WorldToViewportPoint(hits[i].collider.transform.position + new Vector3(0, 5, 0));
                Vector3 rayStartPoint = transform.root.position + new Vector3(0, 5, 0); //rayStartpoint should not inside terrain => not detect
                Vector2 targetLocOnScreen = new Vector2(targetLocInCam.x, (targetLocInCam.y - 0.5f) * _screenCoeff + 0.5f);

                if (Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * crosshairRadius && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * crosshairRadius) {
                    //check if Terrain block the way
                    RaycastHit hit;
                    if (Physics.Raycast(rayStartPoint, (hits[i].collider.transform.position + new Vector3(0, 5, 0) - rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, hits[i].collider.transform.position + new Vector3(0, 5, 0)), _terrainLayerMask)) {
                        if (hit.collider.gameObject.layer == _terrainLayerMask) continue;
                    }

                    //if (!isTargetAlly) SendLockedMessage(target.GetPhotonView());

                    return target;
                }
            }
        }
        return null;
    }

    public IDamageable[] DetectMultiTargets(float crosshairRadius, float minRange, float maxRange, bool isTargetAlly) {
        if (crosshairRadius > 0) {
            RaycastHit[] hits = Physics.SphereCastAll(_cam.transform.position, crosshairRadius * 3, _cam.transform.forward, maxRange, _playerLayerMask).OrderBy(h => h.distance).ToArray();
            _targets.Clear();

            for (int i = 0; i < hits.Length; i++) {
                IDamageable target;
                if ((target = hits[i].collider.GetComponent(typeof(IDamageable)) as IDamageable) == null) {
                    Debug.Log(hits[i].collider + "does not have IDamageable component but is on player layer");
                    continue;
                }

                //it's me
                //if (target.GetOwner().IsLocal) continue;

                //if ((isTargetAlly && target.IsEnemy(bm.GetOwner())) || (!isTargetAlly && !target.IsEnemy(bm.GetOwner()))) continue;

                //check distance
                if (Vector3.Distance(hits[i].collider.transform.position, transform.root.position) > maxRange || Vector3.Distance(hits[i].collider.transform.position, transform.root.position) < minRange) continue;

                Vector3 targetLocInCam = _cam.WorldToViewportPoint(hits[i].collider.transform.position + new Vector3(0, 5, 0));
                Vector3 rayStartPoint = transform.root.position + new Vector3(0, 5, 0); //rayStartpoint should not inside terrain => not detect
                Vector2 targetLocOnScreen = new Vector2(targetLocInCam.x, (targetLocInCam.y - 0.5f) * _screenCoeff + 0.5f);

                if (Mathf.Abs(targetLocOnScreen.x - 0.5f) < DistanceCoeff * crosshairRadius && Mathf.Abs(targetLocOnScreen.y - 0.5f) < DistanceCoeff * crosshairRadius) {
                    //check if Terrain block the way
                    RaycastHit hit;
                    if (Physics.Raycast(rayStartPoint, (hits[i].collider.transform.position + new Vector3(0, 5, 0) - rayStartPoint).normalized, out hit, Vector3.Distance(rayStartPoint, hits[i].collider.transform.position + new Vector3(0, 5, 0)), _terrainLayerMask)) {
                        if (hit.collider.gameObject.layer == _terrainLayerMask) continue;
                    }

                    //if (!isTargetAlly) SendLockedMessage(target.GetPhotonView());

                    _targets.Add(target);
                }
            }

            return _targets.ToArray();
        }
        return null;
    }

    private void Update() {
        if (_onSkill) return;

        if (_crosshairs[_weaponOffset] != null) {
            _crosshairs[_weaponOffset].Update();

            if ((_targetL = DetectTarget(_crosshairRadiusL, _minDistanceL, _maxDistanceL, _isTargetAllyL)) != null) {
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

            if ((_targetR = DetectTarget(_crosshairRadiusR,_minDistanceR, _maxDistanceR, _isTargetAllyR)) != null) {
                _crosshairs[_weaponOffset + 1].OnTarget(true);
                MarkTarget(_weaponOffset + 1, _targetR);

                if (!_lockedR) { Sounds.PlayLock(); _lockedR = true; }
            } else {
                _crosshairs[_weaponOffset + 1].OnTarget(false);
                _lockedR = false;
            }
        }
    }

    public IDamageable GetCurrentTarget(int hand){
        return hand == 0 ? GetCurrentTargetL() : GetCurrentTargetR();
    }

    private IDamageable GetCurrentTargetL() {
        if (_targetL != null && !_isTargetAllyL) {
            //cast a ray to check if hitting shield
            //Debug.DrawRay (cam.transform.position, _targetL.GetPosition() - cam.transform.position)*100f, Color.red, 3f);
            RaycastHit[] hitPoints = Physics.RaycastAll(_cam.transform.position, _targetL.GetPosition() - _cam.transform.position, _maxDistanceL, _shieldLayerMask).OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in hitPoints) {
                IDamageable t = hit.collider.GetComponent(typeof(IDamageable)) as IDamageable;
                if (t == null){
                    Debug.LogError("this object does not have IDamageable but is on shield layer");
                    continue;
                }

                //if (t.IsEnemy(bm.GetOwner())){
                //    return t;
                //}
            }
        }
        return _targetL;
    }

    private IDamageable GetCurrentTargetR() {
        if (_targetR != null && !_isTargetAllyR) {
            RaycastHit[] hitPoints = Physics.RaycastAll(_cam.transform.position, _targetR.GetPosition() - _cam.transform.position, _maxDistanceR, _shieldLayerMask).OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in hitPoints) {
                IDamageable t = hit.collider.GetComponent(typeof(IDamageable)) as IDamageable;
                if (t == null) {
                    Debug.LogError("this object does not have IDamageable but is on shield layer");
                    continue;
                }

                //if (t.IsEnemy(bm.GetOwner())) {
                //    return t;
                //}
            }
        }
        return _targetR;
    }

    private void MarkTarget(int weapPos, IDamageable target) {
        _crosshairs[weapPos].MarkTarget(target);
    }

    //private void SendLockedMessage(PhotonView targetPv) {
    //    if (targetPv == null || targetPv.tag == "Drone") return;
	//
    //    if (targetPv == _lastLockedTargetPv) {
    //        if (Time.time - _timeOfLastSend >= SendLockMsgDeltaTime) {
    //            targetPv.RPC("OnLocked", PhotonTargets.All);
    //            _timeOfLastSend = Time.time;
    //        }
    //    } else {
    //        targetPv.RPC("OnLocked", PhotonTargets.All);
    //        _timeOfLastSend = Time.time;
    //        _lastLockedTargetPv = targetPv;
    //    }
    //}

    public void OnShootAction(int WeapPos) {//crosshair effect
        if (_crosshairs[WeapPos] != null) {
            _crosshairs[WeapPos].OnShootAction();
        }
    }

    public void ShowLocked() {//TODO : move this part to HUD
        if (_isOnLocked) {
            StopCoroutine(_lockingTargetCoroutine);
            _lockingTargetCoroutine = StartCoroutine(HideLockedAfterTime(LockMsgDuration));
        } else {
            _isOnLocked = true;
            _lockingTargetCoroutine = StartCoroutine(HideLockedAfterTime(LockMsgDuration));
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
        //if (!bm.GetOwner().IsLocal) return;

        enabled = b;

        if (!b) DisableAllCrosshairs();
    }

    private void DisableAllCrosshairs() {
        for (int i = 0; i < _crosshairs.Length; i++) {
            if (_crosshairs[i] != null)
                _crosshairs[i].EnableCrosshair(false);
        }
    }

    private void FindPanelCanvas() {
        foreach (var canvas in FindObjectsOfType<Canvas>()) {
            if (canvas.name == "PanelCanvas") {
                _panelCanvas = canvas.transform;
                break;
            }
        }
    }
}