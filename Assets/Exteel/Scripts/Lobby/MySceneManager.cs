using UnityEngine;

public class MySceneManager : MonoBehaviour {
#if UNITY_EDITOR
    [NamedArrayAttribute(new string[] { "Login", "Lobby", "Hangar", "GameLobby", "Store" })]
#endif
    [SerializeField] private GameObject[] Scenes;
    [SerializeField] private GameObject OperatorPanel;
    public static class SceneName { public const int Login = 0, Lobby = 1, Hangar = 2, GameLobby = 3, Store = 4;};
    public static int ActiveScene;
    private GameObject curActiveScene;
    public delegate void LoadSceneAction(int scene);
    public event LoadSceneAction OnSceneLoaded;

    private void Awake() {
        curActiveScene = Scenes[ActiveScene];
        LoadScene(ActiveScene);
    }

    private void SetCurrentScene(int scene) {
        ActiveScene = scene;
        curActiveScene = Scenes[scene];
    }

    public void LoadScene(int scene) {
        //Disable current scene & Enable new scene
        for (int i=0;i<Scenes.Length;i++)
            if(Scenes[i]!=null)Scenes[i].SetActive(i == scene);

        OperatorPanel.SetActive(scene == SceneName.Lobby || scene == SceneName.Hangar || scene == SceneName.Store);

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