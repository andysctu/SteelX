using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
    private Combat Combat;
    private PhotonView pv;
    private Slider HPBar, ENBar;
    private Image ENBar_fill;
    private bool ENNotEnoughEffectIsPlaying = false;
    private Text HPtext, ENtext;

    private void Start() {
        Combat = GetComponent<Combat>();
        pv = Combat.photonView;

        if (!pv.isMine) {
            enabled = false;
            return;
        }
        InitComponents();
    }

    private void InitComponents() {
        InitHealthAndENBars();
    }

    private void InitHealthAndENBars() {
        Slider[] sliders = GameObject.Find("PanelCanvas/HUDPanel/HUD").GetComponentsInChildren<Slider>();
        if (sliders.Length > 0) {
            HPBar = sliders[0];
            HPBar.value = 1;
            HPtext = HPBar.GetComponentInChildren<Text>();
            if (sliders.Length > 1) {
                ENBar = sliders[1];
                ENBar_fill = ENBar.transform.Find("Fill Area/Fill").GetComponent<Image>();
                ENBar.value = 1;
                ENtext = ENBar.GetComponentInChildren<Text>();
            }
        }
    }

    private void Update() {
        // Update Health bar gradually
        HPBar.value = calculateSliderPercent(HPBar.value, Combat.CurrentHP / (float)Combat.MAX_HP);
        HPtext.text = UIExtensionMethods.BarValueToString((int)(Combat.MAX_HP * HPBar.value), Combat.MAX_HP);

        // Update EN bar gradually
        ENBar.value = calculateSliderPercent(ENBar.value, Combat.CurrentEN / (float)Combat.MAX_EN);
        ENtext.text = UIExtensionMethods.BarValueToString((int)(Combat.MAX_EN * ENBar.value), (int)Combat.MAX_EN);
    }

    // Returns currentPercent + 0.01 if currentPercent < targetPercent, else - 0.01
    private float calculateSliderPercent(float currentPercent, float targetPercent) {
        float err = 0.015f;
        if (Mathf.Abs(currentPercent - targetPercent) > err) {
            currentPercent = currentPercent + (currentPercent > targetPercent ? -err : err);
        } else {
            currentPercent = targetPercent;
        }
        return currentPercent;
    }

    public void PlayENnotEnoughEffect() {
        if (!ENNotEnoughEffectIsPlaying) {
            StartCoroutine(ENNotEnoughEffect());
        }
    }

    private IEnumerator ENNotEnoughEffect() {
        ENNotEnoughEffectIsPlaying = true;
        for (int i = 0; i < 4; i++) {
            ENBar_fill.color = new Color32(133, 133, 133, 255);
            yield return new WaitForSeconds(0.15f);
            ENBar_fill.color = new Color32(255, 255, 255, 255);
            yield return new WaitForSeconds(0.15f);
        }
        ENNotEnoughEffectIsPlaying = false;
        yield break;
    }
}