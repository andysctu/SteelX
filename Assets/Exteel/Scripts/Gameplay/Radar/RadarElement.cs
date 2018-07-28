using UnityEngine;

public class RadarElement : MonoBehaviour {

    protected virtual void Start() {
        RegisterThisToRadars();
    }

    private void RegisterThisToRadars() {
        Radar[] radars = FindObjectsOfType<Radar>();
        foreach(Radar radar in radars) {
            radar.RegisterRadarElement(this);
        }
    }
}