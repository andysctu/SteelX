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
        } else {
            //base.Awake();
            //StartScene();
        }
    }

    private void StartTestScene() {
        CTFManager CTFManager = gameObject.AddComponent<CTFManager>();
        CTFManager.Offline = true;
        Debug.Log("Add CTFManager");
    }

    public override void StartScene() {
        //base.StartScene();
        switch (PhotonNetwork.room.CustomProperties["GameMode"].ToString()) {
            case "DeathMode":
            Debug.LogError("Not Implemented");
            break;
            case "TeamDeathMode":
                Debug.LogError("Not Implemented");
            break;
            case "CaptureTheFlag":
                Debug.Log("Add CTFManager.");
                gameObject.AddComponent<CTFManager>();
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