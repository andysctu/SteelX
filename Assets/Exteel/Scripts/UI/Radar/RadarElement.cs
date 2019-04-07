using System.Collections;
using UnityEngine;

public class RadarElement : MonoBehaviour {
    protected Radar Radar;
    protected SpriteRenderer SpriteRenderer;
    protected GameObject ThePlayer;

    protected virtual void Awake() {
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void Start() {
        //GameManager gm = FindObjectOfType<GameManager>();        
        //StartCoroutine(GetThePlayer(gm));
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        int request_times = 0;
        //while ((ThePlayer = gm.GetThePlayerMech()) == null && request_times < 10) {
        //    request_times++;
        //    yield return new WaitForSeconds(0.5f);
        //}

        if (request_times >= 10) {
            Debug.LogError("Can't get the player");
            yield break;
        } else {
            OnGetPlayerAction();
        }
        yield break;
    }

    protected virtual void LateUpdate() {
        if (Radar != null) transform.rotation = Quaternion.Euler(0, Radar.transform.rotation.eulerAngles.y, 0);
    }

    protected virtual void OnGetPlayerAction() {
        RegisterThisToRadar();
    }

    private void RegisterThisToRadar() {
        Radar = ThePlayer.GetComponentInChildren<Radar>();
        Radar.RegisterRadarElement(this);
    }

    public void ShowElementOnRadar(bool b) {
        SpriteRenderer.enabled = b;
    }
}