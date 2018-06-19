using UnityEngine;

public class Flag : MonoBehaviour {
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private GameObject flag_base;
    private LayerMask Terrain = 10;
    private GameManager gm;
    private PhotonView gmpv;
    public PunTeams.Team team = PunTeams.Team.none;
    private bool isOnPlay = true;
    public bool isGrounded = true, isOnBase = true;

    private void Start() {
        ps.Play();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gmpv = gm.GetComponent<PhotonView>();
    }

    private void Update() {
        if (!isGrounded) {
            if (isOnPlay) {
                ps.Stop();
                flag_base.SetActive(false);
                isOnPlay = false;
            }
        } else {
            if (!isOnPlay) {
                flag_base.SetActive(true);
                ps.Play();
                isOnPlay = true;
            }
        }
    }

    private void OnTriggerEnter(Collider collider) {
        if (!isGrounded || collider.gameObject.layer == Terrain)
            return;

        PhotonView pv = collider.transform.root.GetComponent<PhotonView>();
        PhotonPlayer player = pv.owner;
        if (!pv.isMine)
            return;

        if (player.GetTeam() == team) {
            //do I hold the other team's flag ?
            if (isOnBase && player == ((team == PunTeams.Team.red) ? gm.BlueFlagHolder : gm.RedFlagHolder)) {
                //Register score
                gmpv.RPC("GetScoreRequest", PhotonTargets.MasterClient, pv.viewID);
            } else if (!isOnBase) {
                //send back the flag
                print("send back the flag");
                if (team == PunTeams.Team.red) {
                    Vector3 pos = new Vector3(gm.SpawnPoints[1].transform.position.x, 0, gm.SpawnPoints[1].transform.position.z);
                    gmpv.RPC("SetFlag", PhotonTargets.All, -1, 1, pos);
                } else {
                    Vector3 pos = new Vector3(gm.SpawnPoints[0].transform.position.x, 0, gm.SpawnPoints[0].transform.position.z);
                    gmpv.RPC("SetFlag", PhotonTargets.All, -1, 0, pos);
                }
            }
        } else {
            //send get flag request to master
            gmpv.RPC("GetFlagRequest", PhotonTargets.MasterClient, pv.viewID, (team == PunTeams.Team.blue) ? 0 : 1);
        }
    }
}