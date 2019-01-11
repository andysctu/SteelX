using UnityEngine;
using System.Collections;

public class MapElement : MonoBehaviour {
    [SerializeField]protected GameObject ObjectToAttachOnMapCanvas;
    protected MapPanelController MapPanelController;
    protected Camera MapCamera;
    protected GameObject ThePlayer;
    protected GameManager gm;

    protected virtual void Start() {        
        gm = FindObjectOfType<GameManager>();
        GameObject Map = gm.GetMap();

        MapPanelController = Map.GetComponentInChildren<MapPanelController>();
        MapCamera = MapPanelController.GetComponentInChildren<Camera>();

        Canvas MapCanvas = Map.GetComponentInChildren<Canvas>();
        ObjectToAttachOnMapCanvas.name = transform.root.name;
        ObjectToAttachOnMapCanvas.transform.SetParent(MapCanvas.transform);
        ObjectToAttachOnMapCanvas.transform.localRotation = Quaternion.identity;        
        ApplyScale();

        StartCoroutine(GetThePlayer(gm));
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        int request_times = 0;
        while ((ThePlayer = gm.GetThePlayerMech()) == null && request_times < 15) {
            request_times++;
            yield return new WaitForSeconds(0.5f);
        }

        if (request_times >= 15) {
            Debug.LogError("Can't get the player");
            yield break;
        } else {
            OnGetPlayerAction();
        }
        yield break;
    }

    protected virtual void OnGetPlayerAction() {
    }

    protected virtual void ApplyScale() {
        ObjectToAttachOnMapCanvas.transform.localScale *= MapPanelController.scale;
    }

    protected virtual void LateUpdate() {
        ObjectToAttachOnMapCanvas.transform.rotation = Quaternion.Euler(90, MapCamera.transform.rotation.eulerAngles.y, 0);//TODO : Check if this necessary
        ObjectToAttachOnMapCanvas.transform.position = transform.position + Vector3.up * 400;
    }

    public void ShowElementOnMap(bool b) {
        ObjectToAttachOnMapCanvas.SetActive(b);
    }

    private void OnDestroy() {
        Destroy(ObjectToAttachOnMapCanvas);
    }
}