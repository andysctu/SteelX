﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorVars : MonoBehaviour {
	[SerializeField]Combo Combo;

	public CharacterController cc = null;
	public MechController mctrl = null;
	public MechCombat mcbt = null;
	public Sounds Sounds = null;
	public MechIK mechIK = null;

	public int boost_id;
	public int grounded_id;
	public int jump_id;
	public int speed_id;
	public int direction_id;
	public int onMelee_id;

	public int SlashL_id;
	public int SlashL2_id;
	public int SlashL3_id;
	public int SlashL4_id;
	public int SlashL5_id;

	public int SlashR_id;
	public int SlashR2_id;
	public int SlashR3_id;
	public int SlashR4_id;
	public int SlashR5_id;

	public int BCNPose_id;
	public int OnBCN_id;

	public bool inHangar = false;//in Store also manually set this to TRUE
	// Use this for initialization
	void Start () {
		if(transform.root.GetComponent<PhotonView>().isMine)
			cc = transform.parent.gameObject.GetComponent<CharacterController> ();
		mctrl = transform.parent.gameObject.GetComponent<MechController> ();
		mcbt = transform.parent.gameObject.GetComponent<MechCombat> ();
		Sounds = GetComponent<Sounds> ();
		mechIK = GetComponent<MechIK> ();

		if (!inHangar && transform.root.GetComponent<PhotonView>().isMine) {
			boost_id = Animator.StringToHash ("Boost");
			grounded_id = Animator.StringToHash ("Grounded");
			jump_id = Animator.StringToHash ("Jump");
			direction_id = Animator.StringToHash ("Direction");
			onMelee_id = Animator.StringToHash ("OnMelee");
			speed_id = Animator.StringToHash ("Speed");

			SlashL_id = Animator.StringToHash ("SlashL");
			SlashL2_id = Animator.StringToHash ("SlashL2");
			SlashL3_id = Animator.StringToHash ("SlashL3");
			SlashL4_id =  Animator.StringToHash ("SlashL4");
			SlashL5_id =  Animator.StringToHash ("SlashL5");

			SlashR_id = Animator.StringToHash ("SlashR");
			SlashR2_id = Animator.StringToHash ("SlashR2");
			SlashR3_id = Animator.StringToHash ("SlashR3");
			SlashR4_id = Animator.StringToHash ("SlashR4");
			SlashR5_id = Animator.StringToHash ("SlashR5");

			BCNPose_id = Animator.StringToHash ("BCNPose");
			OnBCN_id = Animator.StringToHash ("OnBCN");
			if(Combo!=null){
				Combo.InitVars ();
			}
			if(mctrl!=null){
				mctrl.InitVars ();
			}
			if(mcbt!=null){
				mcbt.initAnimatorVarID ();
			}
		}
	}
}
