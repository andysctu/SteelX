using UnityEngine;
using System.Collections;

public class HealthPool : Photon.MonoBehaviour {
    [SerializeField] private int healAmount = 250;
    [SerializeField] private float healDeltaTime = 2;
    [SerializeField] private GameObject barCanvas;
    private DisplayInfo DisplayInfo;
    private PlayerInZone PlayerInZone;
    private SyncHealthPoolBar syncHealthPoolBar;
    private MechCombat mechCombat;    
    private float LastCheckTime;

    private void Awake() {
        InitComponents();        
    }

    private void Start() {
        GameManager gm = FindObjectOfType<GameManager>();
        StartCoroutine(GetThePlayer(gm));

        DisplayInfo.SetHeight(20);
        DisplayInfo.SetName("HealthPool Infos");
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        GameObject ThePlayer;
        int request_times = 0;
        while((ThePlayer = gm.GetThePlayerMech()) == null && request_times < 15) {
            request_times ++;
            yield return new WaitForSeconds(0.5f);
        }

        if(request_times >= 15) {
            Debug.LogError("Can't get the player");
            yield break;
        }

        InitPlayerRelatedComponents(ThePlayer);
        yield break;
    }

    private void InitComponents() {
        PlayerInZone = GetComponentInChildren<PlayerInZone>();
        syncHealthPoolBar = GetComponent<SyncHealthPoolBar>();
        DisplayInfo = GetComponentInChildren<DisplayInfo>();
    }

    private void InitPlayerRelatedComponents(GameObject player) {
        mechCombat = player.GetComponent<MechCombat>();
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