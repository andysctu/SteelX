using System.Collections;
using UnityEngine;

public class TerritoryRadarElement : RadarElement {
    private TerritoryController parentTerritoryController;
    private TextMesh IDtextMesh;
    [SerializeField] private Sprite blueTerritory, redTerritory, greyTerritory;
    [SerializeField] private Sprite underAttack;

    protected override void Start() {
        base.Start();

        InitComponents();
        DisplayTerritoryID();
    }

    private void InitComponents() {
        IDtextMesh = GetComponentInChildren<TextMesh>();
        parentTerritoryController = transform.parent.GetComponent<TerritoryController>();
    }

    private void DisplayTerritoryID() {
        //IDtextMesh.text = parentTerritoryController.Territory_ID.ToString();
    }

    public void UnderAttack() {

    }

    //public void SwitchSprite(PunTeams.Team team) {
    //    switch (team) {
    //        case PunTeams.Team.blue:
    //            SpriteRenderer.sprite = blueTerritory;
    //        break;
    //        case PunTeams.Team.red:
    //            SpriteRenderer.sprite = redTerritory;
    //        break;
    //        case PunTeams.Team.none:
    //            SpriteRenderer.sprite = greyTerritory;
    //        break;
    //    }
    //}
}