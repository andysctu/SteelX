using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	// Sound clip
	public AudioClip[] ShotSounds = new AudioClip[4]; // init in MechCombat 
	[SerializeField] AudioClip Lock;
	[SerializeField] AudioClip OnLocked;
	[SerializeField] AudioClip Slash1;
	[SerializeField] AudioClip Slash2;
	[SerializeField] AudioClip Slash3;
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
		Source.PlayOneShot(ShotSounds[weaponOffset]);
	}
	public void PlayShotR() {
		Source.PlayOneShot(ShotSounds[weaponOffset+1]);
	}

	public void PlaySlash1(){
		Source.PlayOneShot(Slash1);
	}
	public void PlaySlash2(){
		Source.PlayOneShot(Slash2);
	}
	public void PlaySlash3(){
		Source.PlayOneShot(Slash3);
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
