using UnityEngine;

public class GameSceneManager : IScene {
    [SerializeField]private GameObject EscPanel;
    private MusicManager MusicManager;
    private AudioClip gameMusic;

    public const string _sceneName = "Game";

    public bool test = false;//TODO : remove this

    protected override void Awake() {
        if (test) {
            StartTestScene();
        }
    }

    private void StartTestScene() {
        CTFManager CTFManager = gameObject.AddComponent<CTFManager>();
        CTFManager.Offline = true;
        Debug.Log("Add CTFManager");
    }

    public override void StartScene() {
        switch (PhotonNetwork.room.CustomProperties["GameMode"].ToString()) {
        case "DeathMatch":
            gameObject.AddComponent<DMManager>();
        break;
        case "TeamDeathMode":
            Debug.LogError("Not Implemented");
        break;
        case "CaptureTheFlag":
            gameObject.AddComponent<CTFManager>();
        break;
        default:
            Debug.LogError("No such mode : "+ PhotonNetwork.room.CustomProperties["GameMode"].ToString());
        break;
        }

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