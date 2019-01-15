using UnityEngine;

public class AnimatorVars : MonoBehaviour {
    [HideInInspector] public CharacterController cc = null;
    [HideInInspector] public MechCombat Mcbt = null;
    [HideInInspector] public MechController Mctrl = null;
    [HideInInspector] public HandleInputs HandleInputs = null;
    [HideInInspector] public Sounds Sounds = null;
    [HideInInspector] public MechIK MechIK = null;
    [HideInInspector] public EffectController EffectController = null;

    [HideInInspector] public int BoostHash;
    [HideInInspector] public int GroundedHash;
    [HideInInspector] public int JumpHash;
    [HideInInspector] public int SpeedHash;
    [HideInInspector] public int DirectionHash;
    [HideInInspector] public int AngleHash;

    [HideInInspector] public int BlockLHash;
    [HideInInspector] public int BlockRHash;

    [HideInInspector] public int SlashLHash, SlashRHash;
    [HideInInspector] public int SmashLHash, SmashRHash;

    [HideInInspector] public int CnPoseHash;
    [HideInInspector] public int CnShootHash;
    [HideInInspector] public int CnLoadHash;

    public bool inHangar = false;//in Store also manually set this to TRUE

    private void Awake() {
        FindComponents();
        HashAnimatorVars();
    }

    private void FindComponents() {
        cc = GetComponent<CharacterController>();
        Mctrl = GetComponent<MechController>();
        Mcbt = GetComponent<MechCombat>();
        HandleInputs = GetComponent<HandleInputs>();
        Sounds = GetComponent<Sounds>();
        MechIK = GetComponent<MechIK>();
        EffectController = transform.root.GetComponentInChildren<EffectController>();
    }

    private void HashAnimatorVars() {
        if (inHangar)return;
        //|| (!RootPv.isMine && !PhotonNetwork.isMasterClient) 
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
    }
}