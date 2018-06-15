using UnityEngine;

public class BoosterController : MonoBehaviour {

	[SerializeField]private AudioClip BoostOpen,BoostLoop, BoostClose;
	[SerializeField]private ParticleSystem[] PSs;
    [SerializeField]private Animator animator;

    private AudioSource audioSource;
    private float volume = 1f;

	void Start () {
        audioSource = GetComponent<AudioSource>();
        //audioSource.volume = volume;
        audioSource.clip = BoostLoop;
	}

	public void StartBoost(){
        audioSource.PlayOneShot(BoostOpen);
		if(BoostLoop!=null)audioSource.Play ();
        animator.SetTrigger("open");

        foreach(ParticleSystem ps in PSs) {
            ps.Play();
        }
	}

	public void StopBoost(){
        if (BoostLoop != null) audioSource.Stop ();
        animator.SetTrigger("close");
        foreach (ParticleSystem ps in PSs) {
            ps.Stop();
        }
        if(BoostClose != null)
            audioSource.PlayOneShot(BoostClose,0.5f);
    }
}
