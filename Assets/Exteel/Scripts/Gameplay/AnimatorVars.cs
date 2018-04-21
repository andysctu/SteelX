using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorVars : MonoBehaviour {
	
	private Combo Combo;
	[HideInInspector]public CharacterController cc = null;
	[HideInInspector]public MechController mctrl = null;
	[HideInInspector]public MechCombat mcbt = null;
	[HideInInspector]public Sounds Sounds = null;
	[HideInInspector]public MechIK mechIK = null;
	[HideInInspector]public EffectController EffectController = null;

	[HideInInspector]public int boost_id;
	[HideInInspector]public int grounded_id;
	[HideInInspector]public int jump_id;
	[HideInInspector]public int speed_id;
	[HideInInspector]public int direction_id;
	[HideInInspector]public int onMelee_id;

	[HideInInspector]public int SlashL_id;
	[HideInInspector]public int SlashL2_id;
	[HideInInspector]public int SlashL3_id;
	[HideInInspector]public int SlashL4_id;
	[HideInInspector]public int SlashL5_id;

	[HideInInspector]public int SlashR_id;
	[HideInInspector]public int SlashR2_id;
	[HideInInspector]public int SlashR3_id;
	[HideInInspector]public int SlashR4_id;
	[HideInInspector]public int SlashR5_id;

	[HideInInspector]public int BCNPose_id;
	[HideInInspector]public int OnBCN_id;

	public bool inHangar = false;//in Store also manually set this to TRUE
	// Use this for initialization
	void Start () {
		if(transform.root.GetComponent<PhotonView>().isMine)
			cc = transform.parent.gameObject.GetComponent<CharacterController> ();
		mctrl = transform.parent.gameObject.GetComponent<MechController> ();
		mcbt = transform.parent.gameObject.GetComponent<MechCombat> ();
		Sounds = GetComponent<Sounds> ();
		mechIK = GetComponent<MechIK> ();
		EffectController = transform.root.GetComponentInChildren<EffectController> ();
		Combo = GetComponent<Combo> ();

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
			if(EffectController!=null){
				EffectController.InitVars ();
			}
		}
	}
}
