using UnityEngine;
using UnityEngine.UI;

public class RespawnPanel : MonoBehaviour {
    [SerializeField] private Transform MechButtons, Map;
    private GameManager gm;

    private void Start() {
        gm = FindObjectOfType<GameManager>();

        //init respawn buttons
        Button[] RespawnButtons = MechButtons.GetComponentsInChildren<Button>();
        for(int i = 0; i < RespawnButtons.Length; i++) {
            RespawnButtons[i].onClick.AddListener(() => gm.CallRespawn(i));
        }
    }

    public void InitMap() {
        GameObject g = FindObjectOfType<MapPanelController>().gameObject;
        GameObject map = Instantiate(g, Map);
        map.transform.localPosition = Vector3.zero;
        //map.GetComponent<MapPanelController>().InitButtons();
    }

    public void ShowRespawnPanel(bool b) {
        gameObject.SetActive(b);
    }
}