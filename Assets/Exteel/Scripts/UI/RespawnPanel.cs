using UnityEngine;
using UnityEngine.UI;

public class RespawnPanel : MonoBehaviour {
    [SerializeField] private GameObject RespawnMechStat;
    [SerializeField] private BuildMech Mech;
    [SerializeField] private Transform MechRespawnButtons, Map_transform, Weapon_Slots, Skill_Slots;
    private Camera MapCamera;
    private MapPanelController MapPanelController;
    private GameManager gm;
    private RawImage MapRawImage;
    
    private void Start() {
        InitComponents();

        InitRespawnButtons();

        //Display map
        AssignMapRenderTexture();

        //The buttons on map panel
        RequireButtonsPanel();
    }

    private void InitComponents() {
        gm = FindObjectOfType<GameManager>();

        GameObject Map = gm.GetMap();
        MapCamera = Map.GetComponentInChildren<Camera>();
        MapRawImage = GetComponentInChildren<RawImage>();
        MapPanelController = Map.GetComponentInChildren<MapPanelController>();        
    }

    private void InitRespawnButtons() {
        int player_mechLength = GetPlayerMechLength();
        Button[] RespawnButtons = new Button[player_mechLength];

        for (int i = 0; i < player_mechLength; i++) {
            GameObject g = Instantiate(RespawnMechStat, MechRespawnButtons);
            TransformExtension.SetLocalTransform(g.transform);
            g.transform.Find("Status/Order").GetComponent<Text>().text = i.ToString();

            //Init buttons
            int respawnIndex = i;
            Transform DisplayTransform = g.transform.Find("Display");
            DisplayTransform.GetComponent<Button>().onClick.AddListener(() => DisplayMech(respawnIndex) );
            RespawnButtons[i] = g.transform.Find("Spawn").GetComponent<Button>();
            RespawnButtons[i].onClick.AddListener(() => gm.CallRespawn(respawnIndex));
            //TODO : Implement these

            //Init mech name
            //g.transform.Find("Status/Name").GetComponent<Text>().text

            //Init mechanaught loading bar
        }
    }

    private int GetPlayerMechLength() {
        return UserData.myData.Mech.Length;
    }

    private void AssignMapRenderTexture() {
        MapRawImage.texture = MapCamera.targetTexture;
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

    public void DisplayMech(int mech_num) {
        Mech m = UserData.myData.Mech[mech_num];
        Mech.buildMech(PhotonNetwork.player, m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
    }
}