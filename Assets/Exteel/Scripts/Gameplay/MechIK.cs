using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class MechIK : MonoBehaviour {

	[SerializeField]Transform Left_handIK, Right_handIK;
	[SerializeField]Camera cam;
	MechCombat mechCombat;
	BuildMech bm;
	Animator Animator;
	Transform shoulderL,shoulderR;
	Transform[] Hands;
	float idealweight = 0;

	//AimIK
	[SerializeField]Transform Target;
	public Transform PoleTarget, AimTransform;
	AimIK AimIK;

	private int mode = 0;//mode 0 : one hand weapon ; 1 : BCN ; 2 : RCL
	private bool isIKset = false;
	private bool isOnTargetL = false, isOnTargetR = false , LeftIK_on = false, RightIK_on = false;
	private float weight_L = 0, weight_R = 0;
	private int weaponOffset = 0;

	// Use this for initialization
	void Start () {
		bm = transform.root.GetComponent<BuildMech> ();
		mechCombat = transform.root.GetComponent<MechCombat> ();
		Animator = GetComponent<Animator> ();
		AimIK = GetComponent<AimIK> ();
		InitTransforms ();
	}

	void InitTransforms(){
		shoulderL = transform.Find("metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.Find("metarig/hips/spine/chest/shoulder.R");

		Hands = new Transform[2];
		Hands [0] = shoulderL.Find ("upper_arm.L/forearm.L/hand.L");
		Hands [1] = shoulderR.Find ("upper_arm.R/forearm.R/hand.R");
	}

	// Update is called once per frame
	void Update () {
		switch(mode){
		case 0:
			if (LeftIK_on && !isOnTargetL) {
				Left_handIK.position = (transform.root.position + new Vector3 (0, 10, 0)) + cam.transform.forward * 10;
			}

			if(RightIK_on && !isOnTargetR){
				Right_handIK.position = (transform.root.position + new Vector3 (0, 10, 0)) + cam.transform.forward * 10;
			}
			break;
		case 1:
			AimIK.solver.IKPositionWeight = Mathf.Lerp (AimIK.solver.IKPositionWeight, idealweight, Time.deltaTime * 5);
			Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3(0,10,0);

			if(idealweight==0&&AimIK.solver.IKPositionWeight<0.1f){
				AimIK.solver.IKPositionWeight = 0;
				enabled = false;
			}

			break;
		case 2:
			Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3 (0, 10, 0);
			AimIK.solver.IKPositionWeight = Mathf.Lerp (AimIK.solver.IKPositionWeight, 0, Time.deltaTime * 2);
			if (AimIK.solver.IKPositionWeight < 0.01f) {
				AimIK.solver.IKPositionWeight = 0;
				enabled = false;
			}
			break;
		}
	}

	void OnAnimatorIK(){
		switch(mode){
		case 0:

			if (!LeftIK_on && weight_L>0.1f) {
				weight_L = Mathf.Lerp (weight_L, 0, Time.deltaTime * 6);
				Animator.SetIKPosition (AvatarIKGoal.LeftHand, Left_handIK.position);
				Animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, weight_L);

				if (weight_L <= 0.01) {
					enabled = false;
					weight_L = 0;
				}
			}else{
				Animator.SetIKPosition (AvatarIKGoal.LeftHand, Left_handIK.position);
				Animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, weight_L);
			}

			if (!RightIK_on && weight_R>0.1f) {
				weight_R = Mathf.Lerp (weight_R, 0, Time.deltaTime * 6);

				Animator.SetIKPosition (AvatarIKGoal.RightHand, Right_handIK.position);
				Animator.SetIKPositionWeight (AvatarIKGoal.RightHand, weight_R);
				if (weight_R <= 0.01) {
					enabled = false;
					weight_R = 0;
				}
			}else{
				Animator.SetIKPosition (AvatarIKGoal.RightHand, Right_handIK.position);
				Animator.SetIKPositionWeight (AvatarIKGoal.RightHand, weight_R);
			}
			break;
		}
	}

	//this is called in shooting state
	public void SetIK(bool b, int mode, int hand){//mode 0 : one hand weapon ; 1 : BCN ; 2 : RCL
		this.mode = mode;
		if (b) {
			enabled = true;//auto set to false if weight too low

			if(mode==1 || mode == 2){//Final IK
				weaponOffset = mechCombat.GetCurrentWeaponOffset ();
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
			}

			switch(mode){
			case 0:
				if(hand==0){
					weight_L = 0.5f;
					LeftIK_on = true;
					Left_handIK.position = (transform.root.position + new Vector3 (0, 10, 0)) + cam.transform.forward * 10;
				}else{
					weight_R = 0.5f;
					RightIK_on = true;
					Right_handIK.position = (transform.root.position + new Vector3 (0, 10, 0)) + cam.transform.forward * 10;
				}
				break;
			case 1:
				Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3 (0, 10, 0) ;
				AimIK.solver.IKPositionWeight = 0;
				idealweight = 1;
				break;
			case 2:
				Target.position = cam.transform.forward * 100 + transform.root.position + new Vector3 (0, 10, 0) ;
				AimIK.solver.IKPositionWeight = 0.8f;
				idealweight = 1;
				break;
			}

			Update ();
		}else{
			switch(mode){
			case 0:
				if(hand==0){
					LeftIK_on = false;
				}else{
					RightIK_on = false;
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
		
	public void ResetIK(){
		LeftIK_on = false;
		RightIK_on = false;
		weight_L = 0;
		weight_R = 0;
		idealweight = 0;
	}
}
