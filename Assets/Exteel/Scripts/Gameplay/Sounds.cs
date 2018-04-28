using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	// Sound clip
	[HideInInspector]public AudioClip[] ShotSounds = new AudioClip[4]; // init in MechCombat 
	[HideInInspector]public AudioClip[] SlashL = new AudioClip[6];
	[HideInInspector]public AudioClip[] SlashR = new AudioClip[6];
	[HideInInspector]public AudioClip[] SlashOnHit = new AudioClip[4];
	//[HideInInspector]public AudioClip[] SmashOnHit = new AudioClip[4];  //no files

	[SerializeField]PhotonView pv;
	[SerializeField]AudioClip Lock;
	[SerializeField]AudioClip OnLocked;
	[SerializeField]AudioClip Smash;
	[SerializeField]AudioClip SwitchWeapon;
	[SerializeField]AudioClip BCNload,BCNPose;
	//[SerializeField]AudioClip WalkSound;

	[SerializeField]private AudioSource Source;
	[SerializeField]private AudioSource MovementSource;

	int weaponOffset =0; // update by switchWeapon

	// Use this for initialization
	void Start () {

		MovementSource.volume = 0.3f;
		//MovementSource.clip = WalkSound;

		if(Source!=null)
			Source.volume = 0.3f;
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

	public void PlaySlashL(int num){
		Source.PlayOneShot (SlashL[num + weaponOffset/2*3]);
	}
	public void PlaySlashR(int num){
		Source.PlayOneShot (SlashR[num + weaponOffset/2*3]);
	}

	public void PlayLock(){
		Source.PlayOneShot (Lock);
	}
	public void PlayOnLocked(){
		Source.PlayOneShot (OnLocked);
	}

	public void PlayBCNPose(){
		Source.PlayOneShot (BCNPose);
	}

	public void PlayBCNload(){
		Source.PlayOneShot (BCNload);
	}

	public void PlaySlashOnHit(int num){
		pv.RPC("RPCPlaySlashOnHit", PhotonTargets.All, num);
	}

	/*public void PlayWalk(bool b){
		if(b){
			MovementSource.Play ();
		}else{
			MovementSource.Stop ();
		}
	}*/

	public void PlaySwitchWeapon(){
		Source.PlayOneShot (SwitchWeapon);
	}

	[PunRPC]
	void RPCPlaySlashOnHit(int num){
		if(SlashOnHit[num]!=null)
			Source.PlayOneShot (SlashOnHit[num]);
	}

	/*[PunRPC]
	void RPCPlaySmashOnHit(int num){
		Source.PlayOneShot (SmashOnHit[num]);
	}*/
}
