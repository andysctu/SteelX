using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    [SerializeField] private HUDText Hit, Kill, Defense;

    // HUD
    private Slider healthBar;
    private Slider ENBar;
    private Image ENBar_fill;
    private bool ENNotEnoughEffectIsPlaying = false;
    private bool isENAvailable = true;
    private Text healthtext, ENtext;

    //Only display one hit msg at the same time

    private void initHealthAndENBars() {
        Slider[] sliders = GameObject.Find("PanelCanvas").GetComponentsInChildren<Slider>();
        if (sliders.Length > 0) {
            healthBar = sliders[0];
            healthBar.value = 1;
            healthtext = healthBar.GetComponentInChildren<Text>();
            if (sliders.Length > 1) {
                ENBar = sliders[1];
                ENBar_fill = ENBar.transform.Find("Fill Area/Fill").GetComponent<Image>();
                ENBar.value = 1;
                ENtext = ENBar.GetComponentInChildren<Text>();
            }
        }
    }

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
