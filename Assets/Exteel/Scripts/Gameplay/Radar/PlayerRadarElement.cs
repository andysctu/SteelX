using UnityEngine;

public class PlayerRadarElement : RadarElement {
    [SerializeField] private Sprite Red, Blue, Self_blue, Self_red;
    [SerializeField] private TextMesh nameTextMesh;
    private PhotonView root_pv;

    protected override void Awake() {
        base.Awake();
        root_pv = transform.root.GetComponent<PhotonView>();
    }
    protected override void Start() {
        base.Start();
    }

    protected override void OnGetPlayerAction() {
        base.OnGetPlayerAction();
        
        UpdateNameText();
        SwitchSprite();
    }

    private void UpdateNameText() {
        if (root_pv == null || root_pv.owner == null) {
            nameTextMesh.text = transform.root.name;
        } else {
            nameTextMesh.text = root_pv.owner.NickName;
        }
    }

    private void SwitchSprite() {
        //Check if this is me        
        if (root_pv.isMine && root_pv.tag != "Drone") {
            nameTextMesh.text = "";
            if (GameManager.isTeamMode) {
                if (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) {
                    SpriteRenderer.sprite = Self_red;
                } else {
                    SpriteRenderer.sprite = Self_blue;
                }
            } else {
                SpriteRenderer.sprite = Self_blue;
            }
            return;
        }

        //Drone
        if (root_pv.tag == "Drone") {
            SpriteRenderer.sprite = Red;
            nameTextMesh.color = Color.red;
            return;
        }

        if (GameManager.isTeamMode) {
            if (ThePlayer.GetPhotonView() == null || ThePlayer.GetPhotonView().owner == null) {
                SpriteRenderer.sprite = Red;
                nameTextMesh.color = Color.red;
            } else {
                //Check team
                if (ThePlayer.GetPhotonView().owner.GetTeam() == root_pv.owner.GetTeam()) {
                    SpriteRenderer.sprite = (root_pv.owner.GetTeam() == PunTeams.Team.red) ? Red : Blue;
                    nameTextMesh.color = Color.green;
                } else {
                    SpriteRenderer.sprite = (root_pv.owner.GetTeam() == PunTeams.Team.red) ? Red : Blue;
                    nameTextMesh.color = Color.red;
                }
            }
        } else {
            SpriteRenderer.sprite = Red;
            nameTextMesh.color = Color.red;
        }
    }

    public void OnPlayerDead() {
    }

    public void OnPlayerBroadcast() {
    }
}