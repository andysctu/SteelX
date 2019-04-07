using System;
//using ExitGames.Client.Photon;
using UnityEngine;

public struct confirmData
{
    public Vector3 position;
    public Vector3 speed;
    public float curBoostingSpeed;

    public int state;
    public int ClientTick;
    public float en;

    public float verBoostStartYPos;

    public float instantMoveRemainingDistance;
    public float instantMoveRemainingTime;
    public Vector3 instantMoveDir;

    public bool[] bools;
}

public enum ConfirmDataBools { IsAvailableVerBoost, jumpReleased, isBoosting};

public static class ConfirmData {
    public const int ConformDataBoolsLength = 3;

    public static void RegisterType() {
        //PhotonPeer.RegisterType(typeof(confirmData), (byte)'c', SerializeData, DeserializeData);
    }

    private static readonly byte[] memData = new byte[12 * 3 + 4 * 5 + 2 * 4 + 2];//vector3 3 * 12 , float 5 * 4 , int 2 * 4 , short 2
    //private static short SerializeData(StreamBuffer outStream, object customobject) {
    //    confirmData data = (confirmData)customobject;
	//
    //    int index = 0;
    //    lock (memData) {
    //        byte[] bytes = memData;
    //        Protocol.Serialize(data.position.x, bytes, ref index);
    //        Protocol.Serialize(data.position.y, bytes, ref index);
    //        Protocol.Serialize(data.position.z, bytes, ref index);
	//
    //        Protocol.Serialize(data.speed.x, bytes, ref index);
    //        Protocol.Serialize(data.speed.y, bytes, ref index);
    //        Protocol.Serialize(data.speed.z, bytes, ref index);
	//
    //        Protocol.Serialize(data.curBoostingSpeed, bytes, ref index);
    //        Protocol.Serialize(data.state, bytes, ref index);
    //        Protocol.Serialize(data.ClientTick, bytes, ref index);
    //        Protocol.Serialize(data.en, bytes, ref index);
	//
    //        Protocol.Serialize(data.verBoostStartYPos, bytes, ref index);
	//
    //        Protocol.Serialize(data.instantMoveRemainingDistance, bytes, ref index);
    //        Protocol.Serialize(data.instantMoveRemainingTime, bytes, ref index);
	//
    //        Protocol.Serialize(data.instantMoveDir.x, bytes, ref index);
    //        Protocol.Serialize(data.instantMoveDir.y, bytes, ref index);
    //        Protocol.Serialize(data.instantMoveDir.z, bytes, ref index);
	//
    //        if(data.bools == null)data.bools = new bool[ConformDataBoolsLength];
    //        byte bools = ConvertBoolArrayToByte(data.bools);
    //        Protocol.Serialize(bools, bytes, ref index);
	//
    //        outStream.Write(bytes, 0, 12 * 3 + 4 * 5 + 2 * 4 + 2);
    //    }
	//
    //    return 12 * 3 + 4 * 5 + 2 * 4 + 2;
    //}

    //private static object DeserializeData(StreamBuffer inStream, short length) {
    //    confirmData data = new confirmData();
	//
    //    lock (memData) {
    //        inStream.Read(memData, 0, 12 * 3 + 4 * 5 + 2 * 4 + 2);
    //        int index = 0;
	//
    //        Protocol.Deserialize(out data.position.x, memData, ref index);
    //        Protocol.Deserialize(out data.position.y, memData, ref index);
    //        Protocol.Deserialize(out data.position.z, memData, ref index);
	//
    //        Protocol.Deserialize(out data.speed.x, memData, ref index);
    //        Protocol.Deserialize(out data.speed.y, memData, ref index);
    //        Protocol.Deserialize(out data.speed.z, memData, ref index);
	//
    //        Protocol.Deserialize(out data.curBoostingSpeed, memData, ref index);
    //        Protocol.Deserialize(out data.state, memData, ref index);
    //        Protocol.Deserialize(out data.ClientTick, memData, ref index);
    //        Protocol.Deserialize(out data.en, memData, ref index);
	//
    //        Protocol.Deserialize(out data.verBoostStartYPos, memData, ref index);
	//
    //        Protocol.Deserialize(out data.instantMoveRemainingDistance, memData, ref index);
    //        Protocol.Deserialize(out data.instantMoveRemainingTime, memData, ref index);
	//
    //        Protocol.Deserialize(out data.instantMoveDir.x, memData, ref index);
    //        Protocol.Deserialize(out data.instantMoveDir.y, memData, ref index);
    //        Protocol.Deserialize(out data.instantMoveDir.z, memData, ref index);
	//
    //        short bools;
    //        Protocol.Deserialize(out bools, memData, ref index);
    //        data.bools = ConvertByteToBoolArray((byte)bools);
    //    }
	//
    //    return data;
    //}

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

    public static void TransformMechDataToStruct(MechCombat _mechCombat, MechController _mechController, ref confirmData _confirmInfo) {
        if(_confirmInfo.bools == null)_confirmInfo.bools = new bool[ConformDataBoolsLength];
        _confirmInfo.speed = new Vector3(_mechController.XSpeed, _mechController.YSpeed, _mechController.ZSpeed);
        _confirmInfo.en = _mechCombat.CurrentEN;
        _confirmInfo.bools[(int)ConfirmDataBools.IsAvailableVerBoost] = _mechController.IsAvailableVerBoost;
        _confirmInfo.state = _mechController.CurMovementState != null && _mechController.CurMovementState == _mechController.JumpState ? 1 : 0;
        _confirmInfo.verBoostStartYPos = _mechController.VerticalBoostStartYPos;
        _confirmInfo.bools[(int)ConfirmDataBools.jumpReleased] = _mechController.JumpReleased;
        _confirmInfo.curBoostingSpeed = _mechController.CurBoostingSpeed;
        _confirmInfo.bools[(int)ConfirmDataBools.isBoosting] = _mechController.IsBoosting;

        _confirmInfo.instantMoveRemainingDistance = _mechController.InstantMoveRemainingDistance;
        _confirmInfo.instantMoveDir = _mechController.InstantMoveDir;
        _confirmInfo.instantMoveRemainingTime = _mechController.InstantMoveRemainingTime;
    }

    public static void TransformStructToMechData(confirmData from, ref MechController mctrl, ref MechCombat mcbt) {
        mctrl.SetVerBoostStartPos(from.verBoostStartYPos);
        mctrl.SetAvailableToBoost(from.bools[(int)ConfirmDataBools.IsAvailableVerBoost]);
        mctrl.SetSpeed(from.speed);
        mctrl.JumpReleased = from.bools[(int)ConfirmDataBools.jumpReleased];
        mctrl.InstantMoveRemainingDistance = from.instantMoveRemainingDistance;
        mctrl.InstantMoveRemainingTime = from.instantMoveRemainingTime;
        mctrl.InstantMoveDir = from.instantMoveDir;

        mctrl.CurBoostingSpeed = from.curBoostingSpeed;
        mctrl.IsBoosting = from.bools[(int)ConfirmDataBools.isBoosting];
    }
}

