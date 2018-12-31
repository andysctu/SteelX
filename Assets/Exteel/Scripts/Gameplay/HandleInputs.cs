using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class HandleInputs : MonoBehaviour
{
    private GameManager _gameManager;
    private MechController _mechController;
    private MechCombat _mechCombat;
    private MechCamera _mechCamera;

    private int _senderID; //The actorID of player sending inputs
    private PhotonPlayer _sender;

    private readonly usercmd[] _cmdsToSend = new usercmd[4]; //a series of commands (including history commands) every update
    private usercmd _curUserCmd = new usercmd(){buttons = new bool[4]};
    public float SendRate = 0.03f;
    public float AdjustPositionThreshold = 0.1f, AdjustSpeed = 8;
    public float LerpPosSpeed = 10;//lerp from transform position to "_curPosition"
    private float _preSendTime;

    private bool _init;

    private confirmData _confirmInfo;
    public float SendConfirmRate = 0.05f;
    private float _preConfirmTime;

    private readonly usercmd[] _historyUserCmds = new usercmd[1024];
    private readonly Vector3[] _historyPositions = new Vector3[1024];
    private int _tick;

    private Vector3 _curPosition;

    private void Awake(){
        RegisterOnMechBuilt();
        InitComponents();
    }

    private void RegisterOnMechBuilt() {
        if (GetComponent<BuildMech>() != null) {
            GetComponent<BuildMech>().OnMechBuilt += Init;
        }
    }

    private void Init() {
        Debug.Log("get init");
        PhotonPlayer owner = GetComponent<BuildMech>().GetOwner();
        _senderID = owner.ID;
        _sender = owner;
        _curPosition = transform.position;
        Debug.Log("pos : " + transform.position);

        if (PhotonNetwork.isMasterClient || owner.IsLocal) {
            RegisterInputEvent();
            UserCmd.RegisterType();
            ConfirmData.RegisterType();
        }

        enabled = owner.IsLocal;
        _init = true;
    }

    private void InitComponents(){
        _mechController = GetComponent<MechController>();
        _mechCombat = GetComponent<MechCombat>();
        _mechCamera = GetComponentInChildren<MechCamera>();
    }

    private void RegisterInputEvent(){
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
    }

    private void Start(){
        _gameManager = FindObjectOfType<GameManager>(); //TODO : make sure game manager is exist
    }

    protected void OnPhotonEvent(byte eventCode, object content, int senderId){
        switch (eventCode){
            case GameEventCode.INPUT:
                UpdateCurInputs((usercmd[]) content, senderId);
                break;
            case GameEventCode.POS_CONFIRM:
                ConfirmPosition((confirmData) content);
                break;
        }
    }

    private void UpdateCurInputs(usercmd[] usercmds, int senderID){
        //Received by master
        if (senderID != this._senderID) return; //this is not sender's mech

        int clientTick = usercmds[0].Tick, unProcessedPackageCount = 0;

        if (clientTick - _tick < 0){
            unProcessedPackageCount = 1024 - _tick + clientTick;
            if (unProcessedPackageCount >= 4){
                Debug.LogError("Package loss : " + (1024 - _tick + clientTick - 3));
                unProcessedPackageCount = 3;
            }
        } else if (clientTick - _tick < 4){
            unProcessedPackageCount = clientTick - _tick;
        } else if (clientTick - _tick >= 4){
            Debug.LogError("Package loss : " + (clientTick - _tick - 3));
            unProcessedPackageCount = 3;
        }

        _tick = (clientTick - unProcessedPackageCount) % 1024;

        for (int i = 0; i <= unProcessedPackageCount; i++){
            _curUserCmd = usercmds[unProcessedPackageCount - i];
            ProcessInputs(_curUserCmd);
            _mechCombat.ProcessInputs(_curUserCmd);
            _mechCamera.transform.rotation = Quaternion.Euler(_curUserCmd.rot);

            _tick = (_tick + 1) % 1024;
        }

        transform.position = _curPosition;

        if (Time.time - _preConfirmTime > SendConfirmRate){
            _preConfirmTime = Time.time;

            //TODO : improve this
            //Send new pos back to the client
            _confirmInfo.Tick = _tick;
            _confirmInfo.position = _curPosition;
            ConfirmData.TransformMechDataToStruct(_mechCombat, _mechController, ref _confirmInfo);

            RaiseEventOptions options = new RaiseEventOptions();
            options.TargetActors = new[]{this._senderID};
            PhotonNetwork.RaiseEvent(GameEventCode.POS_CONFIRM, _confirmInfo, false, options);
        }
    }

    private void Update(){
        if (!_init) return;

        if (!_gameManager.BlockInput && _gameManager.GameIsBegin)//TODO : check the game begin
            GetInputs();
        else //Override inputs if blocking
            _curUserCmd = new usercmd(){msec = Time.deltaTime, buttons = new bool[UserCmd.ButtonsLength]};

        _curUserCmd.Tick = _tick;

        _historyPositions[_tick] = _curPosition;
        _historyUserCmds[_tick].buttons = new bool[UserCmd.ButtonsLength];
        UserCmd.CloneUsercmd(_curUserCmd, ref _historyUserCmds[_tick]);

        //Client send inputs to master
        if (!PhotonNetwork.isMasterClient && Time.time - _preSendTime > SendRate){
            _preSendTime = Time.time;

            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;

            int index = 0;
            for (int i = 0; i < 4; i++){
                index = _tick - i < 0 ? 1024 - i + _tick : _tick - i;

                if (_historyUserCmds[index].buttons == null){
                    _historyUserCmds[index].buttons = new bool[UserCmd.ButtonsLength];
                    _historyUserCmds[index].Tick = index;
                }

                _cmdsToSend[i] = _historyUserCmds[index];
            }

            PhotonNetwork.RaiseEvent(GameEventCode.INPUT, _cmdsToSend, false, options);
        }

        if (_sender.IsLocal){
            ProcessInputs(_curUserCmd);

            _mechCombat.ProcessInputs(_curUserCmd);

            _tick = (_tick + 1) % 1024;

            transform.position = Vector3.Lerp(transform.position, _curPosition, Time.deltaTime * LerpPosSpeed);
        }
    }

    private void GetInputs(){
        _curUserCmd.horizontal = Input.GetAxisRaw("Horizontal");
        _curUserCmd.vertical = Input.GetAxisRaw("Vertical");
        _curUserCmd.rot = _mechCamera.transform.eulerAngles;
        _curUserCmd.viewAngle = transform.rotation.eulerAngles.y;
        _curUserCmd.msec = Time.deltaTime;

        _curUserCmd.buttons[(int) UserButton.Space] = Input.GetKey(KeyCode.Space);
        _curUserCmd.buttons[(int) UserButton.LeftShift] = Input.GetKey(KeyCode.LeftShift);
        _curUserCmd.buttons[(int) UserButton.LeftMouse] = Input.GetMouseButton(0);
        _curUserCmd.buttons[(int) UserButton.RightMouse] = Input.GetMouseButton(1);
    }

    private void ProcessInputs(usercmd userCmd){
        transform.Rotate(Vector3.up, userCmd.viewAngle - transform.rotation.eulerAngles.y);

        _curPosition = _mechController.UpdatePosition(_curPosition, userCmd);
    }

    private void ConfirmPosition(confirmData data){
        int tick = data.Tick;
        Vector3 position = data.position;

        if (Vector3.Distance(_historyPositions[tick], position) > AdjustPositionThreshold){
            //Rewind
            int tmpTick = tick;

            Vector3 prePos = _curPosition;

            Debug.Log("***Force pos : " + position + " on tick : " + tmpTick + " diff : " + Vector3.Distance(_historyPositions[tick], position));

            //Adjust
            _historyPositions[tmpTick] = position;
            _curPosition = _historyPositions[tmpTick];

            switch (data.state){
                case 0:
                    _mechController.SetMovementState(_mechController.GroundedState);
                    break;
                case 1:
                    _mechController.SetMovementState(_mechController.JumpState);
                    break;
            }

            ConfirmData.TransformStructToMechData(data, ref _mechController, ref _mechCombat);

            while (tmpTick != this._tick){
                ProcessInputs(_historyUserCmds[tmpTick]);

                tmpTick = (tmpTick + 1) % 1024;

                Debug.Log("=> Tick : " + tmpTick + " Pos : " + _curPosition);

                _historyPositions[tmpTick] = _curPosition;
            }

            //Vector3 afterPos = _curPosition;

            //_curPosition = Vector3.Lerp(prePos, afterPos, Time.deltaTime * AdjustSpeed);
        }
    }

    private void OnDestroy(){
        PhotonNetwork.OnEventCall -= OnPhotonEvent;
    }
}