using RootMotion.FinalIK;
using UnityEngine;

public class MechIK : MonoBehaviour {
    [SerializeField] private Camera cam;
    [SerializeField] private MechCombat mechCombat;
    [SerializeField] private BuildMech bm;
    [SerializeField] private SkillController SkillController;
    private Transform hips, clavicle_L, clavicle_R, Knob;
    private Animator animator;

    //AimIK
    [SerializeField] private AimIK AimIK;
    [SerializeField] private Transform Target;
    private Transform PoleTarget, AimTransform;

    private float idealweight = 0;
    private Vector3 upperArmL_rot, upperArmR_rot;
    private float ideal_roL, ideal_roR;

    private int mode = 0, weaponOffset = 0;//mode 0 : one hand weapon ; 1 : Cannon ; 2 : Rocket
    private bool LeftIK_on = false, RightIK_on = false;
    private float weight = 1;

    private void Awake() {
        RegisterOnWeaponSwitched();
        RegisterOnSkill();
    }

    private void RegisterOnSkill() {
        if (SkillController != null) SkillController.OnSkill += ShutDownIK;
    }

    private void RegisterOnWeaponSwitched() {
        if (mechCombat != null) mechCombat.OnWeaponSwitched += UpdateMechIK;
    }

    private void Start() {
        AimIK = GetComponent<AimIK>();
        animator = transform.GetComponent<Animator>();
        InitTransforms();
    }

    private void OnAnimatorIK() {
        if (Knob != null) {
            animator.SetIKPosition(AvatarIKGoal.LeftHand, Knob.position);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
        }
    }

    private void InitTransforms() {
        hips = transform.Find("Bip01/Bip01_Pelvis");
        clavicle_L = transform.Find("Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_L_Clavicle");
        clavicle_R = transform.Find("Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_R_Clavicle");
    }

    private void LateUpdate() {
        if (LeftIK_on) {
            if (mode == 0) {
                ideal_roL = Vector3.SignedAngle(cam.transform.forward, transform.forward, transform.right);
                ideal_roL = Mathf.Clamp(ideal_roL, -50, 40);
                ideal_roL += 180 - hips.localRotation.eulerAngles.z;

                upperArmL_rot = clavicle_L.localRotation.eulerAngles;
                clavicle_L.localRotation = Quaternion.Euler(upperArmL_rot + new Vector3(ideal_roL, 0, 0));
            } else {
            }
        }

        if (RightIK_on && mode == 0) {
            ideal_roR = -Vector3.SignedAngle(cam.transform.forward, transform.forward, transform.right);
            ideal_roR = Mathf.Clamp(ideal_roR, -50, 40);
            ideal_roR += -(180 - hips.localRotation.eulerAngles.z);

            upperArmR_rot = clavicle_R.localRotation.eulerAngles;
            clavicle_R.localRotation = Quaternion.Euler(upperArmR_rot + new Vector3(ideal_roR, 0, 0));
        }
    }

    private void Update() {
        if (LeftIK_on) {//case 1&2 => leftIK_on
            switch (mode) {
                case 0:
                ideal_roL = Vector3.SignedAngle(cam.transform.forward, transform.forward, transform.right);
                ideal_roL = Mathf.Clamp(ideal_roL, -50, 40);
                ideal_roL += 180 - hips.localRotation.eulerAngles.z;

                upperArmL_rot = clavicle_L.localRotation.eulerAngles;
                clavicle_L.localRotation = Quaternion.Euler(upperArmL_rot + new Vector3(ideal_roL, 0, 0));
                break;
                case 1:
                AimIK.solver.IKPositionWeight = Mathf.Lerp(AimIK.solver.IKPositionWeight, idealweight, Time.deltaTime * 5);
                Target.position = cam.transform.forward * 1000 + transform.root.position + new Vector3(0, 10, 0);

                if (idealweight == 0 && AimIK.solver.IKPositionWeight < 0.1f) {
                    LeftIK_on = false;
                    AimIK.solver.IKPositionWeight = 0;
                }
                break;
                case 2:
                Target.position = cam.transform.position + cam.transform.forward * 1000;
                AimIK.solver.IKPositionWeight = Mathf.Lerp(AimIK.solver.IKPositionWeight, 0, Time.deltaTime * 2);
                if (AimIK.solver.IKPositionWeight < 0.01f) {
                    LeftIK_on = false;
                    AimIK.solver.IKPositionWeight = 0;
                }
                break;
                default:
                break;
            }
        }
    }

    //this is called in shooting state
    public void SetIK(bool b, int mode, int hand) {//mode 0 : one hand weapon ; 1 : BCN ; 2 : RCL
        this.mode = mode;
        if (b) {
            switch (mode) {
                case 0:
                if (hand == 0) {
                    LeftIK_on = true;
                } else {
                    RightIK_on = true;
                }
                break;
                case 1:
                Target.position = cam.transform.forward * 1000 + transform.root.position + new Vector3(0, 10, 0);
                AimIK.solver.IKPositionWeight = 0;
                idealweight = 1;
                LeftIK_on = true;
                break;
                case 2:
                Target.position = cam.transform.forward * 1000 + transform.root.position + new Vector3(0, 10, 0);
                AimIK.solver.IKPositionWeight = 1f;
                LeftIK_on = true;
                break;
            }

            Update();
        } else {
            switch (mode) {
                case 0:
                if (hand == 0) {
                    LeftIK_on = false;
                    ideal_roL = 0;
                } else {
                    RightIK_on = false;
                    ideal_roR = 0;
                }
                break;
                case 1:
                idealweight = 0;
                break;
                case 2:
                break;
            }
        }
    }

    private void ShutDownIK(bool b) {//for skill use
        LeftIK_on = false;
        RightIK_on = false;
    }

    public void UpdateMechIK() {
        weaponOffset = mechCombat.GetCurrentWeaponOffset();

        if (bm.WeaponDatas[weaponOffset].twoHanded) {
            Knob = FindKnob(bm.Weapons[weaponOffset].GetWeapon().transform);

            AimTransform = bm.Weapons[weaponOffset].GetWeapon().transform.Find("AimTransform");//TODO : update when switchweapon
            if (AimTransform == null)
                Debug.LogError("null aim Transform");
            else
                AimIK.solver.transform = AimTransform;

            PoleTarget = bm.Weapons[weaponOffset].GetWeapon().transform.Find("End");
            if (PoleTarget == null)
                Debug.Log("null PoleTarget");
            else
                AimIK.solver.poleTarget = PoleTarget;
        } else {
            Knob = null;
            mode = 0;
        }

        AimIK.solver.IKPositionWeight = 0;
        LeftIK_on = false;
        RightIK_on = false;
        idealweight = 0;
    }

    public void UpdateMechIK(int weaponOffset) {//called by buildMech
        this.weaponOffset = weaponOffset;

        if (bm.WeaponDatas[weaponOffset]!= null &&  bm.WeaponDatas[weaponOffset].twoHanded) {
            Knob = FindKnob(bm.Weapons[weaponOffset].GetWeapon().transform);
            AimTransform = bm.Weapons[weaponOffset].GetWeapon().transform.Find("AimTransform");//TODO : update when switchweapon
            if (AimTransform == null)
                Debug.LogError("null aim Transform");
            else
                AimIK.solver.transform = AimTransform;

            PoleTarget = bm.Weapons[weaponOffset].GetWeapon().transform.Find("End");
            if (PoleTarget == null)
                Debug.Log("null PoleTarget");
            else
                AimIK.solver.poleTarget = PoleTarget;
        } else {
            Knob = null;
            mode = 0;
        }

        AimIK.solver.IKPositionWeight = 0;
        LeftIK_on = false;
        RightIK_on = false;
        idealweight = 0;
    }

    private Transform FindKnob(Transform weapon) {//knob must under the first child
        Transform t = weapon;

        while (t.childCount != 0) {
            t = t.GetChild(0);
        }
        return t;
    }
}