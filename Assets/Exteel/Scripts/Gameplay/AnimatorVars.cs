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

	[HideInInspector]public int slashL_id;
	[HideInInspector]public int slashL2_id;
	[HideInInspector]public int slashL3_id;
	[HideInInspector]public int slashL4_id;

	[HideInInspector]public int slashR_id;
	[HideInInspector]public int slashR2_id;
	[HideInInspector]public int slashR3_id;
	[HideInInspector]public int slashR4_id;

	[HideInInspector]public int BCNPose_id;
	[HideInInspector]public int OnBCN_id;

	public bool inHangar = false;//in Store also manually set this to TRUE

	void Start () {
		FindComponents ();
		HashAnimatorVars ();
	}

	void FindComponents(){
		if(transform.root.GetComponent<PhotonView>().isMine)
			cc = transform.parent.gameObject.GetComponent<CharacterController> ();
		
		mctrl = transform.parent.gameObject.GetComponent<MechController> ();
		mcbt = transform.parent.gameObject.GetComponent<MechCombat> ();
		Sounds = GetComponent<Sounds> ();
		mechIK = GetComponent<MechIK> ();
		EffectController = transform.root.GetComponentInChildren<EffectController> ();
		Combo = GetComponent<Combo> ();
	}

	void HashAnimatorVars(){
		if (inHangar || !transform.root.GetComponent<PhotonView> ().isMine)
			return;
		
		boost_id = Animator.StringToHash ("Boost");
		grounded_id = Animator.StringToHash ("Grounded");
		jump_id = Animator.StringToHash ("Jump");
		direction_id = Animator.StringToHash ("Direction");
		onMelee_id = Animator.StringToHash ("OnMelee");
		speed_id = Animator.StringToHash ("Speed");

		slashL_id = Animator.StringToHash ("SlashL");
		slashL2_id = Animator.StringToHash ("SlashL2");
		slashL3_id = Animator.StringToHash ("SlashL3");
		slashL4_id =  Animator.StringToHash ("SlashL4");

		slashR_id = Animator.StringToHash ("SlashR");
		slashR2_id = Animator.StringToHash ("SlashR2");
		slashR3_id = Animator.StringToHash ("SlashR3");
		slashR4_id = Animator.StringToHash ("SlashR4");

		BCNPose_id = Animator.StringToHash ("BCNPose");
		OnBCN_id = Animator.StringToHash ("OnBCN");
	}
}
