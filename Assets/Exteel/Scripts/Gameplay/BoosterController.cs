using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BoosterController : MonoBehaviour {

	[SerializeField]private AudioClip BoostStart,BoostLoop;
	[SerializeField]private ParticleSystem ps;
	private AudioSource audioSource;
    private float volume = 0.3f;

	void Start () {
		Transform neck = transform.root.Find ("CurrentMech/metarig/hips/spine/chest/neck");
		ParticleSystem g = neck.GetComponentInChildren<ParticleSystem> ();
		if(g != null){
			Destroy (g.gameObject);
		}
		ps.transform.SetParent (neck);
		audioSource = GetComponent<AudioSource> ();	
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
