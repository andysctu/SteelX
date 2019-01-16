using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BoosterController : MonoBehaviour {
    [SerializeField] private AudioClip BoostOpen = null, BoostLoop = null, BoostClose = null;
    [SerializeField] private ParticleSystem[] PSs;
    [SerializeField] private Animator animator;

    private AudioSource audioSource;
    private bool _isBoostFlameOn;
    private void Start() {
        InitAudioSource();
    }

    private void InitAudioSource() {
        audioSource = GetComponent<AudioSource>();

        audioSource.volume = 0.8f;
        audioSource.dopplerLevel = 0;
        audioSource.spatialBlend = 1;
        audioSource.clip = BoostLoop;
        audioSource.playOnAwake = false;
        audioSource.minDistance = 50;
        audioSource.maxDistance = 350;
    }

    public void StartBoost() {
        if(audioSource == null || _isBoostFlameOn)//this gameObejct is destroyed
            return;

        _isBoostFlameOn = true;

        audioSource.PlayOneShot(BoostOpen);
        if (BoostLoop != null) audioSource.Play();
        animator.SetTrigger("open");

        foreach (ParticleSystem ps in PSs) {
            ps.Play();
        }
    }

    public void StopBoost() {
        if (audioSource == null || !_isBoostFlameOn) {//this gameObject is destroyed
            Debug.LogWarning("StopBoost gets called when boosterController is destroyed.");//TODO : debug take out
            return;
        }

        _isBoostFlameOn = false;

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