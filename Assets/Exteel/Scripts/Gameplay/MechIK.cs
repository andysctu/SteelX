using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class MechIK : MonoBehaviour {
	
	[SerializeField]Camera cam;
	[SerializeField]MechCombat mechCombat;
	[SerializeField]BuildMech bm;
	Transform shoulderL,shoulderR;
	float idealweight = 0;

	[SerializeField]Transform upperArmL, upperArmR;
	private Vector3 upperArmL_rot, upperArmR_rot;
	private float rotOffsetL, rotOffsetR;
	private float ideal_roL, ideal_roR;
	public bool onTargetL, onTargetR;//TODO : implement this

	//AimIK
	[SerializeField]AimIK AimIK;
	[SerializeField]Transform Target;
	public Transform PoleTarget, AimTransform;

	private int mode = 0;//mode 0 : one hand weapon ; 1 : BCN ; 2 : RCL
	private bool isIKset = false;
	private bool isOnTargetL = false, isOnTargetR = false , LeftIK_on = false, RightIK_on = false;
	private int weaponOffset = 0;

	// Use this for initialization
	void Start () {
		AimIK = GetComponent<AimIK> ();
		InitTransforms ();
	}

	void InitTransforms(){
		upperArmL = transform.Find ("metarig/hips/spine/chest/shoulder.L/upper_arm.L");
		upperArmR = transform.Find ("metarig/hips/spine/chest/shoulder.R/upper_arm.R");
	}

	void LateUpdate(){
		if (LeftIK_on) {
			ideal_roL = Vector3.SignedAngle(cam.transform.forward, transform.forward, transform.right);
			ideal_roL = Mathf.Clamp (ideal_roL, -50, 40);

			upperArmL_rot = upperArmL.localRotation.eulerAngles;
			upperArmL.localRotation = Quaternion.Euler (upperArmL_rot + new Vector3 (0, rotOffsetL + ideal_roL, 0));
		}

		if (RightIK_on) {
			ideal_roR = - Vector3.SignedAngle(cam.transform.forward, transform.forward, transform.right);
			ideal_roR = Mathf.Clamp (ideal_roR, -50, 40);

			upperArmR_rot = upperArmR.localRotation.eulerAngles;
			upperArmR.localRotation = Quaternion.Euler (upperArmR_rot + new Vector3 (0, rotOffsetR + ideal_roR, 0));
		}

	}

	void Update () {
		if (LeftIK_on) {//case 1&2 => leftIK_on
			switch (mode) {
			case 1:
				AimIK.solver.IKPositionWeight = Mathf.Lerp (AimIK.solver.IKPositionWeight, idealweight, Time.deltaTime * 5);
				Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3 (0, 10, 0);

				if (idealweight == 0 && AimIK.solver.IKPositionWeight < 0.1f) {
					LeftIK_on = false;
					AimIK.solver.IKPositionWeight = 0;
				}
				break;
			case 2:
				Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3 (0, 10, 0);
				AimIK.solver.IKPositionWeight = Mathf.Lerp (AimIK.solver.IKPositionWeight, 0, Time.deltaTime * 2);
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
	public void SetIK(bool b, int mode, int hand){//mode 0 : one hand weapon ; 1 : BCN ; 2 : RCL
		this.mode = mode;
		if (b) {
			switch(mode){
			case 0:
				if(hand==0){
					LeftIK_on = true;
				}else{
					RightIK_on = true;
				}
				break;
			case 1:
				Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3 (0, 10, 0);
				AimIK.solver.IKPositionWeight = 0;
				idealweight = 1;
				LeftIK_on = true;
				break;
			case 2:
				Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3 (0, 10, 0) ;
				AimIK.solver.IKPositionWeight = 0.8f;
				idealweight = 1;
				LeftIK_on = true;
				break;
			}

			Update ();
		}else{
			switch(mode){
			case 0:
				if(hand==0){
					LeftIK_on = false;
					ideal_roL = 0;
				}else{
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

	public void UpdateMechIK(){
		weaponOffset = mechCombat.GetCurrentWeaponOffset ();
		if(bm.weaponScripts[weaponOffset].isTwoHanded){
			AimTransform = bm.weapons [weaponOffset].transform.Find ("AimTransform");//TODO : update when switchweapon
			if (AimTransform == null)
				Debug.Log ("null aim Transform");
			else
				AimIK.solver.transform = AimTransform;

			PoleTarget = bm.weapons [weaponOffset].transform.Find ("End");
			if (PoleTarget == null)
				Debug.Log ("null PoleTarget");
			else
				AimIK.solver.poleTarget = PoleTarget;
		}else{
			mode = 0;
		}

		AimIK.solver.IKPositionWeight = 0;
		LeftIK_on = false;
		RightIK_on = false;
		idealweight = 0;
	}
}
