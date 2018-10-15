using UnityEngine;

public class CheckIsRendered : MonoBehaviour {
    private SkinnedMeshRenderer theMeshRenderer;
    private CrosshairController crosshairController;//same player's crosshair for all mechs

    private bool isVisible = false;
    private float lastRequestTime;
    private float requestDeltaTime = 0.5f;

    private void Start() {
        if (GetComponent<PhotonView>().isMine && gameObject.tag != "Drone") {//not get msg from myself
            enabled = false;
        }
        //find core
        if (gameObject.tag != "Drone") {
            SkinnedMeshRenderer[] meshRenderers = transform.Find("CurrentMech").GetComponentsInChildren<SkinnedMeshRenderer>();
            theMeshRenderer = meshRenderers[0];
        } else {
            SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            theMeshRenderer = meshRenderer;
        }
    }

    public void SetCrosshair(CrosshairController crosshairController) {
        this.crosshairController = crosshairController;
    }

    // Update is called once per frame
    private void Update() {
        if (crosshairController == null) {//TODO : improve this
            if (Time.time - lastRequestTime >= requestDeltaTime) {
                lastRequestTime = Time.time;
                Camera[] cameras = (Camera[])GameObject.FindObjectsOfType<Camera>();
                foreach (Camera cam in cameras) {
                    if (cam.transform.root.name == "PlayerCam") {
                        crosshairController = cam.GetComponent<CrosshairController>();
                    }
                }
            }
        } else {
            if(theMeshRenderer == null) {//TODO : remake this
                SkinnedMeshRenderer[] meshRenderers = transform.Find("CurrentMech").GetComponentsInChildren<SkinnedMeshRenderer>();
                theMeshRenderer = meshRenderers[0];
            }

            if (theMeshRenderer.isVisible) {
                if (!isVisible) {
                    isVisible = true;
                    crosshairController.Targets.Add(transform.root.gameObject);
                }
            } else {
                if (isVisible) {
                    isVisible = false;
                    crosshairController.Targets.Remove(transform.root.gameObject);
                }
            }
        }
    }
}