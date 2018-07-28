using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Radar : MonoBehaviour {
    private Transform radarCam;
    private Transform ThePlayer;    
    private List<RadarElement> radarElements = new List<RadarElement>();
    public bool followThePlayer = true;

    private void Start() {
        radarCam = GetComponentInChildren<Camera>().transform;

        GameManager gm = FindObjectOfType<GameManager>();
        if(followThePlayer)StartCoroutine(GetThePlayer(gm));
    }

    public void RegisterRadarElement(RadarElement radarElement) {
        radarElements.Add(radarElement);
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        GameObject ThePlayer;
        int request_times = 0;
        while ((ThePlayer = gm.GetThePlayer()) == null && request_times < 10) {
            yield return new WaitForSeconds(0.5f);
        }

        if(request_times >= 10) {
            Debug.LogError("Can't get the player");
            yield break;
        }

        InitPlayerRelatedComponents(ThePlayer);
        yield break;
    }

    private void InitPlayerRelatedComponents(GameObject ThePlayer) {
        this.ThePlayer = ThePlayer.transform;
    }

    private void Update() {
        if(ThePlayer==null) return;

        //Update cam position
        radarCam.position = ThePlayer.position + Vector3.up * 100;
        radarCam.rotation = Quaternion.Euler( new Vector3(90, ThePlayer.rotation.eulerAngles.y + 90, 90));
    }

    private void FixedUpdate() {
        if (ThePlayer == null) return;

        //Update radar elements position
        foreach (RadarElement radarElement in radarElements) {

            
        }
    }
}