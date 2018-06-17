using UnityEngine;
using UnityEngine.UI;

public class RespawnPanel : MonoBehaviour {
    [SerializeField] private Sprite Panel_bluemark, Panel_redmark, Panel_greymark;
    [SerializeField] private GameObject[] zone_marks;
    private const int BLUE = 0, RED = 1, NONE = -1;

    public void ChangeMark(int zone_id, int num) {
        Image zone_mark = zone_marks[zone_id].GetComponent<Image>();

        switch (num) {
            case BLUE:
            zone_mark.sprite = Panel_bluemark;
            break;
            case RED:
            zone_mark.sprite = Panel_redmark;
            break;
            case NONE:
            zone_mark.sprite = Panel_greymark;
            break;
        }
    }
}