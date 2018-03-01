﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorVars : MonoBehaviour {
	public CharacterController cc = null;
	public MechController mctrl = null;
	public MechCombat mcbt = null;
	public Sounds Sounds = null;

	public int boost_id;
	public int grounded_id;
	public int jump_id;
	public int speed_id;
	public int direction_id;
	public int onSlash_id;

	public bool inHangar = false;
	// Use this for initialization
	void Start () {
		cc = transform.parent.gameObject.GetComponent<CharacterController> ();
		mctrl = transform.parent.gameObject.GetComponent<MechController> ();
		mcbt = transform.parent.gameObject.GetComponent<MechCombat> ();
		Sounds = GetComponent<Sounds> ();

		if (!inHangar) {
			boost_id = Animator.StringToHash ("Boost");
			grounded_id = Animator.StringToHash ("Grounded");
			jump_id = Animator.StringToHash ("Jump");
			direction_id = Animator.StringToHash ("Direction");
			onSlash_id = Animator.StringToHash ("OnSlash");
			speed_id = Animator.StringToHash ("Speed");
		}
	}
}
