using UnityEngine;

public class GameSceneManager : IScene {
    [SerializeField]private GameObject EscPanel;    
    private AudioClip gameMusic;
    private MusicManager MusicManager;
    private GameManager gm;
    public const string _sceneName = "Game";

    public bool test = false;//TODO : remove this

    protected override void Awake() {
        if (test) {
            StartTestScene();
        }
    }

    private void StartTestScene() {
        //CTFManager CTFManager = gameObject.AddComponent<CTFManager>();
        //gm = CTFManager as GameManager;
        //CTFManager.Offline = true;
        //Debug.Log("Add CTFManager");

        TestModeManager TestModeManager = gameObject.AddComponent<TestModeManager>();
        gm = TestModeManager as GameManager;
    }

    public override void StartScene() {
        if (MusicManager == null)
            MusicManager = FindObjectOfType<MusicManager>();

        MusicManager.ManageMusic(null);//Shut down game lobby music

        switch (PhotonNetwork.room.CustomProperties["GameMode"].ToString()) {
        case "DeathMatch":
            gm = gameObject.AddComponent<DMManager>();
        break;
        case "TeamDeathMode":
            Debug.LogError("Not Implemented");
        break;
        case "CaptureTheFlag":
            gm = gameObject.AddComponent<CTFManager>();
            gameMusic = Resources.Load<AudioClip>("GFM/Game_Music/CTFsoundtrack");
            gm.RegisterTimerEvent(180, () => MusicManager.ManageMusic(gameMusic));//180 : music length
        break;
        default:
            Debug.LogError("No such mode : "+ PhotonNetwork.room.CustomProperties["GameMode"].ToString());
        break;
        }
    }

    public override void EndScene() {
        base.EndScene();
        MusicManager.ManageMusic(null);
    }

    public override string GetSceneName() {
        return _sceneName;
    }
}