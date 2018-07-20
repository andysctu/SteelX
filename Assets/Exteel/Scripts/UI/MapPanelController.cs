using UnityEngine.UI;
using UnityEngine;

public class MapPanelController : MonoBehaviour {
    [Tooltip("The order must match the IDs")]
    [SerializeField] private GameObject[] RespawnPointsOnMap;
    [SerializeField] private Button[] RespawnPointButtons;
    [SerializeField] private Sprite Panel_bluemark, Panel_redmark, Panel_greymark;
    private Image[] Territory_marks;

    public void Awake() {
        GameManager gm = FindObjectOfType<GameManager>();
        RespawnPointButtons = new Button[RespawnPointsOnMap.Length];
        Territory_marks = new Image[RespawnPointsOnMap.Length];
        for (int i = 0; i < RespawnPointButtons.Length; i++) {
            Territory_marks[i] = RespawnPointsOnMap[i].GetComponent<Image>();
            RespawnPointButtons[i] = RespawnPointsOnMap[i].GetComponent<Button>();
            RespawnPointButtons[i].onClick.AddListener(() => gm.CallRespawn(i));
        }
    }

    private void Update() {
        UpdatePlayerPosOnMap();
    }

    private void UpdatePlayerPosOnMap() {

    }

    public void ChangeMark(int territory_id, int num) {
        switch (num) {
            case (int)GameManager.Team.BLUE:
            Territory_marks[territory_id].sprite = Panel_bluemark;
            break;
            case (int)GameManager.Team.RED:
            Territory_marks[territory_id].sprite = Panel_redmark;
            break;
            case (int)GameManager.Team.NONE:
            Territory_marks[territory_id].sprite = Panel_greymark;
            break;
        }
    }
}
