using UnityEngine;
using System.Collections;

public class DisplayInfo : MonoBehaviour{
    [SerializeField] private GameObject ObjectToParentOnCanvas;
    private Transform ObjectTransform;//for efficiency
    private Camera cam;
    private Camera[] thePlayerCameras;
    private Transform targetPlayer_transform;
    private int curIndex = 0;    
    private float maxDistance = 500;
    private bool isDisplaying = true;
    private int height = 10;

    protected virtual void Awake() {
        ObjectTransform = ObjectToParentOnCanvas.transform;
        ObjectToParentOnCanvas.SetActive(false);
    }

    protected virtual void Start() {
        GameManager gm = FindObjectOfType<GameManager>();
        StartCoroutine(GetThePlayer(gm));

        //Parent the object to canvas
        ParentInfoToPanelCanvas();
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        int request_times = 0;
        GameObject ThePlayer;
        while ((ThePlayer = (GameObject)PhotonNetwork.player.TagObject) == null && request_times < 15) {
            request_times++;
            yield return new WaitForSeconds(0.5f);
        }

        if (request_times >= 15) {
            Debug.LogError("Can't get the player");
            yield break;
        }

        thePlayerCameras = gm.GetThePlayerMainCameras();
        InitPlayerRelatedComponents(ThePlayer);
        ObjectToParentOnCanvas.SetActive(true);
        yield break;
    }

    protected virtual void InitPlayerRelatedComponents(GameObject ThePlayer) {
        targetPlayer_transform = ThePlayer.transform;

        //get the player camera when the player is instantiated
        cam = thePlayerCameras[curIndex];
        maxDistance = cam.farClipPlane;        
    }

    private void ParentInfoToPanelCanvas() {
        Transform InfoCanvas = GameObject.Find("PanelCanvas/Infos").transform;
        ObjectToParentOnCanvas.transform.SetParent(InfoCanvas);
        ObjectToParentOnCanvas.transform.localRotation = Quaternion.identity;        
    }

    public void SetHeight(int height) {
        this.height = height;
    }

    public void SetName(string name) {
        ObjectTransform.name = name;
    }

    public void EnableDisplay(bool b) {
        ObjectToParentOnCanvas.SetActive(b);
        enabled = b;
        isDisplaying = b;
    }

    private void LateUpdate() {
        if (cam != null && targetPlayer_transform != null) {//targetPlayer is null when get destroyed
            ObjectToParentOnCanvas.transform.position = cam.WorldToScreenPoint(transform.position + new Vector3(0, height, 0));

            if (isDisplaying && (ObjectTransform.position.z < 0 || Vector3.Distance(transform.position, targetPlayer_transform.position) > maxDistance)) {
                isDisplaying = false;
                ObjectToParentOnCanvas.SetActive(false);
            }
            if (!isDisplaying && ObjectTransform.position.z >= 0 && Vector3.Distance(transform.position, targetPlayer_transform.position) <= maxDistance) {
                isDisplaying = true;
                ObjectToParentOnCanvas.SetActive(true);
            }

            if (!cam.enabled || !cam.gameObject.activeSelf) {
                curIndex = (curIndex + 1) % thePlayerCameras.Length;
                cam = thePlayerCameras[curIndex];
            }
        }
    }

    private void OnDestroy() {
        Destroy(ObjectToParentOnCanvas);
    }
}