using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorVars : MonoBehaviour {
	public CharacterController cc;
	public MechController mctrl;
	public MechCombat mcbt;
	public Sounds Sounds;

	public int boost_id;
	public int grounded_id;
	public int jump_id;
	public int speed_id;
	public int direction_id;
	public int onSlash_id;
	// Use this for initialization
	void Start () {
		cc = transform.parent.gameObject.GetComponent<CharacterController> ();
		mctrl = transform.parent.gameObject.GetComponent<MechController> ();
		mcbt = transform.parent.gameObject.GetComponent<MechCombat> ();
		Sounds = GetComponent<Sounds> ();

		boost_id = Animator.StringToHash ("Boost");
		grounded_id = Animator.StringToHash ("Grounded");
		jump_id = Animator.StringToHash ("Jump");
		direction_id = Animator.StringToHash ("Direction");
		onSlash_id = Animator.StringToHash ("OnSlash");
		speed_id = Animator.StringToHash ("Speed");
	}
}
