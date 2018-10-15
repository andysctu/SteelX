using UnityEngine;
using UnityEngine.UI;

public class PlayerMapElement : MapElement {
    [SerializeField] private Image Point;
    [SerializeField] private Text NameText;
    [SerializeField] private Sprite Red, Blue, Self_Red, Self_Blue;
    private PhotonView root_pv;
    private bool applyRotSeperately = false;

    private void Awake() {
        root_pv = transform.root.GetComponent<PhotonView>();
    }

    protected override void Start() {
        base.Start();
    }

    protected override void OnGetPlayerAction() {
        base.OnGetPlayerAction();

        UpdateNameText();
        SwitchSprite();

        if (root_pv.isMine) {
            applyRotSeperately = true;
            transform.localRotation = Quaternion.identity;
        }
    }

    private void UpdateNameText() {
        if (root_pv == null || root_pv.owner == null) {
            NameText.text = transform.root.name;
        } else {
            NameText.text = root_pv.owner.NickName;
        }
    }

    private void SwitchSprite() {
        //Check if this is me        
        if (root_pv.isMine && root_pv.tag != "Drone") {
            if (GameManager.IsTeamMode) {
                if (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) {
                    Point.sprite = Self_Red;
                } else {
                    Point.sprite = Self_Blue;
                }
            } else {
                Point.sprite = Self_Blue;
            }
            NameText.color = Color.yellow;
            return;
        }

        //Drone
        if (root_pv.tag == "Drone") {
            Point.sprite = Red;
            NameText.color = Color.red;
            return;
        }

        if (GameManager.IsTeamMode) {
            if (ThePlayer.GetPhotonView() == null || ThePlayer.GetPhotonView().owner == null) {
                Point.sprite = Red;
                NameText.color = Color.red;
            } else {
                //Check team
                if (ThePlayer.GetPhotonView().owner.GetTeam() == root_pv.owner.GetTeam()) {
                    Point.sprite = (root_pv.owner.GetTeam() == PunTeams.Team.red)? Red : Blue;
                    NameText.color = Color.green;
                } else {
                    Point.sprite = (root_pv.owner.GetTeam() == PunTeams.Team.red) ? Red : Blue;
                    NameText.color = Color.red;
                }
            }
        } else {
            Point.sprite = Red;
            NameText.color = Color.red;
        }
    }

    protected override void LateUpdate() {
        ObjectToAttachOnMapCanvas.transform.rotation = Quaternion.Euler(90, MapCamera.transform.rotation.eulerAngles.y, 0);//TODO : Check if this necessary
        ObjectToAttachOnMapCanvas.transform.position = transform.position + Vector3.up * 450;

        if (applyRotSeperately) {
            Point.transform.rotation = Quaternion.Euler(90, transform.root.rotation.eulerAngles.y, 0);
        }
    }

    public void OnPlayerDead() {
    }

    public void OnPlayerBroadcast() {
    }
}