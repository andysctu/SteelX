using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BoosterController : MonoBehaviour {
    [SerializeField] private AudioClip BoostOpen = null, BoostLoop = null, BoostClose = null;
    [SerializeField] private ParticleSystem[] PSs;
    [SerializeField] private Animator animator;

    private AudioSource audioSource;

    private void Start() {
        InitAudioSource();
    }

    private void InitAudioSource() {
        audioSource = GetComponent<AudioSource>();

        audioSource.volume = 1;
        audioSource.dopplerLevel = 0;
        audioSource.spatialBlend = 1;
        audioSource.clip = BoostLoop;
        audioSource.playOnAwake = false;
        audioSource.minDistance = 50;
        audioSource.maxDistance = 350;
    }

    public void StartBoost() {
        if(audioSource == null)//this gameObejct is destroyed
            return;

        audioSource.PlayOneShot(BoostOpen);
        if (BoostLoop != null) audioSource.Play();
        animator.SetTrigger("open");

        foreach (ParticleSystem ps in PSs) {
            ps.Play();
        }
    }

    public void StopBoost() {
        if (audioSource == null) {//this gameObejct is destroyed
            Debug.LogWarning("StopBoost gets called when boosterController is destroyed.");//TODO : debug take out
            return;
        }
        if (BoostLoop != null) audioSource.Stop();
        animator.SetTrigger("close");
        foreach (ParticleSystem ps in PSs) {
            ps.Stop();
        }
        if (BoostClose != null)
            audioSource.PlayOneShot(BoostClose, 0.5f);
    }

    private void OnDestroy() {
        audioSource = null;
    }
}