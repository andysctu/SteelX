using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	// Sound clip
	[SerializeField] AudioClip Shot;
	private AudioSource Source;

	// Use this for initialization
	void Start () {
		Source = GetComponent<AudioSource>();
	}
	
	public void PlayShot() {
		Source.PlayOneShot(Shot);
	}
}
