using UnityEngine.UI;
using UnityEngine;

public class MapPanelController : MonoBehaviour {
    [SerializeField] private Camera MapCamera;
    public int Width, Height;
    public float scale = 1;

    [Tooltip("The order must match the IDs")]
    [SerializeField] private GameObject ButtonsPanel;
    [SerializeField] private GameObject[] RespawnPointsOnMap;
    private Button[] RespawnPointButtons;

    public void Awake() {
        InitButtons();
        InitRenderTexture();
    }

    private void InitButtons() {
        GameManager gm = FindObjectOfType<GameManager>();
        RespawnPointButtons = new Button[RespawnPointsOnMap.Length];
        for (int i = 0; i < RespawnPointButtons.Length; i++) {
            if (RespawnPointsOnMap[i] == null)continue;
            RespawnPointButtons[i] = RespawnPointsOnMap[i].GetComponent<Button>();

            int index = i;
            RespawnPointButtons[i].onClick.AddListener(() => gm.SetRespawnPoint(index));
        }
    }

    private void InitRenderTexture() {
        RenderTexture renderTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32);
        MapCamera.targetTexture = renderTexture;
    }

    public float GetScale() {
        return scale;
    }
    
    public GameObject GetButtonsPanel() {
        return ButtonsPanel;
    }
}
