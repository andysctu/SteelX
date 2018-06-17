using UnityEngine;
using UnityEngine.UI;
public class GreyZone : MonoBehaviour {
    private int curZoneState = -1; //0:blue team ; 1:red team ; -1 : none
    private int curBarState = -1; // same
    private bool switchBarColor = true; // true : need to change color due to barstate change
    private float coeff = 0.005f;
    private float trueAmount = 0;
    private const int BLUE = 0, RED = 1, NONE = -1;
    public GameObject player;
    public int Zone_Id = 0;//to identify which image is on respawnPanel

    [SerializeField] private Image bar, mark;
    [SerializeField] private Sprite bar_blue, bar_blue1, bar_red, bar_red1; //bar_blue1 is the light color one
    [SerializeField] private Sprite mark_blue, mark_red;
    [SerializeField] private Material base_none, base_blue, base_red;
    [SerializeField] private GameObject barCanvas;
    private RespawnPanel RespawnPanel;
    private PlayerInZone PlayerInZone;
    private Camera cam;
    private PhotonView pv;

    public void Init() {//called by gameManager
        PlayerInZone = GetComponent<PlayerInZone>();
        RespawnPanel = (RespawnPanel)Object.FindObjectOfType<RespawnPanel>();
        pv = GetComponent<PhotonView>();
        mark.enabled = false;

        cam = player.GetComponentInChildren<Camera>();
        PlayerInZone.SetPlayerID(player.GetPhotonView().viewID);
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

        if (curZoneState == -1 && switchBarColor) {
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
        if (stream.isWriting) {
            if (PhotonNetwork.isMasterClient) {
                if (PlayerInZone.whichTeamDominate() != NONE) {
                    if (curBarState == PlayerInZone.whichTeamDominate()) {
                        if (bar.fillAmount + PlayerInZone.PlayerCountDiff() * coeff >= 1) {
                            bar.fillAmount = 1;
                            if (curBarState != curZoneState) {
                                pv.RPC("ChangeZone", PhotonTargets.All, curBarState);
                            }
                        } else {
                            bar.fillAmount += PlayerInZone.PlayerCountDiff() * coeff;
                        }
                    } else {
                        if (bar.fillAmount - PlayerInZone.PlayerCountDiff() * coeff <= 0) {
                            bar.fillAmount = 0;
                            curBarState = PlayerInZone.whichTeamDominate();
                            pv.RPC("ChangeZone", PhotonTargets.All, NONE);//change to grey zone
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
    public void ChangeZone(int num) {
        curZoneState = num;
        if (num == BLUE) {
            GetComponent<MeshRenderer>().material = base_blue;
            bar.sprite = bar_blue;
            mark.enabled = true;
            mark.sprite = mark_blue;
        } else if (num == RED) {
            GetComponent<MeshRenderer>().material = base_red;
            bar.sprite = bar_red;
            mark.enabled = true;
            mark.sprite = mark_red;
        } else {
            GetComponent<MeshRenderer>().material = base_none;
            mark.sprite = null;
            mark.enabled = false;
            switchBarColor = true;
        }

        //change mark
        RespawnPanel.ChangeMark(Zone_Id, num);

        //for new player to load
        if (PhotonNetwork.isMasterClient) {
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h.Add("GreyZone_" + Zone_Id, num);
            PhotonNetwork.room.SetCustomProperties(h);
        }
    }
}