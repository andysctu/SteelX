using UnityEngine;

public class Sounds : MonoBehaviour {
    [SerializeField] private PhotonView pv;
    [SerializeField] private AudioClip Lock, OnLocked;
    [SerializeField] private AudioClip SwitchWeapon;
    //[SerializeField]AudioClip WalkSound;
    [SerializeField] private AudioSource Source;
    [SerializeField] private AudioSource MovementSource;

    private void Start() {
        SetVolume(0.8f);
    }

    private void SetVolume(float v) {
        if (MovementSource != null)
            MovementSource.volume = v;

        if (Source != null)
            Source.volume = v;
    }

    public void PlayLock() {
        Source.PlayOneShot(Lock);
    }
    public void PlayOnLocked() {
        Source.PlayOneShot(OnLocked);
    }

    /*public void PlayWalk(bool b){
		if(b){
			MovementSource.Play ();
		}else{
			MovementSource.Stop ();
		}
	}*/

    public void PlayClip(AudioClip ac) {
        Source.PlayOneShot(ac);
    }

    public void PlaySwitchWeapon() {
        Source.PlayOneShot(SwitchWeapon);
    }
}