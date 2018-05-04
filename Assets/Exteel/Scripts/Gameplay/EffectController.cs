using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour {

	[SerializeField]private ParticleSystem switchWeaponEffectL, switchWeaponEffectR;
	[SerializeField]private ParticleSystem boostingDust, respawnEffect;
	[SerializeField]private GameObject shieldOnHit;
	[SerializeField]private Sounds Sounds;
	[SerializeField]private Animator Animator;
	[SerializeField]private AnimatorVars AnimatorVars;
	[SerializeField]private Transform[] Hands;
	private MechController mctrl;
	private bool isBoostingDustPlaying = false;
	// Use this for initialization
	void Start () {
		initComponents ();
		initTransforms ();
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
			if (!isBoostingDustPlaying) {
				boostingDust.Play ();
				isBoostingDustPlaying = true;
			}
			UpdateBoostingDust ();
		} else {
			if (isBoostingDustPlaying) {
				isBoostingDustPlaying = false;
				boostingDust.Stop ();
			}
		}
	}

	public void UpdateBoostingDust(){//called by horizontal boosting state
		float direction = Animator.GetFloat (AnimatorVars.direction_id), speed = Animator.GetFloat (AnimatorVars.speed_id);
		if ((direction > 0 || direction < 0 || speed > 0 || speed < 0) && Animator.GetBool(AnimatorVars.boost_id)) {
			if(!isBoostingDustPlaying)
				BoostingDustEffect (true);
			boostingDust.transform.localRotation = Quaternion.Euler (-90, Vector3.SignedAngle (Vector3.up, new Vector3 (-direction, speed, 0), Vector3.forward), 90);
		}else{
			if(isBoostingDustPlaying)
				BoostingDustEffect (false);
		}
	}

	public void RespawnEffect(){
		respawnEffect.Play ();
	}

	public void ShieldOnHitEffect(int shield){
		GameObject g = Instantiate (shieldOnHit, Hands [shield].position - Hands[shield].forward*2, Quaternion.identity, Hands [shield]);
		g.transform.rotation = Quaternion.LookRotation (Hands [shield].forward);
		g.GetComponent<ParticleSystem> ().Play ();
		Destroy (g, 2);
	}

	void OnEnable(){
		RespawnEffect ();
	}
}
