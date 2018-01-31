using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	// Sound clip
	[SerializeField] AudioClip Shot;
	[SerializeField] AudioClip Lock;
	[SerializeField] AudioClip OnLocked;
	[SerializeField] AudioClip Slash1;
	[SerializeField] AudioClip Slash2;
	[SerializeField] AudioClip Slash3;
	[SerializeField] AudioClip RCLShoot;
	[SerializeField] AudioClip BoostStart;
	[SerializeField] AudioClip BoostLoop;
	private AudioSource Source;

	// Use this for initialization
	void Start () {
		Source = GetComponent<AudioSource>();
		Source.volume = 0.1f;
	}
	
	public void PlayShot() {
		Source.PlayOneShot(Shot);
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
	public void PlayRCLShoot(){
		Source.PlayOneShot (RCLShoot);
	}
	public void PlayBoostStart(){
		Source.PlayOneShot(BoostStart);
	}
	public void PlayBoostLoop(){
		Source.PlayOneShot(BoostLoop);
	}
}
