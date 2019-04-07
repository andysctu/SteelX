using UnityEngine;

public class Flag : MonoBehaviour {
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private GameObject flag_base;
    [SerializeField] private BoxCollider BoxCollider;
    private CTFManager CTFManager;
    private LayerMask Terrain = 10, IgnoreRayCast = 2;
    //private PhotonView gmpv;
    private bool isGrounded = true, isOnBase = true;
    private float remainUntouchedStartTime = 0;
    private const float MaxUntouchedDuration = 30;
    //public PunTeams.Team flag_team = PunTeams.Team.none;

    private enum ColliderSize { SMALL, BIG };

    public bool Debug_sendBack = false;

    private void Awake() {
        ps.Play();
    }

    //private void Start() {       
    //    CTFManager = FindObjectOfType<CTFManager>();
    //    gmpv = CTFManager.GetComponent<PhotonView>();
    //}

    private void FixedUpdate() {
        if (!isOnBase && isGrounded) {
            if(Time.time - remainUntouchedStartTime > MaxUntouchedDuration) {
                //Send back the flag
                //if (PhotonNetwork.isMasterClient)
                //    gmpv.RPC("SetFlag", PhotonTargets.All, true, -1, (flag_team == PunTeams.Team.red) ? (int)PunTeams.Team.red : (int)PunTeams.Team.blue,
                //        (flag_team == PunTeams.Team.red) ? CTFManager.GetRedBasePosition() : CTFManager.GetBlueBasePosition() );

                //also reset time
                remainUntouchedStartTime = Time.time;
            }            
        } else {
            //reset time
            remainUntouchedStartTime = Time.time;
        }

        if (Debug_sendBack) {
            Debug_sendBack = false;
            remainUntouchedStartTime = 0;
        }
    }

    private void OnTriggerEnter(Collider collider) {
        if (!isGrounded || collider.gameObject.layer == Terrain) return;

        //PhotonView pv = collider.transform.root.GetComponent<PhotonView>();

        //if (pv == null) {
        //    Debug.LogWarning(collider.name + " does not have photonview but triggered the flag.");
        //    return;
        //}

        //if (!pv.isMine) return;
        //PhotonPlayer player = pv.owner;

        if (CTFManager == null) {
            Debug.Log("Flag is triggered but CTFManager is not init.");
            return;
        }

        //PhotonMessageInfo info = new PhotonMessageInfo(PhotonNetwork.player, PhotonNetwork.ServerTimestamp, pv);

        //if (player.GetTeam() == flag_team) {
        //    //Do I hold the other team's flag ?
        //    if (isOnBase && player == ((flag_team == PunTeams.Team.red) ? CTFManager.BlueFlagHolder : CTFManager.RedFlagHolder)) {
        //        gmpv.RPC("GetScoreRequest", PhotonTargets.MasterClient, info);
        //    } else if (!isOnBase) {
        //        //Send back the flag
        //        gmpv.RPC("GetFlagRequest", PhotonTargets.MasterClient, (flag_team == PunTeams.Team.red) ? (int)PunTeams.Team.red : (int)PunTeams.Team.blue, info);                
        //    }
        //} else {
        //    //send get flag request to master
        //    gmpv.RPC("GetFlagRequest", PhotonTargets.MasterClient, (flag_team == PunTeams.Team.red) ? (int)PunTeams.Team.red : (int)PunTeams.Team.blue, info);
        //}
    }

    public void OnParentToPlayerAction() {
        isGrounded = false;
        isOnBase = false;

        EnableEffects(false);
    }

    public void OnDroppedAction() {
        SetFlagCollidorSize(ColliderSize.SMALL);
        isGrounded = true;
        isOnBase = false;        
        gameObject.layer = IgnoreRayCast;

        EnableEffects(true);
    }

    public void OnBaseAction() {
        SetFlagCollidorSize(ColliderSize.BIG);
        isOnBase = true;
        isGrounded = true;

        EnableEffects(true);
    }

    private void SetFlagCollidorSize(ColliderSize size) {
        if (size == ColliderSize.SMALL) {
            BoxCollider.size = new Vector3(4.3f, 10, 4.5f);
        } else {
            BoxCollider.size = new Vector3(38.62f, 10, 37.54f);
        }
    }

    private void EnableEffects(bool b) {
        flag_base.SetActive(b);        

        if (b) {
            ps.Play();
        } else {
            ps.Stop();
        }
    }
}