using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayMusic : MonoBehaviour {

	static bool isLobbyMusicExist = false;
	AudioSource audiosource;
	// Use this for initialization
	void Start () {
		if (isLobbyMusicExist == true) {
			Destroy (gameObject);
		} else {

			SceneManager.sceneLoaded += Musicmanage;

			DontDestroyOnLoad (this);
			audiosource = GetComponent<AudioSource> ();
			audiosource.Play ();
			isLobbyMusicExist = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void FixedUpdate(){
		if(isLobbyMusicExist==false){
			Destroy (gameObject);
		}
	}

	void Musicmanage(Scene scene, LoadSceneMode mode){
		if(scene.name!="Lobby"&&scene.name!="Hangar"&&scene.name!="Store"&&scene.name!="GameLobby"){
			isLobbyMusicExist = false;
		}
	}
}
