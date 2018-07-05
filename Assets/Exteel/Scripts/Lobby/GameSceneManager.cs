using UnityEngine;

public class GameSceneManager : IScene {
    MusicManager MusicManager;
    AudioClip gameMusic;
    public const string _sceneName = "Game";

    public override void StartScene() {
        base.StartScene();

        if (MusicManager == null)
            MusicManager = FindObjectOfType<MusicManager>();
        MusicManager.ManageMusic(gameMusic);
    }

    public override void EndScene() {
        base.EndScene();
        MusicManager.ManageMusic(null);
    }

    public override string GetSceneName() {
        return _sceneName;
    }
}