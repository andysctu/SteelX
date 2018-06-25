using UnityEngine;

public class MySceneManager : MonoBehaviour {
#if UNITY_EDITOR
    [NamedArrayAttribute(new string[] { "Login", "Lobby", "Hangar", "GameLobby", "Store" })]
#endif
    [SerializeField] private GameObject[] Scenes;
    [SerializeField] private GameObject OperatorPanel;
    public static class SceneName { public const int Login = 0, Lobby = 1, Hangar = 2, GameLobby = 3, Store = 4; };
    public static int ActiveScene { get;private set;}
    private GameObject curActiveScene;
    public delegate void LoadSceneAction(int scene);
    public event LoadSceneAction OnSceneLoaded;

    private void Awake() {
        ActiveScene = FindActiveScene();
        curActiveScene = Scenes[FindActiveScene()];

        LoadScene(ActiveScene);
    }

    private int FindActiveScene() {
        for (int i = 0; i < Scenes.Length; i++) {
            if (Scenes[i].activeSelf) {
                return i;
            }
        }

        Debug.LogError("No scene are active.");
        LoadScene(SceneName.Lobby);
        return SceneName.Lobby;
    }

    private void SetCurrentScene(int scene) {
        ActiveScene = scene;
        curActiveScene = Scenes[scene];
    }

    public void LoadScene(int scene) {
        //Disable current scene
        curActiveScene.SetActive(false);

        OperatorPanel.SetActive(scene == SceneName.Lobby || scene == SceneName.Hangar || scene == SceneName.Store);

        //Enable new scene
        Scenes[scene].SetActive(true);

        //Update current scene
        SetCurrentScene(scene);
        
        if(OnSceneLoaded != null)
            OnSceneLoaded(scene);
    }

    public void GoToHangar() {
        LoadScene(SceneName.Hangar);
    }

    public void GoToStore() {
        LoadScene(SceneName.Store);
    }

    public void GoToLobby() {
        LoadScene(SceneName.Lobby);
    }

    public void GoToGameLobby() {
        LoadScene(SceneName.GameLobby);
    }


    void OnConnectedToMaster() {
        print("Connected to Server successfully.");
    }
}