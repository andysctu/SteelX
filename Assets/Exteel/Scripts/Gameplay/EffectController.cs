using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour {

    [SerializeField]SkillController SkillController;
    [SerializeField]private ParticleSystem switchWeaponEffectL, switchWeaponEffectR;
	[SerializeField]private ParticleSystem boostingDust, respawnEffect, damageless;
	[SerializeField]private GameObject shieldOnHit, slashOnHitEffect, smashOnHitEffect;
	[SerializeField]private Sounds Sounds;
	[SerializeField]private Animator Animator;
	[SerializeField]private AnimatorVars AnimatorVars;

    private Transform[] Hands;
    private MechCombat mcbt;
	private MechController mctrl;
	private bool isBoostingDustPlaying = false;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0);

    private void Awake() {
        RegisterOnSkill();    
    }

    private void RegisterOnSkill() {
        if(SkillController!=null)SkillController.OnSkill += PlayOnSkillEffect;
    }

    void OnEnable() {
        RespawnEffect();
    }

    void Start () {
		initComponents ();
		initTransforms ();
        ImplementDamageless();

    }

    void ImplementDamageless() {
        Transform hips = transform.root.Find("CurrentMech/metarig/hips");
        if(hips == null) {
            Debug.LogError("can't find hips");
            return;
        }

        damageless.transform.SetParent(hips);
        damageless.transform.localPosition = Vector3.zero;
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

    //TODO : improve this
	public void UpdateBoostingDust(){//called by horizontal boosting state
		float direction = Animator.GetFloat ("Direction"), speed = Animator.GetFloat ("Speed");//other player also call this

        if ((direction > 0 || direction < 0 || speed > 0 || speed < 0) && Animator.GetBool("Boost")) {
			if(!isBoostingDustPlaying)
				BoostingDustEffect (true);
			boostingDust.transform.localRotation = Quaternion.Euler (-90, Vector3.SignedAngle (Vector3.up, new Vector3 (-direction, speed, 0), Vector3.forward), 90);
		}else{
			if(isBoostingDustPlaying)
				BoostingDustEffect (false);
		}
	}

	public void RespawnEffect(){
        StartCoroutine(PlayRespawnEffect());		
	}

    IEnumerator PlayRespawnEffect() {
        respawnEffect.Play();
        yield return new WaitForSeconds(2);
        respawnEffect.Clear();
        respawnEffect.Stop();
    }

    public void SlashOnHitEffect(bool isShield, int hand) {
        if (isShield) {
            GameObject g = Instantiate(shieldOnHit, Hands[hand].position - Hands[hand].transform.forward * 2, Quaternion.identity, Hands[hand]);
            g.GetComponent<ParticleSystem>().Play();
        } else {
            GameObject g = Instantiate(slashOnHitEffect, transform.position + MECH_MID_POINT, Quaternion.identity, transform);
            g.GetComponent<ParticleSystem>().Play();
        }
    }

    public void SmashOnHitEffect(bool isShield, int hand) {
        if (isShield) {
            GameObject g = Instantiate(shieldOnHit, Hands[hand].position - Hands[hand].transform.forward * 2, Quaternion.identity, Hands[hand]);
            g.GetComponent<ParticleSystem>().Play();
        } else {
            GameObject g = Instantiate(smashOnHitEffect, transform.position + MECH_MID_POINT, Quaternion.identity, transform);
            g.GetComponent<ParticleSystem>().Play();
        }
    }

    private void PlayOnSkillEffect(bool b) {
        if(b)
            damageless.Play();
        else
            damageless.Stop();
    }
}
