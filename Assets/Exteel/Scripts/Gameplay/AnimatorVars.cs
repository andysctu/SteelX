using UnityEngine;

public class AnimatorVars : MonoBehaviour {
    [HideInInspector] public CharacterController cc = null;
    [HideInInspector] public MechCombat Mcbt = null;
    [HideInInspector] public MechController Mctrl = null;
    [HideInInspector] public HandleInputs HandleInputs = null;
    [HideInInspector] public Sounds Sounds = null;
    [HideInInspector] public MechIK MechIK = null;
    [HideInInspector] public EffectController EffectController = null;
    public PhotonView RootPv;

    [HideInInspector] public int BoostHash;
    [HideInInspector] public int GroundedHash;
    [HideInInspector] public int JumpHash;
    [HideInInspector] public int SpeedHash;
    [HideInInspector] public int DirectionHash;
    [HideInInspector] public int OnMeleeHash;

    [HideInInspector] public int BlockLHash;
    [HideInInspector] public int BlockRHash;

    [HideInInspector] public int SlashHash, FinalSlashHash;

    [HideInInspector] public int CnPoseHash;
    [HideInInspector] public int CnShootHash;
    [HideInInspector] public int CnLoadHash;

    public bool inHangar = false;//in Store also manually set this to TRUE

    private void Awake() {
        if(inHangar)return;
        FindComponents();
        HashAnimatorVars();
    }

    private void FindComponents() {
        cc = transform.parent.GetComponent<CharacterController>();
        Mctrl = transform.parent.GetComponent<MechController>();
        Mcbt = transform.parent.GetComponent<MechCombat>();
        HandleInputs = transform.parent.GetComponent<HandleInputs>();
        Sounds = GetComponent<Sounds>();
        MechIK = GetComponent<MechIK>();
        EffectController = transform.root.GetComponentInChildren<EffectController>();
    }

    private void HashAnimatorVars() {
        if (inHangar || (!RootPv.isMine && !PhotonNetwork.isMasterClient) )return;

        BoostHash = Animator.StringToHash("Boost");
        GroundedHash = Animator.StringToHash("Grounded");
        JumpHash = Animator.StringToHash("Jump");
        DirectionHash = Animator.StringToHash("Direction");
        OnMeleeHash = Animator.StringToHash("OnMelee");
        SpeedHash = Animator.StringToHash("Speed");

        BlockLHash = Animator.StringToHash("BlockL");
        BlockRHash = Animator.StringToHash("BlockR");

        SlashHash = Animator.StringToHash("Slash");
        FinalSlashHash = Animator.StringToHash("FinalSlash");

        CnPoseHash = Animator.StringToHash("CnPose");
        CnShootHash = Animator.StringToHash("CnShoot");
        CnLoadHash = Animator.StringToHash("CnLoad");
    }
}