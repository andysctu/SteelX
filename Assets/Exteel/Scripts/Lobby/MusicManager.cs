using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour {
	private AudioSource audiosource;
    private AudioClip curAudioClip = null;
    private float volume = 0.4f;    
    static bool isLobbyMusicExist = false;

    private void Awake() {
        if(isLobbyMusicExist)
            Destroy(gameObject);
        audiosource = GetComponent<AudioSource>();
        isLobbyMusicExist = true;
    }

    public void SetVolume(float volume) {
        this.volume = volume;
    }

    public void ManageMusic(AudioClip clip){
        if(clip == curAudioClip && audiosource.isPlaying)return;

        curAudioClip = clip;
        audiosource.clip = curAudioClip;

        if (clip == null) {
            audiosource.Stop();
            return;
        }

        audiosource.Play();
    }
}
