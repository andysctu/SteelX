using UnityEngine;

public class AnimatorHashVars{
    public static int BoostHash;
    public static int GroundedHash;
    public static int JumpHash;
    public static int SpeedHash;
    public static int DirectionHash;
    public static int AngleHash;

    public static int BlockLHash;
    public static int BlockRHash;

    public static int SlashLHash, SlashRHash;
    public static int SmashLHash, SmashRHash;

    public static int CnPoseHash;
    public static int CnShootHash;
    public static int CnLoadHash;
    
    private static bool _isHashed;

    public static void HashAnimatorVars() {
        if(_isHashed)return;

        BoostHash = Animator.StringToHash("Boost");
        GroundedHash = Animator.StringToHash("Grounded");
        JumpHash = Animator.StringToHash("Jump");
        DirectionHash = Animator.StringToHash("Direction");
        SpeedHash = Animator.StringToHash("Speed");
        AngleHash = Animator.StringToHash("Angle");

        BlockLHash = Animator.StringToHash("BlockL");
        BlockRHash = Animator.StringToHash("BlockR");

        SlashLHash = Animator.StringToHash("SlashL");
        SlashRHash = Animator.StringToHash("SlashR");
        SmashLHash = Animator.StringToHash("SmashL");
        SmashRHash = Animator.StringToHash("SmashR");

        CnPoseHash = Animator.StringToHash("CnPose");
        CnShootHash = Animator.StringToHash("CnShoot");
        CnLoadHash = Animator.StringToHash("CnLoad");

        _isHashed = true;
    }
}