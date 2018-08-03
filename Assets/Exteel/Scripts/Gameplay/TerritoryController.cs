using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TerritoryController : MonoBehaviour {
    public int Territory_ID = 0;    
    public bool interactable = true;//base : false 
    public GameManager.Team curTerritoryState = GameManager.Team.NONE;
    private MapPanelController MapPanelController;
    private TerritoryRadarElement TerritoryRadarElement;
    private TerritoryMapElement TerritoryMapElement;

    private PlayerInZone PlayerInZone;
    private MeshRenderer baseMesh;
    private PhotonView pv;
    private DisplayInfo DisplayInfo;

    private int curBarState = (int)GameManager.Team.NONE;
    private bool switchBarColor = true; // true : need to change color due to barstate change
    private float coeff = 0.005f;
    private float trueAmount = 0;

    //TODO : consider remake this part
    [SerializeField] private Image bar, mark;
    [SerializeField] private Sprite bar_blue, bar_blue1, bar_red, bar_red1; //bar_blue1 is the light color one
    [SerializeField] private Sprite mark_blue, mark_red;
    [SerializeField] private Material base_none, base_blue, base_red;
    [SerializeField] private GameObject Infos;

    private void Awake() {
        InitComponents();
        TerritoryMapElement.SetNumText(Territory_ID);

        if (!interactable) {
            enabled = false;
            if(DisplayInfo!=null)DisplayInfo.EnableDisplay(false);
        }
    }

    private void Start() {
        if (!interactable) {
            return;
        }

        DisplayInfo.SetHeight(20);
        DisplayInfo.SetName("Territory " + Territory_ID + " Infos");
    }

    private void InitComponents() {
        baseMesh = GetComponentInChildren<MeshRenderer>();
        DisplayInfo = GetComponent<DisplayInfo>();
        TerritoryRadarElement = GetComponentInChildren<TerritoryRadarElement>();
        TerritoryMapElement = GetComponentInChildren<TerritoryMapElement>();
        PlayerInZone = GetComponentInChildren<PlayerInZone>();
        pv = GetComponent<PhotonView>();
        if(mark!=null)mark.enabled = false;
    }

    public void AssignMapPanelController(MapPanelController MapPanelController) {
        if (!interactable) {
            return;
        }

        this.MapPanelController = MapPanelController;
    }

    private void Update() {
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, trueAmount, Time.deltaTime * 10f);
        TerritoryMapElement.SetFillAmount(bar.fillAmount);

        if (curTerritoryState == GameManager.Team.NONE && switchBarColor) {//TODO : remake this part
            if (curBarState == 0) {
                bar.sprite = bar_blue1;
                bar.color = new Color32(255, 255, 255, 255);

                TerritoryMapElement.SwitchBarColor(TerritoryMapElement.State.BLUE_LIGHT);

                switchBarColor = false;
            } else {
                bar.sprite = bar_red1;
                bar.color = new Color32(255, 255, 255, 255);
                switchBarColor = false;

                TerritoryMapElement.SwitchBarColor(TerritoryMapElement.State.RED_LIGHT);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (!interactable) {
            return;
        }

        if (stream.isWriting) {
            if (PhotonNetwork.isMasterClient) {

                if(!GameManager.gameIsBegin)
                    return;

                if (PlayerInZone.whichTeamDominate() != (int)GameManager.Team.NONE) {
                    if (curBarState == PlayerInZone.whichTeamDominate()) {
                        if (bar.fillAmount + PlayerInZone.PlayerCountDiff() * coeff >= 1) {
                            bar.fillAmount = 1;
                            if (curBarState != (int)curTerritoryState) {
                                pv.RPC("ChangeTerritory", PhotonTargets.All, curBarState);
                            }
                        } else {
                            bar.fillAmount += PlayerInZone.PlayerCountDiff() * coeff;
                        }
                    } else {
                        if (bar.fillAmount - PlayerInZone.PlayerCountDiff() * coeff <= 0) {
                            bar.fillAmount = 0;
                            curBarState = PlayerInZone.whichTeamDominate();
                            pv.RPC("ChangeTerritory", PhotonTargets.All, (int)GameManager.Team.NONE);//change to grey zone
                        } else {
                            bar.fillAmount -= PlayerInZone.PlayerCountDiff() * coeff;
                        }
                    }
                }
                trueAmount = bar.fillAmount;
                stream.SendNext(bar.fillAmount);
                stream.SendNext(curBarState);
            }
        } else {
            trueAmount = (float)stream.ReceiveNext();
            curBarState = (int)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void ChangeTerritory(int num) {
        curTerritoryState = (GameManager.Team)num;
        if (num == (int)GameManager.Team.BLUE) {
            baseMesh.material = base_blue;
            bar.sprite = bar_blue;
            mark.enabled = true;
            mark.sprite = mark_blue;

            TerritoryMapElement.SwitchBarColor(TerritoryMapElement.State.BLUE);
        } else if (num == (int)GameManager.Team.RED) {
            baseMesh.material = base_red;
            bar.sprite = bar_red;
            mark.enabled = true;
            mark.sprite = mark_red;

            TerritoryMapElement.SwitchBarColor(TerritoryMapElement.State.RED);
        } else {
            baseMesh.material = base_none;
            mark.sprite = null;
            mark.enabled = false;
            switchBarColor = true;

            TerritoryMapElement.SwitchBarColor(TerritoryMapElement.State.NONE);
        }
        TerritoryRadarElement.SwitchSprite((GameManager.Team)num);

        if (MapPanelController == null) {
            Debug.LogWarning("MapPanelControllers is null");
            return;
        }

        //notify the map to change mark
        //MapPanelController.ChangeMark(Territory_ID, num);     

        //for new player to load
        if (PhotonNetwork.isMasterClient) {
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h.Add("T_" + Territory_ID, num);
            PhotonNetwork.room.SetCustomProperties(h);
        }
    }
}