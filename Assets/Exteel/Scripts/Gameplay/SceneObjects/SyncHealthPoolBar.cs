using UnityEngine;
using UnityEngine.UI;

public class SyncHealthPoolBar : MonoBehaviour {
    [SerializeField] private Sprite bar_green, bar_grey;
    [SerializeField] private Image bar;    
    [SerializeField] private float increaseAmount = 0.001f;
    private PlayerInZone PlayerInZone;
    public bool isAvailable = true;
    private float trueAmount = 1;
    private const int GREEN = 0, GREY = 1;

    private void Awake() {
        PlayerInZone = GetComponentInChildren<PlayerInZone>();
    }

    private void Start() {
        if (isAvailable) {//check state
            SetColor(GREEN);
        } else {
            SetColor(GREY);
        }
    }

    //[PunRPC]
    private void SetColor(int color) {
        if (color == GREEN) {
            isAvailable = true;
            bar.sprite = bar_green;
        } else {
            isAvailable = false;
            bar.sprite = bar_grey;
        }
    }

    private void Update() {
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, trueAmount, Time.deltaTime * 10f);
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    //    if (stream.isWriting) {
    //        if (PhotonNetwork.isMasterClient) {
    //            if (!isAvailable) {
    //                bar.fillAmount += increaseAmount;
    //                if (bar.fillAmount >= 1) {
    //                    isAvailable = true;
    //                    photonView.RPC("SetColor", PhotonTargets.All, GREEN);
    //                }
    //            }
	//
    //            if (bar.fillAmount > 0 && isAvailable) {
    //                bar.fillAmount -= PlayerInZone.getNotFullHPPlayerCount() * increaseAmount;
    //            }
	//
    //            if (bar.fillAmount <= 0 && isAvailable) {
    //                isAvailable = false;
    //                photonView.RPC("SetColor", PhotonTargets.All, GREY);
    //            }
    //            trueAmount = bar.fillAmount;
    //            stream.SendNext(bar.fillAmount);
    //        }
    //    } else {
    //        trueAmount = (float)stream.ReceiveNext();
    //    }
    //}
}