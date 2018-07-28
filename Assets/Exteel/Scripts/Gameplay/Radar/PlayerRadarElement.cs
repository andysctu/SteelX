using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerRadarElement : RadarElement {
    [SerializeField]private MeshRenderer plane;
    [SerializeField]private Material Enemy, Ally;
    [SerializeField]private TextMesh nameTextMesh;
    private GameObject ThePlayer;

    protected override void Start() {
        base.Start();
        if (GameManager.isTeamMode) {
            GameManager gm = FindObjectOfType<GameManager>();
            StartCoroutine(GetThePlayer(gm));
        } else {
            plane.material = Enemy;
        }

        if(transform.root.GetComponent<PhotonView>() == null || transform.root.GetComponent<PhotonView>().owner == null) {
            nameTextMesh.text = transform.root.name;
        } else {
            nameTextMesh.text = transform.root.GetComponent<PhotonView>().owner.NickName;
        }        
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        GameObject ThePlayer;
        int request_times = 0;
        while ((ThePlayer = gm.GetThePlayer()) == null && request_times < 10) {
            request_times ++;
            yield return new WaitForSeconds(0.5f);
        }
        
        if(request_times >= 10) {
            Debug.LogError("Can't get the player");
            yield break;
        } else {
            if(ThePlayer.GetPhotonView()==null || ThePlayer.GetPhotonView().owner == null) {
                plane.material = Enemy;
                yield break;
            }

            //Check team
            if(ThePlayer.GetPhotonView().owner.GetTeam() == PhotonNetwork.player.GetTeam()) {
                plane.material = Ally;
            } else {
                plane.material = Enemy;
            }
        }

        yield break;
    }

    public void OnPlayerBroadcast() {

    }
}