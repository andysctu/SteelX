using UnityEngine;

public class InputManager : MonoBehaviour {
    private GameManager _gameManager;
    private BuildMech _buildMech;
    private MechController _mechController;
    private MechCombat _mechCombat;
    private MechCamera _mechCamera;

    private int _senderID; //The actorID of player sending inputs
    private PhotonPlayer _sender;

    private const int MaxClientSendCmdSize = 15;
    private usercmd[] _cmdsToSend;//a series of commands (including history commands) every update
    private usercmd _curUserCmd;
    public float ClientSendInputsInterval = 0.04f;
    private float _preClientSendTime;

    public float CorrectPositionThreshold = 0.1f, CorrectingSpeed = 8;
    public float LerpPosSpeed = 10;//lerp from transform position to "_curPosition"

    public float ConfirmInterval = 0.05f;
    private float _preConfirmTime;
    private confirmData _confirmData;
    private readonly usercmd[] _historyUserCmds = new usercmd[1024];
    private readonly Vector3[] _serverHistoryPositions = new Vector3[1024];
    private readonly Vector3[] _clientHistoryPositions = new Vector3[1024];
    private int _tick;

    private bool _init;
    public Vector3 CurPosition { get;private set;}

    private void Awake() {
        _curUserCmd = new usercmd() { buttons = new bool[UserCmd.ButtonsLength] };
        _cmdsToSend = new usercmd[MaxClientSendCmdSize];
        InitComponents();
        RegisterOnMechBuilt();
    }

    private void RegisterOnMechBuilt() {
        if ((_buildMech = GetComponent<BuildMech>()) != null) {
            _buildMech.OnMechBuilt += Init;
        }
    }

    private void Init() {
        PhotonPlayer owner = _buildMech.GetOwner();
        _senderID = owner.ID;
        _sender = owner;
        CurPosition = transform.position;

        if (PhotonNetwork.isMasterClient || owner.IsLocal) {
            RegisterInputEvent();
            UserCmd.RegisterType();
            ConfirmData.RegisterType();
        }

        enabled = owner.IsLocal;
        _init = true;

        _gameManager = FindObjectOfType<GameManager>(); //TODO : make sure game manager exist
        _gameManager.OnWorldUpdate += OnWorldUpdate;
    }

    private void InitComponents() {
        _mechController = GetComponent<MechController>();
        _mechCombat = GetComponent<MechCombat>();
        _mechCamera = GetComponentInChildren<MechCamera>();
    }

    private void RegisterInputEvent() {
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
    }

    protected void OnPhotonEvent(byte eventCode, object content, int senderId) {
        switch (eventCode) {
            case GameEventCode.Input:
            MasterReceiveInputs((usercmd[])content, senderId);
            break;
            case GameEventCode.PosConfirm:
            ConfirmPosition((confirmData)content);
            break;
        }
    }

    private void OnWorldUpdate() {
        if (_sender == null) return;

        //take a snapshot of current state and send
        _serverHistoryPositions[_gameManager.GetServerTick()] = CurPosition;
        //TODO : broadcast position

        if (PhotonNetwork.isMasterClient && !_sender.IsLocal) {

            
        }
    }

    private void MasterReceiveInputs(usercmd[] usercmds, int senderID) {
        //Received by master
        if (senderID != this._senderID || usercmds == null || usercmds.Length == 0) return; //this is not sender's mech

        //check how many inputs not known by master
        int tickDiff = usercmds[0].Tick - _tick >= 0 ? usercmds[0].Tick - _tick : 1024 - (_tick - usercmds[0].Tick);
        tickDiff = Mathf.Clamp(tickDiff, 0, MaxClientSendCmdSize - 1);
        _tick = (usercmds[0].Tick - tickDiff) % 1024;

        //Process inputs
        for (int i = 0; i <= tickDiff; i++) {
            ProcessInputs(usercmds[tickDiff - i]);
            _mechCamera.transform.rotation = Quaternion.Euler(_curUserCmd.rot);
            _mechCombat.ProcessInputs(_curUserCmd);

            _tick = (_tick + 1) % 1024;
        }

        if (Time.time - _preConfirmTime > ConfirmInterval){
            _preConfirmTime = Time.time;

            _confirmData.ClientTick = _tick;
            _confirmData.position = CurPosition;
            ConfirmData.TransformMechDataToStruct(_mechCombat, _mechController, ref _confirmData);
            PhotonNetwork.RaiseEvent(GameEventCode.PosConfirm, _confirmData, false, new RaiseEventOptions { TargetActors = new[] { _senderID } });
        }

        transform.position = CurPosition;//for display todo : move to mech controller
    }

    private void Update() {
        if (!_init) return;

        if (!_gameManager.BlockInput && _gameManager.GameIsBegin) {
            GetInputs();
        } else {//Override inputs
            _curUserCmd = new usercmd() { msec = Time.deltaTime, buttons = new bool[UserCmd.ButtonsLength], rot = _mechCamera.transform.eulerAngles, viewAngle = transform.rotation.eulerAngles.y };
        }

        _curUserCmd.Tick = _tick;
        _curUserCmd.ServerTick = _gameManager.GetServerTick();

        _historyUserCmds[_tick].buttons = new bool[UserCmd.ButtonsLength];
        UserCmd.CloneUsercmd(_curUserCmd, ref _historyUserCmds[_tick]);

        //Client send inputs to master
        if (!PhotonNetwork.isMasterClient && Time.time - _preClientSendTime > ClientSendInputsInterval) {
            _preClientSendTime = Time.time;

            for (int i = 0; i < MaxClientSendCmdSize; i++) {
                int index = _tick - i < 0 ? 1024 - i + _tick : _tick - i;

                if (_historyUserCmds[index].buttons == null) {
                    _historyUserCmds[index].buttons = new bool[UserCmd.ButtonsLength];
                    _historyUserCmds[index].Tick = index;
                    _historyUserCmds[index].ServerTick = _gameManager.GetServerTick();
                    _historyUserCmds[index].timeStamp = PhotonNetwork.ServerTimestamp;
                }

                _cmdsToSend[i] = _historyUserCmds[index];
            }

            PhotonNetwork.RaiseEvent(GameEventCode.Input, _cmdsToSend, false, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient });
        }

        if (_sender.IsLocal) {
            ProcessInputs(_curUserCmd);
            _mechCombat.ProcessInputs(_curUserCmd);

             _tick = (_tick + 1) % 1024;
            _clientHistoryPositions[_tick] = CurPosition;
            transform.position = Vector3.Lerp(transform.position, CurPosition, Time.deltaTime * LerpPosSpeed);//todo : move to mech controller
        }

        _serverHistoryPositions[(_gameManager.GetServerTick() + 1) % 1024] = CurPosition;
    }

    private void GetInputs() {
        _curUserCmd.horizontal = Input.GetAxisRaw("Horizontal");
        _curUserCmd.vertical = Input.GetAxisRaw("Vertical");
        _curUserCmd.rot = _mechCamera.transform.eulerAngles;
        _curUserCmd.viewAngle = transform.rotation.eulerAngles.y;
        _curUserCmd.msec = Time.deltaTime;
        _curUserCmd.timeStamp = PhotonNetwork.ServerTimestamp;

        _curUserCmd.buttons[(int)UserButton.Space] = Input.GetKey(KeyCode.Space);
        _curUserCmd.buttons[(int)UserButton.LeftShift] = Input.GetKey(KeyCode.LeftShift);
        _curUserCmd.buttons[(int)UserButton.R] = Input.GetKey(KeyCode.R);
        _curUserCmd.buttons[(int)UserButton.LeftMouse] = Input.GetMouseButton(0);
        _curUserCmd.buttons[(int)UserButton.RightMouse] = Input.GetMouseButton(1);
    }

    private void ProcessInputs(usercmd userCmd) {
        transform.Rotate(Vector3.up, userCmd.viewAngle - transform.rotation.eulerAngles.y);

        CurPosition = _mechController.UpdatePosition(CurPosition, userCmd);
    }

    private void ConfirmPosition(confirmData data) {
        int tick = data.ClientTick;
        Vector3 position = data.position;

        if (Vector3.Distance(_clientHistoryPositions[tick], position) > CorrectPositionThreshold) {
            int tmpTick = tick;

            //Vector3 prePos = _curPosition;

            Debug.Log("***Force pos : " + position + " on tick : " + tmpTick + " world :" + tick + " err : " + Vector3.Distance(_serverHistoryPositions[tick], position));

            //Adjust
            _clientHistoryPositions[tick] = position;
            CurPosition = position;

            switch (data.state) {
                case 0:
                _mechController.SetMovementState(_mechController.GroundedState);
                break;
                case 1:
                _mechController.SetMovementState(_mechController.JumpState);
                break;
            }

            ConfirmData.TransformStructToMechData(data, ref _mechController, ref _mechCombat);

            while (tmpTick != this._tick) {
                ProcessInputs(_historyUserCmds[tmpTick]);

                tmpTick = (tmpTick + 1) % 1024;

                _clientHistoryPositions[(_historyUserCmds[tmpTick].ServerTick + 1) % 1024] = CurPosition;
            }

            //Vector3 afterPos = _curPosition;

            //_curPosition = Vector3.Lerp(prePos, afterPos, Time.deltaTime * CorrectingSpeed);
        }
    }

    //public Vector3 GetPosition(float time){
        //while()
    //}

    private void OnDestroy() {
        PhotonNetwork.OnEventCall -= OnPhotonEvent;
        if (_gameManager != null) _gameManager.OnWorldUpdate -= OnWorldUpdate;
    }
}