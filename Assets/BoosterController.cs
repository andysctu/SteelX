using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BoosterController : MonoBehaviour {

	[SerializeField]private AudioClip BoostStart,BoostLoop;
	[SerializeField]private ParticleSystem ps;
	private AudioSource AudioSource;

	void Start () {
		Transform neck = transform.root.Find ("CurrentMech/metarig/hips/spine/chest/neck");
		ParticleSystem g = neck.GetComponentInChildren<ParticleSystem> ();
		if(g != null){
			Destroy (g.gameObject);
		}
		ps.transform.SetParent (neck);
		AudioSource = GetComponent<AudioSource> ();	
		AudioSource.clip = BoostLoop;
	}

	public void StartBoost(){
		AudioSource.PlayOneShot (BoostStart);
		AudioSource.Play ();
		ps.Play ();
	}

	public void StopBoost(){
		AudioSource.Stop ();
		ps.Stop();
	}
}
