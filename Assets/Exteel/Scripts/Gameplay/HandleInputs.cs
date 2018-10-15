using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class HandleInputs : MonoBehaviour {
    [SerializeField] private PhotonView _rootPv;
    private MechController mctrl;
    private GameManager gm;

    private int _actorId;//For master client to check if inputs are sent from this

    private readonly Hashtable _confirmInfo = new Hashtable();
    private enum CONFIRM { Position, Tick };

    private enum CMD { MSec, Horizontal, Vertical, ViewAngle, Tick, ButtonByte };

    public UserCmd CurUserCmd;
    private bool spaceIsPressed = false;
    public enum Button { Space, LeftShift };

    private readonly UserCmd[] historyUserCmds = new UserCmd[1024];
    private readonly Vector3[] historyPositions = new Vector3[1024];
    private int _tick = 0;

    public float SendRate = 0.03f;
    private float preSendTime;

    public float SendConfirmRate = 0.05f;
    private float preConfirmTime;

    private readonly Hashtable[] _commandsToSend = new Hashtable[4];

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

        mctrl = GetComponent<MechController>();
        CurUserCmd.Buttons = new bool[2];

        _actorId = _rootPv.ownerId;
        enabled = _rootPv.isMine;
    }

    private void Start() {
        gm = FindObjectOfType<GameManager>();

        for (int i = 0; i < _commandsToSend.Length; i++) {
            _commandsToSend[i] = new Hashtable();
        }
    }

    private void RegisterInputEvent() {
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
    }

    protected void OnPhotonEvent(byte eventCode, object content, int senderId) {
        switch (eventCode) {
            case GameEventCode.INPUT:
            UpdateCurInputs((Hashtable[])content, senderId);
            break;
            case GameEventCode.POS_CONFIRM:
            int tick = (int)((Hashtable)content)[(int)CONFIRM.Tick];
            Vector3 position = (Vector3)((Hashtable)content)[(int)CONFIRM.Position];

            ConfirmPosition(tick, position);
            break;
        }
    }

    private void UpdateCurInputs(Hashtable[] tables, int senderId) {
        if (senderId != _actorId) return;//this is not his mech

        int latestTick = (int)tables[0][(int)CMD.Tick];
        int d = 0;
        if (latestTick - _tick < 0) {
            d = 1024 - _tick + latestTick;
        } else if (latestTick - _tick < 4) {
            d = latestTick - _tick;
        } else {
            d = 4;
        }

        for (int i = 0; i < 4 && i < d; i++) {
            if (d - i - 1 < 0 || d - i - 1 > 4) Debug.LogError("D : " + d + " i : " + i + " curTick : " + _tick + " latestTick : " + latestTick);

            CurUserCmd.msec = (float)tables[d - i - 1][(int)CMD.MSec];
            CurUserCmd.Horizontal = (float)tables[d - i - 1][(int)CMD.Horizontal];
            CurUserCmd.Vertical = (float)tables[d - i - 1][(int)CMD.Vertical];
            CurUserCmd.ViewAngle = (float)tables[d - i - 1][(int)CMD.ViewAngle];

            byte button = (byte)tables[d - i - 1][(int)CMD.ButtonByte];
            Array.Copy(ConvertByteToBoolArray(button), 0, CurUserCmd.Buttons, 0, 2);
            //CurUserCmd.Buttons = ConvertByteToBoolArray(button);

            ProcessInputs(CurUserCmd);

            Debug.Log("Server tick : " + ((_tick + i + 1) % 1024) + ", Pos : " + transform.position + ", msec : " + CurUserCmd.msec
                      + ", hor : " + CurUserCmd.Horizontal + ", ver :" + CurUserCmd.Vertical + ", space : " + CurUserCmd.Buttons[(int)Button.Space]);
        }

        _tick = latestTick;


        if(Time.time - preConfirmTime < SendConfirmRate)return;

        preConfirmTime = Time.time;

        //Send new pos back to the client
        _confirmInfo[(int)CONFIRM.Tick] = (latestTick + 1) % 1024;
        _confirmInfo[(int)CONFIRM.Position] = transform.position;

        RaiseEventOptions options = new RaiseEventOptions();
        options.TargetActors = new[] { _actorId };
        PhotonNetwork.RaiseEvent(GameEventCode.POS_CONFIRM, _confirmInfo, false, options);
    }

    private void Update() {
        GetInputs();

        //Override inputs if blocking
        if (gm.BlockInput) {
            CurUserCmd.Horizontal = 0;
            CurUserCmd.Vertical = 0;
            CurUserCmd.Buttons[(int)Button.Space] = false;
            CurUserCmd.Buttons[(int)Button.LeftShift] = false;
        }

        historyPositions[_tick] = transform.position;
        historyUserCmds[_tick] = CurUserCmd;
        historyUserCmds[_tick].Buttons = new bool[2];
        CurUserCmd.Buttons.CopyTo(historyUserCmds[_tick].Buttons, 0);

        //Client send inputs to master
        if (!PhotonNetwork.isMasterClient && Time.time - preSendTime > SendRate) {
            preSendTime = Time.time;

            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;

            int index = 0;
            for (int i = 0; i < 4; i++) {
                index = _tick - i < 0 ? 1024 - i + _tick : _tick - i;


                if (historyUserCmds[index].msec != 0) {
                    Byte button = ConvertBoolArrayToByte(historyUserCmds[index].Buttons);
                    _commandsToSend[i][(int)CMD.MSec] = historyUserCmds[index].msec;
                    _commandsToSend[i][(int)CMD.Horizontal] = historyUserCmds[index].Horizontal;
                    _commandsToSend[i][(int)CMD.Vertical] = historyUserCmds[index].Vertical;
                    _commandsToSend[i][(int)CMD.ViewAngle] = historyUserCmds[index].ViewAngle;
                    _commandsToSend[i][(int)CMD.Tick] = index;
                    _commandsToSend[i][(int)CMD.ButtonByte] = button;
                } else {
                    Byte button = ConvertBoolArrayToByte(new bool[2] { false, false });
                    _commandsToSend[i][(int)CMD.MSec] = 0;
                    _commandsToSend[i][(int)CMD.Horizontal] = 0;
                    _commandsToSend[i][(int)CMD.Vertical] = 0;
                    _commandsToSend[i][(int)CMD.ViewAngle] = 0;
                    _commandsToSend[i][(int)CMD.Tick] = index;
                    _commandsToSend[i][(int)CMD.ButtonByte] = button;
                }

            }
            PhotonNetwork.RaiseEvent(GameEventCode.INPUT, _commandsToSend, false, options);
        }

        if (_rootPv.isMine) {
            ProcessInputs(CurUserCmd);

            _tick = (_tick + 1) % 1024;
        }
    }

    private void GetInputs() {
        CurUserCmd.Horizontal = Input.GetAxis("Horizontal");
        CurUserCmd.Vertical = Input.GetAxis("Vertical");
        CurUserCmd.ViewAngle = transform.rotation.eulerAngles.y;
        CurUserCmd.msec = Time.deltaTime;

        CurUserCmd.Buttons[(int)Button.Space] = Input.GetKey(KeyCode.Space);
        CurUserCmd.Buttons[(int)Button.LeftShift] = Input.GetKey(KeyCode.LeftShift);
    }

    private void ProcessInputs(UserCmd userCmd) {
        transform.Rotate(Vector3.up, userCmd.ViewAngle - transform.rotation.eulerAngles.y);

        mctrl.UpdatePosition(userCmd);
    }

    public float threshold = 0.1f;

    private void ConfirmPosition(int tick, Vector3 position) {
        if (Vector3.Distance(historyPositions[tick], position) > threshold) {
            //Debug.Log("Tick " + tick +" has dif : " + Vector3.Distance(historyPositions[tick], position) + " curTick : "+_tick);
            //Debug.Log("History : "+ historyPositions[tick] + " serverPos : "+position);
            //Rewind
            int tmpTick = tick;

            Vector3 prePos = transform.position;

            //Adjust
            historyPositions[tmpTick] = position;
            transform.position = historyPositions[tmpTick];

            Debug.Log("***Force pos : " + historyPositions[tmpTick] + " on tick : " + tmpTick);

            while (tmpTick != this._tick) {
                ProcessInputs(historyUserCmds[tmpTick]);

                Debug.Log("tick : " + tmpTick + " Pos : " + transform.position);
                Debug.Log("Calculate from : " + historyUserCmds[tmpTick].msec +
                          "ms , hor : " + historyUserCmds[tmpTick].Horizontal + " , ver : " +
                          historyUserCmds[tmpTick].Vertical + " space : " + historyUserCmds[tmpTick].Buttons[(int)Button.Space]);

                tmpTick = (tmpTick + 1) % 1024;

                historyPositions[tmpTick] = transform.position;
            }

            Vector3 afterPos = transform.position;

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


