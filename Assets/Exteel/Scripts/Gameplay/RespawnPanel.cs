using UnityEngine;
using UnityEngine.UI;

public class RespawnPanel : MonoBehaviour {
    [SerializeField] private Transform MechRespawnButtons, Map_transform;
    private MapPanelController MapPanelController;
    private GameManager gm;
    private RawImage MapRawImage;

    private void Start() {
        InitComponents();

        InitRespawnButtons();

        //Map panel
        AssignMapRenderTexture();

        RequireButtonsPanel();
    }

    private void InitComponents() {
        gm = FindObjectOfType<GameManager>();

        GameObject Map = gm.GetMap();
        Camera MapCamera = Map.GetComponentInChildren<Camera>();

        MapRawImage = GetComponentInChildren<RawImage>();
        MapPanelController = Map.GetComponentInChildren<MapPanelController>();
        MapRawImage.texture = MapCamera.targetTexture;
    }

    private void InitRespawnButtons() {
        Button[] RespawnButtons = MechRespawnButtons.GetComponentsInChildren<Button>();
        for(int i = 0; i < RespawnButtons.Length; i++) {
            int respawnIndex = i;
            RespawnButtons[i].onClick.AddListener(() => gm.CallRespawn(respawnIndex));
        }
    }

    private void AssignMapRenderTexture() {
        MapRawImage.rectTransform.sizeDelta = new Vector2(MapPanelController.Width, MapPanelController.Height);
    }

    public void ShowRespawnPanel(bool b) {
        gameObject.SetActive(b);
    }

    private void RequireButtonsPanel() {
        GameObject ButtonsPanel = MapPanelController.GetButtonsPanel();
        ButtonsPanel.transform.SetParent(Map_transform);
        TransformExtension.SetLocalTransform(ButtonsPanel.transform, Vector3.zero, Quaternion.identity, new Vector3(1,1,1));
    }
}