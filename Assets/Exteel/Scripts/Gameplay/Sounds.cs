﻿using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	// Sound clip
	public AudioClip[] ShotSounds = new AudioClip[4]; // init in MechCombat 
	[SerializeField] PhotonView pv;

	[SerializeField] AudioClip Lock;
	[SerializeField] AudioClip OnLocked;
	[SerializeField] AudioClip[] Slash;
	[SerializeField] AudioClip BoostStart;
	[SerializeField] AudioClip BoostLoop;

	[SerializeField] AudioClip[] RPCsounds;
	/*
	0 : Switch weapons
	1 : Slash On Hit

	*/
	int weaponOffset =0; // update by switchWeapon

	[SerializeField]
	private AudioSource Source;

	[SerializeField]
	private AudioSource MovementSource;

	// Use this for initialization
	void Start () {
		if (MovementSource != null) {
			MovementSource.clip = BoostLoop;
			MovementSource.volume = 0.3f;
		}

		if(Source!=null)
			Source.volume = 0.1f;
	}

	public void UpdateSounds(int Offset){
		weaponOffset = Offset;
	}
	
	public void PlayShotL() {  // RCL is also using this
		if(ShotSounds[weaponOffset]!=null)
			Source.PlayOneShot(ShotSounds[weaponOffset]);
	}
	public void PlayShotR() {
		if(ShotSounds[weaponOffset+1]!=null)
			Source.PlayOneShot(ShotSounds[weaponOffset+1]);
	}

	public void PlaySlash(int num){
		Source.PlayOneShot (Slash [num]);
	}

	public void PlayLock(){
		Source.PlayOneShot (Lock);
	}
	public void PlayBoostStart(){
		Source.PlayOneShot(BoostStart);
	}
	public void PlayBoostLoop(){
		MovementSource.Play ();
	}
	public void StopBoostLoop(){
		MovementSource.Stop ();
	}
	public void PlayOnLocked(){
		Source.PlayOneShot (OnLocked);
	}
	public void PlaySlashOnHit(){
		pv.RPC("RPCPlaySound", PhotonTargets.All, 1);
	}
	public void PlaySwitchWeapon(){
		pv.RPC("RPCPlaySound", PhotonTargets.All, 0);
	}
	[PunRPC]
	private void RPCPlaySound(int num){
		Source.PlayOneShot (RPCsounds[num]);
	} 
}