using UnityEngine;

public abstract class IScene : MonoBehaviour{
    protected SceneStateController SceneStateController;
    public abstract string GetSceneName();

    protected virtual void Awake() {//For not start from login
        if (SceneStateController.ActiveScene != "" && SceneStateController.ActiveScene != GetSceneName())
            gameObject.SetActive(false);
    }

    public virtual void StartScene() {
        gameObject.SetActive(true);
        SceneStateController = FindObjectOfType<SceneStateController>();
    }

    public virtual void EndScene() {
        gameObject.SetActive(false);
    }
}
