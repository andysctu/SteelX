using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class HandleInputs : MonoBehaviour {
    [SerializeField] private PhotonView _rootPv;
    private MechController _mechController;
    private MechCombat _mechCombat;
    private GameManager _gameManager;

    private int _actorId;//For master client to check if inputs are sent from this

    //Client
    private enum ClientData { MSec, Horizontal, Vertical, ViewAngle, Tick, ButtonByte };
    private readonly Hashtable[] _commandsToSend = new Hashtable[4];
    private UserCmd _curUserCmd;
    public float SendRate = 0.03f;
    private float _preSendTime;

    //Master
    private enum ConfirmData { Position, Speed, State, IsVerBoostAvailable, VerBoostStartYPos, JumpReleased, CurBoostingSpeed, IsBoosting, EN, Tick};
    private readonly Hashtable _confirmInfo = new Hashtable();
    public float SendConfirmRate = 0.05f;
    private float _preConfirmTime;

    public enum Button { Space, LeftShift };

    private readonly UserCmd[] _historyUserCmds = new UserCmd[1024];
    private readonly Vector3[] _historyPositions = new Vector3[1024];
    private int _tick;

    public struct UserCmd {
        public float msec;
        public float Horizontal;
        public float Vertical;
        public float ViewAngle;
        public bool[] Buttons;
    }

    private void Awake() {
        if (PhotonNetwork.isMasterClient || _rootPv.isMine) {
            RegisterInputEvent();
        }

        _mechController = GetComponent<MechController>();
        _mechCombat = GetComponent<MechCombat>();

        _curUserCmd.Buttons = new bool[2];

        _actorId = _rootPv.ownerId;
        enabled = _rootPv.isMine;//enable determine whether to send the client data to master
    }

    private void RegisterInputEvent() {
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
    }

    private void Start() {
        _gameManager = FindObjectOfType<GameManager>();

        for (int i = 0; i < _commandsToSend.Length; i++)
            _commandsToSend[i] = new Hashtable();
    }

    protected void OnPhotonEvent(byte eventCode, object content, int senderId) {
        switch (eventCode) {
            case GameEventCode.INPUT:
            UpdateCurInputs((Hashtable[])content, senderId);
            break;
            case GameEventCode.POS_CONFIRM:
            ConfirmPosition((Hashtable)content);
            break;
        }
    }

    private void UpdateCurInputs(Hashtable[] tables, int senderId) {
        if (senderId != _actorId) return;//this is not his mech

        int clientTick = (int)tables[0][(int)ClientData.Tick];

        int unProcessedPackageCount = 0;

        if (clientTick - _tick < 0) {
            unProcessedPackageCount = 1024 - _tick + clientTick;
            if (unProcessedPackageCount >= 4) {
                Debug.LogError("Package loss : "+ (1024 - _tick + clientTick - 3));
                unProcessedPackageCount = 3;
            }
        } else if (clientTick - _tick < 4) {
            unProcessedPackageCount = clientTick - _tick;
        } else if (clientTick - _tick >= 4) {
            Debug.LogError("Package loss : "+(clientTick - _tick - 3));
            unProcessedPackageCount = 3;
        }

        _tick = (clientTick - unProcessedPackageCount) % 1024;

        for (int i = 0; i <= unProcessedPackageCount; i++) {
            _curUserCmd.msec = (float)tables[unProcessedPackageCount - i][(int)ClientData.MSec];
            _curUserCmd.Horizontal = (float)tables[unProcessedPackageCount - i][(int)ClientData.Horizontal];
            _curUserCmd.Vertical = (float)tables[unProcessedPackageCount - i][(int)ClientData.Vertical];
            _curUserCmd.ViewAngle = (float)tables[unProcessedPackageCount - i][(int)ClientData.ViewAngle];

            byte button = (byte)tables[unProcessedPackageCount - i][(int)ClientData.ButtonByte];
            Array.Copy(ConvertByteToBoolArray(button), 0, _curUserCmd.Buttons, 0, 2);

            ProcessInputs(_curUserCmd);

            _tick = (_tick + 1) % 1024;

            //Debug.Log("Server tick : " + _tick + ", Pos : " + transform.position + " Calculated from msec : " + _curUserCmd.msec
            //          + ", hor : " + _curUserCmd.Horizontal + ", ver :" + _curUserCmd.Vertical + ", space : " + _curUserCmd.Buttons[(int)Button.Space]);
            //Debug.Log("Server tick : " + _tick + ", Pos : " + transform.position);
        }

        if (Time.time - _preConfirmTime > SendConfirmRate){
            _preConfirmTime = Time.time;
            
            //Send new pos back to the client
            _confirmInfo[(int)ConfirmData.Tick] = _tick;
            _confirmInfo[(int)ConfirmData.Position] = transform.position;
            _confirmInfo[(int)ConfirmData.Speed] = new Vector3(_mechController.XSpeed, _mechController.YSpeed, _mechController.ZSpeed);
            _confirmInfo[(int)ConfirmData.EN] = _mechCombat.CurrentEN;
            _confirmInfo[(int)ConfirmData.IsVerBoostAvailable] = _mechController.IsAvailableVerBoost;
            _confirmInfo[(int)ConfirmData.State] = _mechController.CurMovementState != null && _mechController.CurMovementState == _mechController.JumpState ? 1 : 0;
            _confirmInfo[(int)ConfirmData.VerBoostStartYPos] = _mechController.VerticalBoostStartYPos;
            _confirmInfo[(int)ConfirmData.JumpReleased] = _mechController.JumpReleased;
            _confirmInfo[(int)ConfirmData.CurBoostingSpeed] = _mechController.CurBoostingSpeed;
            _confirmInfo[(int)ConfirmData.IsBoosting] = _mechController.IsBoosting;


            RaiseEventOptions options = new RaiseEventOptions();
            options.TargetActors = new[] { _actorId };
            PhotonNetwork.RaiseEvent(GameEventCode.POS_CONFIRM, _confirmInfo, false, options);
        }
    }

    private void Update() {
        GetInputs();

        //Override inputs if blocking
        if (_gameManager.BlockInput) {
            _curUserCmd.Horizontal = 0;
            _curUserCmd.Vertical = 0;
            _curUserCmd.Buttons[(int)Button.Space] = false;
            _curUserCmd.Buttons[(int)Button.LeftShift] = false;
        }

        _historyPositions[_tick] = transform.position;
        _historyUserCmds[_tick] = _curUserCmd;
        _historyUserCmds[_tick].Buttons = new bool[2];
        _curUserCmd.Buttons.CopyTo(_historyUserCmds[_tick].Buttons, 0);

        //Client send inputs to master
        if (!PhotonNetwork.isMasterClient && Time.time - _preSendTime > SendRate) {
            _preSendTime = Time.time;

            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;

            int index = 0;
            for (int i = 0; i < 4; i++) {
                index = _tick - i < 0 ? 1024 - i + _tick : _tick - i;

                if (_historyUserCmds[index].msec != 0) {
                    Byte button = ConvertBoolArrayToByte(_historyUserCmds[index].Buttons);
                    _commandsToSend[i][(int)ClientData.MSec] = _historyUserCmds[index].msec;
                    _commandsToSend[i][(int)ClientData.Horizontal] = _historyUserCmds[index].Horizontal;
                    _commandsToSend[i][(int)ClientData.Vertical] = _historyUserCmds[index].Vertical;
                    _commandsToSend[i][(int)ClientData.ViewAngle] = _historyUserCmds[index].ViewAngle;
                    _commandsToSend[i][(int)ClientData.Tick] = index;
                    _commandsToSend[i][(int)ClientData.ButtonByte] = button;
                } else {//stacked packages not enough => filled with empty
                    Byte button = ConvertBoolArrayToByte(new bool[2] { false, false });
                    _commandsToSend[i][(int)ClientData.MSec] = 0;
                    _commandsToSend[i][(int)ClientData.Horizontal] = 0;
                    _commandsToSend[i][(int)ClientData.Vertical] = 0;
                    _commandsToSend[i][(int)ClientData.ViewAngle] = 0;
                    _commandsToSend[i][(int)ClientData.Tick] = index;
                    _commandsToSend[i][(int)ClientData.ButtonByte] = button;
                }
            }
            PhotonNetwork.RaiseEvent(GameEventCode.INPUT, _commandsToSend, false, options);
        }

        if (_rootPv.isMine) {
            ProcessInputs(_curUserCmd);

            _tick = (_tick + 1) % 1024;

            //Debug.Log("Client tick : " + _tick +", Pos : "+ transform.position + "Calculated from : msec : "+CurUserCmd.msec + ", hor : "+CurUserCmd.Horizontal + ", ver : "+CurUserCmd.Vertical);
        }
    }

    private void GetInputs() {
        _curUserCmd.Horizontal = Input.GetAxis("Horizontal");
        _curUserCmd.Vertical = Input.GetAxis("Vertical");
        _curUserCmd.ViewAngle = transform.rotation.eulerAngles.y;
        _curUserCmd.msec = Time.deltaTime;

        _curUserCmd.Buttons[(int)Button.Space] = Input.GetKey(KeyCode.Space);
        _curUserCmd.Buttons[(int)Button.LeftShift] = Input.GetKey(KeyCode.LeftShift);
    }

    private void ProcessInputs(UserCmd userCmd) {
        transform.Rotate(Vector3.up, userCmd.ViewAngle - transform.rotation.eulerAngles.y);

        _mechController.UpdatePosition(userCmd);
    }

    public float AdjustPositionThreshold = 0.1f;

    private void ConfirmPosition(Hashtable hashtable) {
        int tick = (int)hashtable[(int)ConfirmData.Tick];
        Vector3 position = (Vector3)hashtable[(int)ConfirmData.Position];
        
        //TODO : EN

        if (Vector3.Distance(_historyPositions[tick], position) > AdjustPositionThreshold) {
            //Debug.Log("Tick " + tick +" has dif : " + Vector3.Distance(historyPositions[tick], position) + " curTick : "+_tick);
            //Debug.Log("History : "+ historyPositions[tick] + " serverPos : "+position);
            //Rewind
            int tmpTick = tick;

            Vector3 prePos = transform.position;

            Debug.Log("***Force pos : " + position + " on tick : " + tmpTick + " diff : " + Vector3.Distance(_historyPositions[tick], position));

            //Adjust
            _historyPositions[tmpTick] = position;
            transform.position = _historyPositions[tmpTick];

            _mechController.SetMovementState((int)hashtable[(int)ConfirmData.State] == 0 ? (MechController.MovementState)_mechController.GroundedState : _mechController.JumpState );
            _mechController.SetVerBoostStartPos((float)hashtable[(int)ConfirmData.VerBoostStartYPos]);
            _mechController.SetAvailableToBoost((bool)hashtable[(int)ConfirmData.IsVerBoostAvailable]);
            _mechController.SetSpeed((Vector3)hashtable[(int)ConfirmData.Speed]);
            _mechController.JumpReleased = (bool)hashtable[(int)ConfirmData.JumpReleased];

            _mechController.CurBoostingSpeed = (float)hashtable[(int)ConfirmData.CurBoostingSpeed];
            _mechController.IsBoosting = (bool)hashtable[(int)ConfirmData.IsBoosting];

            while (tmpTick != this._tick) {
                ProcessInputs(_historyUserCmds[tmpTick]);

                Debug.Log("Calculated from : " + _historyUserCmds[tmpTick].msec +
                          "ms , hor : " + _historyUserCmds[tmpTick].Horizontal + " , ver : " +
                          _historyUserCmds[tmpTick].Vertical + " space : " + _historyUserCmds[tmpTick].Buttons[(int)Button.Space]);

                tmpTick = (tmpTick + 1) % 1024;

                Debug.Log("=> Tick : " + tmpTick + " Pos : " + transform.position);

                _historyPositions[tmpTick] = transform.position;
            }

            //Vector3 afterPos = transform.position;

            //transform.position = Vector3.Lerp(prePos, afterPos, Time.deltaTime * 8);
        }
    }

    private static byte ConvertBoolArrayToByte(bool[] source) {
        byte result = 0;
        // This assumes the array never contains more than 8 elements!
        int index = 0;

        // Loop through the array
        foreach (bool b in source) {
            // if the element is 'true' set the bit at that position
            if (b)
                result |= (byte)(1 << (7 - index));

            index++;
        }

        return result;
    }

    private static bool[] ConvertByteToBoolArray(byte b) {
        // prepare the return result
        bool[] result = new bool[8];

        // check each bit in the byte. if 1 set to true, if 0 set to false
        for (int i = 0; i < 8; i++)
            result[i] = (b & (1 << i)) == 0 ? false : true;

        // reverse the array
        Array.Reverse(result);

        return result;
    }
}


