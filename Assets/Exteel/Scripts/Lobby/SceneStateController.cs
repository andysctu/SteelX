using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneStateController : MonoBehaviour {
    private Dictionary<string, IScene> _Scenes;
    public static string ActiveScene = "";
    private IScene curActiveScene = null;

    private void Awake() {
        if (FindObjectsOfType<SceneStateController>().Length > 1) {
            Destroy(gameObject);
            return;
        }

        SetDefaultScene();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SetDefaultScene() {
        if (ActiveScene == "") {//this is for not start for login
            if ((ActiveScene = FindActiveScene()) == "") {
                Debug.LogError("AcitveScene empty and can't find active scene");
                return;
            }
        }   
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        RegisterScenes();
        LoadScene(ActiveScene);
    }

    //Find all IScene and register into dictionary
    private void RegisterScenes() {        
        _Scenes = new Dictionary<string, IScene>();
        IScene[] scenes = (IScene[])Resources.FindObjectsOfTypeAll(typeof(IScene));
        foreach (IScene s in scenes) {
            _Scenes.Add(s.GetSceneName(), s);
        }
    }

    private void SetCurrentScene(string sceneName) {
        if (_Scenes == null || !_Scenes.ContainsKey(sceneName)) {
            Debug.LogWarning("Can't find scene : " + sceneName);
            return;
        }

        ActiveScene = sceneName;
        curActiveScene = _Scenes[sceneName];
    }

    public static void SetSceneToLoadOnLoaded(string sceneName) {//TODO : consider remove this
        ActiveScene = sceneName;
    }

    public void LoadScene(string sceneName) {
        if (curActiveScene != null)curActiveScene.EndScene();

        SetCurrentScene(sceneName);

        curActiveScene.StartScene();
    }

    private string FindActiveScene() {
        IScene[] scenes = FindObjectsOfType<IScene>();
        foreach (IScene s in scenes) {
            if (s.gameObject.activeSelf) {
                return s.GetSceneName();
            }
        }
        return "";
    }

    public override string ToString() {
        return "Current scene : " + curActiveScene.GetSceneName();
    }
}