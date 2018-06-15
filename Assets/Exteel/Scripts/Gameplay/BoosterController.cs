using UnityEngine;

public class BoosterController : MonoBehaviour {

	[SerializeField]private AudioClip BoostStart,BoostLoop;
	[SerializeField]private ParticleSystem ps;
    private AudioSource audioSource;
    private float volume = 0.3f;

	void Start () {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = BoostLoop;
	}

	public void StartBoost(){
        audioSource.PlayOneShot(BoostStart);
		audioSource.Play ();
		ps.Play ();
	}

	public void StopBoost(){
		audioSource.Stop ();
		ps.Stop();
	}
}
