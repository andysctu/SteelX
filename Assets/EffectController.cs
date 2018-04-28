using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour {

	[SerializeField]private ParticleSystem switchWeaponEffectL, switchWeaponEffectR;
	[SerializeField]private ParticleSystem boostingDust, respawnEffect;
	[SerializeField]private Sounds Sounds;
	[SerializeField]private Animator Animator;
	[SerializeField]AnimatorVars AnimatorVars;
	private MechController mctrl;
	private Transform[] Hands;
	private int speed_id, direction_id;

	// Use this for initialization
	void Start () {
		initComponents ();
		initTransforms ();
	}

	public void InitVars(){//called by AnimatorVaars
		speed_id = AnimatorVars.speed_id;
		direction_id = AnimatorVars.direction_id;
	}

	void initComponents(){
		mctrl = transform.root.GetComponent<MechController> ();
	}
		
	void initTransforms(){
		Transform shoulderL, shoulderR;
		shoulderL = transform.root.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.root.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		Hands = new Transform[2];
		Hands [0] = shoulderL.Find ("upper_arm.L/forearm.L/hand.L");
		Hands [1] = shoulderR.Find ("upper_arm.R/forearm.R/hand.R");


		switchWeaponEffectL.transform.SetParent (Hands [0]);
		switchWeaponEffectL.transform.localPosition = Vector3.zero;

		switchWeaponEffectR.transform.SetParent (Hands [1]);
		switchWeaponEffectR.transform.localPosition = Vector3.zero;
	}

	public void SwitchWeaponEffect(){
		Sounds.PlaySwitchWeapon ();
		switchWeaponEffectL.Play ();
		switchWeaponEffectR.Play ();
	}

	public void BoostingDustEffect(bool b){//controlled by horizontal boosting state
		if (b) {
			UpdateBoostingDust ();
			boostingDust.Play ();
		}else
			boostingDust.Stop ();
	}

	public void UpdateBoostingDust(){//called by horizontal boosting state
		boostingDust.transform.localRotation = Quaternion.Euler (-90,Vector3.SignedAngle (Vector3.up, new Vector3 (-Animator.GetFloat("Direction"), Animator.GetFloat("Speed"), 0), Vector3.forward),90);
	}

	public void RespawnEffect(){
		respawnEffect.Play ();
	}

	void OnEnable(){
		RespawnEffect ();
	}
}
