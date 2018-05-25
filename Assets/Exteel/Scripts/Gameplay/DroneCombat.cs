using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCombat : Combat {

	public Transform[] Hands;

    [SerializeField] private SkillController SkillController;
    private int default_layer = 0, player_layer = 8;
	private EffectController EffectController;
    private bool onSkill = false;

    private void Awake() {
        if(SkillController!=null)SkillController.OnSkill += OnSkill;
    }
    void Start () {
		currentHP = MAX_HP;
		EffectController = GetComponent<EffectController> ();
		findGameManager();
        EffectController.RespawnEffect();
		gm.RegisterPlayer(photonView.viewID, 0);
	}

	[PunRPC]
	public override void OnHit(int d, int shooter_viewID, string weapon, bool isSlowDown = false) {
        Debug.Log("currenthp : " + currentHP);
		currentHP -= d;

        if (CheckIsSwordByStr(weapon)){
            EffectController.SlashOnHitEffect(false, 0);
        }

		if (currentHP <= 0) {
//			if (shooter == PhotonNetwork.playerName) hud.ShowText(cam, transform.position, "Kill");
			DisableDrone ();
			//gm.RegisterKill(shooter_viewID, photonView.viewID);
		}
	}

	[PunRPC]
	public void ShieldOnHit(int d, int shooter_viewID, int hand, string weapon) {
        Debug.Log("dmg on shield : " + d);
		currentHP -= d;

        if (CheckIsSwordByStr(weapon)){
            EffectController.SlashOnHitEffect(true, hand);
        }else if (CheckIsSpearByStr(weapon)) {
            EffectController.SmashOnHitEffect(true, hand);
        }

        if (currentHP <= 0) {
			//			if (shooter == PhotonNetwork.playerName) hud.ShowText(cam, transform.position, "Kill");
			DisableDrone ();
			gm.RegisterKill(shooter_viewID, photonView.viewID);
		}
	}

	[PunRPC]
	void KnockBack(Vector3 dir, float length){
		transform.position += dir * length;
	}

	void DisableDrone() {
		gameObject.layer = default_layer;
        StartCoroutine(DisableDroneWhenNotOnSkill());
	}

    IEnumerator DisableDroneWhenNotOnSkill() {
        yield return new WaitWhile(() => onSkill);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = false;
        }
        StartCoroutine(RespawnAfterTime(2));
    }


    void EnableDrone() {
        EffectController.RespawnEffect();
        gameObject.layer = player_layer;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = true;
		}
		currentHP = MAX_HP;
	}

    void OnSkill(bool b) {
        onSkill = b;
    }

	IEnumerator RespawnAfterTime(int time){
		yield return new WaitForSeconds (time);
		EnableDrone ();
	}

    bool CheckIsSwordByStr(string name) {
        return name.Contains("SHL");
    }
    bool CheckIsSpearByStr(string name) {
        return name.Contains("ADR");
    }
}
