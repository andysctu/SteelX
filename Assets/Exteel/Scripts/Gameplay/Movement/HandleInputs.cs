using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class HandleInputs : MonoBehaviour
{
    private GameManager _gameManager;
    private MechController _mechController;
    private MechCombat _mechCombat;

    private int _senderID; //The actorID of player sending inputs
    private PhotonPlayer _sender;

    private readonly usercmd[] _cmdsToSend = new usercmd[4]; //a series of commands (including history commands) every update
    private usercmd _curUserCmd = new usercmd(){buttons = new bool[4]};
    public float SendRate = 0.03f;
    public float AdjustPositionThreshold = 0.1f, AdjustSpeed = 8;
    private float _preSendTime;

    private bool _init;

    //Master
    private enum ConfirmData
    {
        Position,
        Speed,
        State,
        EN,
        Tick,

        IsVerBoostAvailable,
        VerBoostStartYPos,
        JumpReleased,

        CurBoostingSpeed,
        IsBoosting,

        InstantMoveRemainingDistance,
        InstantMoveRemainingTime,
        InstantMoveDir
    };

    private readonly Hashtable _confirmInfo = new Hashtable();
    public float SendConfirmRate = 0.05f;
    private float _preConfirmTime;

    private readonly usercmd[] _historyUserCmds = new usercmd[1024];
    private readonly Vector3[] _historyPositions = new Vector3[1024];
    private int _tick;

    private Vector3 _curPosition;

    private void Awake(){
        InitComponents();
    }

    public void Init(PhotonPlayer sender){
        _senderID = sender.ID;
        _sender = sender;

        if (PhotonNetwork.isMasterClient || sender.IsLocal){
            RegisterInputEvent();
            UserCmd.RegisterType();
        }

        enabled = sender.IsLocal;
        _init = true;
    }

    private void InitComponents(){
        _mechController = GetComponent<MechController>();
        _mechCombat = GetComponent<MechCombat>();
    }

    private void RegisterInputEvent(){
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
    }

    private void Start(){
        _gameManager = FindObjectOfType<GameManager>(); //TODO : make sure game manager is exist

        _curPosition = transform.position;
    }

    protected void OnPhotonEvent(byte eventCode, object content, int senderId){
        switch (eventCode){
            case GameEventCode.INPUT:
                UpdateCurInputs((usercmd[]) content, senderId);
                break;
            case GameEventCode.POS_CONFIRM:
                ConfirmPosition((Hashtable) content);
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

            _tick = (_tick + 1) % 1024;
        }

        transform.position = _curPosition;

        if (Time.time - _preConfirmTime > SendConfirmRate){
            _preConfirmTime = Time.time;

            //TODO : improve this
            //Send new pos back to the client
            _confirmInfo[(int) ConfirmData.Tick] = _tick;
            _confirmInfo[(int) ConfirmData.Position] = _curPosition;
            _confirmInfo[(int) ConfirmData.Speed] = new Vector3(_mechController.XSpeed, _mechController.YSpeed, _mechController.ZSpeed);
            _confirmInfo[(int) ConfirmData.EN] = _mechCombat.CurrentEN;
            _confirmInfo[(int) ConfirmData.IsVerBoostAvailable] = _mechController.IsAvailableVerBoost;
            _confirmInfo[(int) ConfirmData.State] = _mechController.CurMovementState != null && _mechController.CurMovementState == _mechController.JumpState ? 1 : 0;
            _confirmInfo[(int) ConfirmData.VerBoostStartYPos] = _mechController.VerticalBoostStartYPos;
            _confirmInfo[(int) ConfirmData.JumpReleased] = _mechController.JumpReleased;
            _confirmInfo[(int) ConfirmData.CurBoostingSpeed] = _mechController.CurBoostingSpeed;
            _confirmInfo[(int) ConfirmData.IsBoosting] = _mechController.IsBoosting;

            _confirmInfo[(int) ConfirmData.InstantMoveRemainingDistance] = _mechController.InstantMoveRemainingDistance;
            _confirmInfo[(int)ConfirmData.InstantMoveDir] = _mechController.InstantMoveDir;
            _confirmInfo[(int)ConfirmData.InstantMoveRemainingTime] = _mechController.InstantMoveRemainingTime;

            RaiseEventOptions options = new RaiseEventOptions();
            options.TargetActors = new[]{this._senderID};
            PhotonNetwork.RaiseEvent(GameEventCode.POS_CONFIRM, _confirmInfo, false, options);
        }
    }

    public float LerpPosSpeed = 10;

    private void Update(){
        if (!_init) return;

        if (!_gameManager.BlockInput)
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

            _tick = (_tick + 1) % 1024;

            transform.position = Vector3.Lerp(transform.position, _curPosition, Time.deltaTime * LerpPosSpeed);
        }
    }

    private void GetInputs(){
        _curUserCmd.horizontal = Input.GetAxis("Horizontal");
        _curUserCmd.vertical = Input.GetAxis("Vertical");
        _curUserCmd.viewAngle = transform.rotation.eulerAngles.y; //TODO : improve this
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

    private void ConfirmPosition(Hashtable hashtable){
        int tick = (int) hashtable[(int) ConfirmData.Tick];
        Vector3 position = (Vector3) hashtable[(int) ConfirmData.Position];

        //TODO : EN

        if (Vector3.Distance(_historyPositions[tick], position) > AdjustPositionThreshold){
            //Rewind
            int tmpTick = tick;

            Vector3 prePos = _curPosition;

            Debug.Log("***Force pos : " + position + " on tick : " + tmpTick + " diff : " + Vector3.Distance(_historyPositions[tick], position));

            //Adjust
            _historyPositions[tmpTick] = position;
            _curPosition = _historyPositions[tmpTick];

            switch ((int) hashtable[(int) ConfirmData.State]){
                case 0:
                    _mechController.SetMovementState(_mechController.GroundedState);
                    break;
                case 1:
                    _mechController.SetMovementState(_mechController.JumpState);
                    break;
            }

            _mechController.SetVerBoostStartPos((float) hashtable[(int) ConfirmData.VerBoostStartYPos]);
            _mechController.SetAvailableToBoost((bool) hashtable[(int) ConfirmData.IsVerBoostAvailable]);
            _mechController.SetSpeed((Vector3) hashtable[(int) ConfirmData.Speed]);
            _mechController.JumpReleased = (bool) hashtable[(int) ConfirmData.JumpReleased];
            _mechController.InstantMoveRemainingDistance = (float) hashtable[(int) ConfirmData.InstantMoveRemainingDistance];
            _mechController.InstantMoveRemainingTime = (float)hashtable[(int)ConfirmData.InstantMoveRemainingTime];
            _mechController.InstantMoveDir = (Vector3)hashtable[(int)ConfirmData.InstantMoveDir];

            _mechController.CurBoostingSpeed = (float) hashtable[(int) ConfirmData.CurBoostingSpeed];
            _mechController.IsBoosting = (bool) hashtable[(int) ConfirmData.IsBoosting];

            while (tmpTick != this._tick){
                ProcessInputs(_historyUserCmds[tmpTick]);

                tmpTick = (tmpTick + 1) % 1024;

                Debug.Log("=> Tick : " + tmpTick + " Pos : " + transform.position);

                _historyPositions[tmpTick] = _curPosition;
            }

            //Vector3 afterPos = _curPosition;

            //_curPosition = Vector3.Lerp(prePos, afterPos, Time.deltaTime * AdjustSpeed);
        }
    }
}