using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInZone))]
public class HealthPool : Photon.MonoBehaviour {

	[SerializeField]private int healAmount = 250;
	[SerializeField]private float healDeltaTime = 2;
	public GameObject player;
	PlayerInZone PlayerInZone;
	SyncHealthPoolBar syncHealthPoolBar;
	Camera cam;
	MechCombat mechCombat;
	private float LastCheckTime;

	public void Init(){
		cam = player.GetComponentInChildren<Camera> ();
		mechCombat = player.GetComponent<MechCombat> ();
		PlayerInZone = GetComponent<PlayerInZone> ();
		syncHealthPoolBar = GetComponent<SyncHealthPoolBar> ();
		PlayerInZone.SetPlayerID(player.GetPhotonView ().viewID);
	}

	void Update () {
		if (cam != null) {
			transform.LookAt (new Vector3 (cam.transform.position.x, transform.position.y, cam.transform.position.z));
		}
	}

	void FixedUpdate(){
		if(PlayerInZone.IsThePlayerInside()){
			if(Time.time - LastCheckTime >= healDeltaTime){
				if(!mechCombat.IsHpFull() && syncHealthPoolBar.isAvailable){
					if(mechCombat.GetMaxHp() - mechCombat.CurrentHP() >= healAmount){
						LastCheckTime = Time.time;
						mechCombat.photonView.RPC ("OnHeal", PhotonTargets.All, 0, healAmount);
					}else{
						LastCheckTime = Time.time;
						mechCombat.photonView.RPC ("OnHeal", PhotonTargets.All, 0, (mechCombat.GetMaxHp() - mechCombat.CurrentHP()));
					}
				}else{
					LastCheckTime = Time.time;
				}
			}
		}else{
			LastCheckTime = Time.time;
		}
	}
}
