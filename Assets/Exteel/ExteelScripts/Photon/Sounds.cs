using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	// Sound clip
	public AudioClip[] ShotSounds = new AudioClip[4]; // init in MechCombat 
	[SerializeField] AudioClip Lock;
	[SerializeField] AudioClip OnLocked;
	[SerializeField] AudioClip[] Slash;
	[SerializeField] AudioClip SlashOnHit;
	[SerializeField] AudioClip BoostStart;
	[SerializeField] AudioClip BoostLoop;
	[SerializeField] AudioClip SwitchWeapon;
	int weaponOffset =0; // update by switchWeapon
	private AudioSource Source;

	// Use this for initialization
	void Start () {
		Source = GetComponent<AudioSource>();
		Source.volume = 0.1f;
	}

	public void UpdateSounds(int Offset){
		weaponOffset = Offset;
	}
	
	public void PlayShotL() {  // RCL is fine
		if(ShotSounds[weaponOffset]!=null)
			Source.PlayOneShot(ShotSounds[weaponOffset]);
	}
	public void PlayShotR() {
		if(ShotSounds[weaponOffset+1]!=null)
			Source.PlayOneShot(ShotSounds[weaponOffset+1]);
	}

	public void PlaySlashOnHit(){
		Source.PlayOneShot (SlashOnHit);
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
		Source.PlayOneShot(BoostLoop);
	}
	public void PlaySwitchWeapon(){
		Source.PlayOneShot(SwitchWeapon);
	}
}
