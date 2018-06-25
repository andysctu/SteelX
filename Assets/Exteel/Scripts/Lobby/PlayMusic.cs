using UnityEngine;

public class PlayMusic : MonoBehaviour {
    private MySceneManager MySceneManager;

    static bool isLobbyMusicExist = false;
	AudioSource audiosource;

    private void Awake() {
        if(isLobbyMusicExist)Destroy(this);

        MySceneManager = FindObjectOfType<MySceneManager>();
        if(MySceneManager==null)return;
        MySceneManager.OnSceneLoaded += ManageMusic;
        audiosource = GetComponent<AudioSource>();
    }

    void ManageMusic(int scene){
        if(MySceneManager.ActiveScene == MySceneManager.SceneName.Login)
            return;

        if(!audiosource.isPlaying)            
            audiosource.Play();
        if (scene != MySceneManager.SceneName.Lobby && scene != MySceneManager.SceneName.Hangar && scene != MySceneManager.SceneName.Store && scene != MySceneManager.SceneName.GameLobby) {
            //Destroy music in game
            isLobbyMusicExist = false;
            Destroy(this);
        } else {            
            //if (!isLobbyMusicExist) {
            //    isLobbyMusicExist = true;
            //    audiosource = GetComponent<AudioSource>();
            //    audiosource.Play();
            //} else {
            //    //already has one playing
            //    Destroy(this);
            //}
        }
	}
}
