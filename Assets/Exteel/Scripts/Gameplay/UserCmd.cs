using System;
using ExitGames.Client.Photon;
using UnityEngine;

public struct usercmd {
    public float msec;
    public float horizontal;
    public float vertical;
    public float viewAngle;//mech angle
    public int timeStamp;
    public Vector3 rot;//cam rot
    public bool[] buttons;
    public int Tick, ServerTick;
}

public enum UserButton { LeftShift, Space, LeftMouse, RightMouse, R, Num1, Num2, Num3, Num4};

public static class UserCmd {
    public const int ButtonsLength = 5;

    public static void RegisterType() {
        PhotonPeer.RegisterType(typeof(usercmd), (byte)'I', SerializeUserCmd, DeserializeUserCmd);
    }

    private static readonly byte[] memUserCmd = new byte[8 * 4 + 2 + 4*2];
    private static short SerializeUserCmd(StreamBuffer outStream, object customobject) {
        usercmd cmd = (usercmd)customobject;

        int index = 0;
        lock (memUserCmd) {
            byte[] bytes = memUserCmd;
            Protocol.Serialize(cmd.msec, bytes, ref index);
            Protocol.Serialize(cmd.horizontal, bytes, ref index);
            Protocol.Serialize(cmd.vertical, bytes, ref index);
            Protocol.Serialize(cmd.viewAngle, bytes, ref index);
            Protocol.Serialize(cmd.timeStamp, bytes, ref index);
            Protocol.Serialize(cmd.rot.x, bytes, ref index);
            Protocol.Serialize(cmd.rot.y, bytes, ref index);
            Protocol.Serialize(cmd.rot.z, bytes, ref index);

            short button = ConvertBoolArrayToShort(cmd.buttons);
            Protocol.Serialize(button, bytes, ref index);

            Protocol.Serialize(cmd.Tick, bytes, ref index);
            Protocol.Serialize(cmd.ServerTick, bytes, ref index);

            outStream.Write(bytes, 0, 8 * 4 + 2 + 4 * 2);
        }

        return 8 * 4 + 2 + 4 * 2;
    }

    private static object DeserializeUserCmd(StreamBuffer inStream, short length) {
        usercmd cmd = new usercmd();

        lock (memUserCmd) {
            inStream.Read(memUserCmd, 0, 8 * 4 + 2 + 4 * 2);
            int index = 0;
            Protocol.Deserialize(out cmd.msec, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.horizontal, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.vertical, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.viewAngle, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.timeStamp, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.rot.x, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.rot.y, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.rot.z, memUserCmd, ref index);

            short button;
            Protocol.Deserialize(out button, memUserCmd, ref index);
            cmd.buttons = ConvertShortToBoolArray(button);

            Protocol.Deserialize(out cmd.Tick, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.ServerTick, memUserCmd, ref index);
        }

        return cmd;
    }

    private static short ConvertBoolArrayToShort(bool[] source) {
        short result = 0;
        // This assumes the array never contains more than 16 elements!
        int index = 0;

        // Loop through the array
        foreach (bool b in source) {
            // if the element is 'true' set the bit at that position
            if (b)
                result |= (short)(1 << (15 - index));

            index++;
        }

        return result;
    }

    private static bool[] ConvertShortToBoolArray(short b) {
        // prepare the return result
        bool[] result = new bool[16];

        // check each bit in the byte. if 1 set to true, if 0 set to false
        for (int i = 0; i < 16; i++)
            result[i] = (b & (1 << i)) != 0;

        // reverse the array
        Array.Reverse(result);

        return result;
    }

    public static void CloneUsercmd(usercmd from, ref usercmd to){
        to.msec = from.msec;
        to.horizontal = from.horizontal;
        to.vertical = from.vertical;
        to.viewAngle = from.viewAngle;
        to.timeStamp = from.timeStamp;
        to.rot = from.rot;
        Array.Copy(from.buttons, to.buttons, ButtonsLength);
        to.Tick = from.Tick;
        to.ServerTick = from.ServerTick;
    }
}

