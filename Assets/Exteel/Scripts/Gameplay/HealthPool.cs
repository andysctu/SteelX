using UnityEngine;

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

    private void InitComponents() {
        PlayerInZone = GetComponent<PlayerInZone>();
        syncHealthPoolBar = GetComponent<SyncHealthPoolBar>();
    }

    public void Init(GameObject player) {
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