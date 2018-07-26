﻿using UnityEngine;
using UnityEngine.UI;

public class TerritoryController : MonoBehaviour {
    public int Territory_ID = 0;    
    public bool interactable = true;//base : false 
    public GameManager.Team curTerritoryState = GameManager.Team.NONE;
    private MapPanelController[] MapPanelControllers;
    private PlayerInZone PlayerInZone;
    private MeshRenderer baseMesh;
    private Camera cam;
    private PhotonView pv;

    private int curBarState = (int)GameManager.Team.NONE;
    private bool switchBarColor = true; // true : need to change color due to barstate change
    private float coeff = 0.005f;
    private float trueAmount = 0;
    private GameObject playerToLookAt;

    //TODO : consider remake this part
    [SerializeField] private Image bar, mark;
    [SerializeField] private Sprite bar_blue, bar_blue1, bar_red, bar_red1; //bar_blue1 is the light color one
    [SerializeField] private Sprite mark_blue, mark_red;
    [SerializeField] private Material base_none, base_blue, base_red;
    [SerializeField] private GameObject barCanvas;

    private void Awake() {
        if (!interactable) {
            enabled = false;
            return;
        }
        InitComponents();
    }

    private void InitComponents() {
        baseMesh = GetComponent<MeshRenderer>();
        PlayerInZone = GetComponent<PlayerInZone>();
        pv = GetComponent<PhotonView>();
        mark.enabled = false;
    }

    public void FindMapPanels(MapPanelController[] MapPanelControllers) {
        if (!interactable) {
            enabled = false;
            return;
        }
        this.MapPanelControllers = new MapPanelController[MapPanelControllers.Length];
        MapPanelControllers.CopyTo(this.MapPanelControllers, 0);
    }

    public void SetPlayerToLookAt(GameObject player) {
        playerToLookAt = player;
        cam = playerToLookAt.GetComponentInChildren<Camera>();
        if(interactable)PlayerInZone.SetPlayerID(playerToLookAt.GetPhotonView().viewID);
    }

    private void Update() {
        if (cam != null) {
            barCanvas.transform.LookAt(new Vector3(cam.transform.position.x, barCanvas.transform.position.y, cam.transform.position.z));

            //update scale
            float distance = Vector3.Distance(transform.position, cam.transform.position);
            distance = Mathf.Clamp(distance, 0, 200f);
            barCanvas.transform.localScale = new Vector3(0.02f + distance / 100 * 0.02f, 0.02f + distance / 100 * 0.02f, 1);
        }

        bar.fillAmount = Mathf.Lerp(bar.fillAmount, trueAmount, Time.deltaTime * 10f);

        if (curTerritoryState == GameManager.Team.NONE && switchBarColor) {
            if (curBarState == 0) {
                bar.sprite = bar_blue1;
                bar.color = new Color32(255, 255, 255, 255);
                switchBarColor = false;
            } else {
                bar.sprite = bar_red1;
                bar.color = new Color32(255, 255, 255, 255);
                switchBarColor = false;
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
        } else if (num == (int)GameManager.Team.RED) {
            baseMesh.material = base_red;
            bar.sprite = bar_red;
            mark.enabled = true;
            mark.sprite = mark_red;
        } else {
            baseMesh.material = base_none;
            mark.sprite = null;
            mark.enabled = false;
            switchBarColor = true;
        }

        if(MapPanelControllers == null) {
            Debug.LogWarning("MapPanelControllers is null");
            return;
        }

        //notify all maps to change mark
        foreach(MapPanelController m in MapPanelControllers) {
            m.ChangeMark(Territory_ID, num);
        }

        //for new player to load
        if (PhotonNetwork.isMasterClient) {
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h.Add("T_" + Territory_ID, num);
            PhotonNetwork.room.SetCustomProperties(h);
        }
    }
}