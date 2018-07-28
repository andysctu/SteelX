using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInZone))]
public class HealthPool : Photon.MonoBehaviour {
    [SerializeField] private int healAmount = 250;
    [SerializeField] private float healDeltaTime = 2;
    [SerializeField] private GameObject barCanvas;
    private PlayerInZone PlayerInZone;
    private SyncHealthPoolBar syncHealthPoolBar;
    private Camera cam;
    private MechCombat mechCombat;    
    private float LastCheckTime;

    private void Awake() {
        InitComponents();        
    }

    private void Start() {
        GameManager gm = FindObjectOfType<GameManager>();
        StartCoroutine(GetThePlayer(gm));
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        GameObject ThePlayer;
        int request_times = 0;
        while((ThePlayer = gm.GetThePlayer()) == null && request_times < 10) {
            request_times ++;
            yield return new WaitForSeconds(0.5f);
        }

        if(request_times >= 10) {
            Debug.LogError("Can't get the player");
            yield break;
        }

        InitPlayerRelatedComponents(ThePlayer);
        yield break;
    }

    private void InitComponents() {
        PlayerInZone = GetComponent<PlayerInZone>();
        syncHealthPoolBar = GetComponent<SyncHealthPoolBar>();
    }

    private void InitPlayerRelatedComponents(GameObject player) {
        cam = player.GetComponentInChildren<Camera>();
        mechCombat = player.GetComponent<MechCombat>();
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
    }

    private void FixedUpdate() {
        if (PlayerInZone.IsThePlayerInside()) {
            if (Time.time - LastCheckTime >= healDeltaTime) {
                if (!mechCombat.IsHpFull() && syncHealthPoolBar.isAvailable) {
                    if (mechCombat.GetMaxHp() - mechCombat.CurrentHP >= healAmount) {
                        LastCheckTime = Time.time;
                        mechCombat.photonView.RPC("OnHeal", PhotonTargets.All, 0, healAmount);
                    } else {
                        LastCheckTime = Time.time;
                        mechCombat.photonView.RPC("OnHeal", PhotonTargets.All, 0, (mechCombat.GetMaxHp() - mechCombat.CurrentHP));
                    }
                } else {
                    LastCheckTime = Time.time;
                }
            }
        } else {
            LastCheckTime = Time.time;
        }
    }
}