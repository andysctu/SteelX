using System;
using ExitGames.Client.Photon;
using UnityEngine;

public struct usercmd {
    public float msec;
    public float horizontal;
    public float vertical;
    public float viewAngle;
    public bool[] buttons;
    public int Tick;
}

public enum UserButton { LeftShift, Space, LeftMouse, RightMouse, Num1, Num2, Num3, Num4};

public static class UserCmd {
    public const int ButtonsLength = 4;

    public static void RegisterType() {
        PhotonPeer.RegisterType(typeof(usercmd), (byte)'I', SerializeUserCmd, DeserializeUserCmd);
    }

    private static readonly byte[] memUserCmd = new byte[4 * 4 + 2 + 4];
    private static short SerializeUserCmd(StreamBuffer outStream, object customobject) {
        usercmd cmd = (usercmd)customobject;

        int index = 0;
        lock (memUserCmd) {
            byte[] bytes = memUserCmd;
            Protocol.Serialize(cmd.msec, bytes, ref index);
            Protocol.Serialize(cmd.horizontal, bytes, ref index);
            Protocol.Serialize(cmd.vertical, bytes, ref index);
            Protocol.Serialize(cmd.viewAngle, bytes, ref index);

            byte button = ConvertBoolArrayToByte(cmd.buttons);
            Protocol.Serialize(button, bytes, ref index);

            Protocol.Serialize(cmd.Tick, bytes, ref index);

            outStream.Write(bytes, 0, 4 * 4 + 2 + 4);
        }

        return 4 * 4 + 2 + 4;
    }

    private static object DeserializeUserCmd(StreamBuffer inStream, short length) {
        usercmd cmd = new usercmd();

        lock (memUserCmd) {
            inStream.Read(memUserCmd, 0, 4 * 4 + 2 + 4);
            int index = 0;
            Protocol.Deserialize(out cmd.msec, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.horizontal, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.vertical, memUserCmd, ref index);
            Protocol.Deserialize(out cmd.viewAngle, memUserCmd, ref index);

            short button;
            Protocol.Deserialize(out button, memUserCmd, ref index);
            cmd.buttons = ConvertByteToBoolArray((byte)button);

            Protocol.Deserialize(out cmd.Tick, memUserCmd, ref index);
        }

        return cmd;
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

