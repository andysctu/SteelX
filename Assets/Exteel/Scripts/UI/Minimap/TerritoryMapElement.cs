using UnityEngine;
using UnityEngine.UI;

public class TerritoryMapElement : MapElement {
    [SerializeField] private Image Base, Bar, Mark;    
    [SerializeField] private Sprite RedBase, BlueBase, GreyBase;
    [SerializeField] private Sprite bar_blue, bar_red;
    [SerializeField] private Sprite mark_blue, mark_red;
    private int territory_ID;
    private Text NumText;
    public enum State { BLUE, BLUE_LIGHT, RED, RED_LIGHT, NONE};

    public void SetNumText(int num) {
        if(NumText == null) {
            NumText = GetComponentInChildren<Text>();
        }

        territory_ID = num;
        NumText.text = num.ToString();
    }

    protected override void Start() {
        base.Start();

        Button button = ObjectToAttachOnMapCanvas.AddComponent<Button>();
        button.onClick.AddListener( () => gm.SetRespawnPoint(territory_ID));
    }

    public void SetFillAmount(float amount) {
        Bar.fillAmount = amount;
    }

    public void SwitchBarColor(State state) {//TODO : improve color
        switch (state) {
        case State.BLUE:
            Base.sprite = BlueBase;
            Bar.sprite = bar_blue;
            Mark.enabled = true;
            Mark.sprite = mark_blue;
        break;
        case State.BLUE_LIGHT:
            Base.sprite = GreyBase;
            Bar.sprite = bar_blue;
            Mark.enabled = false;
            Mark.sprite = null;
        break;
        case State.RED:
            Base.sprite = RedBase;
            Bar.sprite = bar_red;
            Mark.enabled = true;
            Mark.sprite = mark_red;
        break;
        case State.RED_LIGHT:
            Base.sprite = GreyBase;
            Bar.sprite = bar_red;
            Mark.enabled = false;
            Mark.sprite = null;
        break;
        case State.NONE:
            Base.sprite = GreyBase;
            Bar.sprite = null;
            Mark.enabled = false;
            Mark.sprite = null;
        break;
        }
    }

}