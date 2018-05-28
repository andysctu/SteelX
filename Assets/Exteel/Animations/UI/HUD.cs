using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    [SerializeField] private HUDText Hit, Kill, Defense;

    //Only display one hit msg at the same time

    public void DisplayHit(Camera cam) {//this is called by the player who needs to see
        Hit.Display(cam);
    }

    public void DisplayDefense(Camera cam) {
        Defense.Display(cam);
    }

    public void DisplayKill(Camera cam) {
        Kill.Display(cam);
    }
}
