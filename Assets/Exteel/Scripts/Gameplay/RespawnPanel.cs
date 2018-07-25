using UnityEngine;
using UnityEngine.UI;

public class RespawnPanel : MonoBehaviour {
    [SerializeField] private Transform MechRespawnButtons, Map_transform;
    private GameManager gm;

    private void Start() {
        gm = FindObjectOfType<GameManager>();

        //Init respawn buttons
        Button[] RespawnButtons = MechRespawnButtons.GetComponentsInChildren<Button>();
        for(int i = 0; i < RespawnButtons.Length; i++) {
            int respawnIndex = i;
            RespawnButtons[i].onClick.AddListener(() => gm.CallRespawn(respawnIndex));
        }

        //Map panel
        InitMap();
    }

    private void InitMap() {
        GameObject MapToInstantiate = gm.GetMap();
        GameObject map = Instantiate(MapToInstantiate, Map_transform);
        map.transform.localPosition = Vector3.zero;
    }

    public void ShowRespawnPanel(bool b) {
        gameObject.SetActive(b);
    }
}