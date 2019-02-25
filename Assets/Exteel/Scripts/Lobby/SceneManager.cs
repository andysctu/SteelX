using UnityEngine;

public abstract class SceneManager : MonoBehaviour{
    public GameObject Scene;
    protected SceneStateController SceneStateController;
    public abstract string GetSceneName();

    protected virtual void Awake() {//For not start from login
        if (SceneStateController.ActiveScene != "" && SceneStateController.ActiveScene != GetSceneName())
            EnableScene(false);
    }

    public virtual void StartScene() {
        EnableScene(true);
        SceneStateController = FindObjectOfType<SceneStateController>();
    }

    public virtual void EndScene() {
        EnableScene(false);
        gameObject.SetActive(false);
    }

    public override string ToString() {
        return GetSceneName();
    }

    public virtual void EnableScene(bool b){
        Scene.SetActive(b);
    }

    public virtual bool IsActive(){
        return Scene.activeSelf;
    }
}
