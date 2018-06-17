using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour {
    [SerializeField] private Image barL, barR;
    [SerializeField] private Image barL_fill, barR_fill;
    [SerializeField] private BuildMech bm;
    [SerializeField] private MechCombat mcbt;
    [SerializeField] private PhotonView pv;//mech combat's pv

    private Weapon[] weaponScripts;
    private float[] curValue = new float[4];
    private const float CooldownCoeffWhenNotOverHeat = 0.2f;
    private int cooldownRate, weaponOffset;
    private int MaxHeat;
    private Color32 RED = new Color32(255, 0, 0, 200), YELLOW = new Color32(255, 255, 85, 200);

    private void Awake() {
        RegisterOnMechBuilt();
        RegisterOnWeaponSwitched();
    }

    private void RegisterOnMechBuilt() {
        if (bm != null) {
            bm.OnMechBuilt += InitVars;
            bm.OnMechBuilt += ResetHeatBar;
        }
    }

    private void RegisterOnWeaponSwitched() {
        if (mcbt != null) {
            mcbt.OnWeaponSwitched += UpdateHeatBar;
        }
    }

    private void InitVars() {//called when finished buildmech
        weaponScripts = bm.weaponScripts;
        cooldownRate = bm.MechProperty.CooldownRate;
        MaxHeat = bm.MechProperty.MaxHeat;
    }

    private void FixedUpdate() {
        for (int i = 0; i < 4; i++) {
            curValue[i] -= ((mcbt.is_overheat[i]) ? cooldownRate : cooldownRate * CooldownCoeffWhenNotOverHeat) * Time.fixedDeltaTime;// cooldown faster when overheat

            if (curValue[i] <= 0) {
                if (mcbt.is_overheat[i]) { // if previous is overheated => change color
                    if (i == weaponOffset) {
                        barL_fill.color = YELLOW;
                    } else if (i == weaponOffset + 1) {
                        barR_fill.color = YELLOW;
                    }
                }
                mcbt.is_overheat[i] = false;
                curValue[i] = 0;
            }
        }

        DrawBarL();
        DrawBarR();
    }

    private void UpdateHeatBar() {
        weaponOffset = mcbt.GetCurrentWeaponOffset();

        if (bm.weaponScripts[weaponOffset].twoHanded) {
            EnableHeatBar(weaponOffset, true);
            EnableHeatBar(weaponOffset + 1, false);
        } else {
            EnableHeatBar(weaponOffset, bm.weaponScripts[weaponOffset] != null);
            EnableHeatBar(weaponOffset + 1, bm.weaponScripts[weaponOffset + 1] != null);
        }

        if (mcbt.is_overheat[weaponOffset]) {//update color
            barL_fill.color = RED;
        } else {
            barL_fill.color = YELLOW;
        }

        if (mcbt.is_overheat[weaponOffset + 1]) {
            barR_fill.color = RED;
        } else {
            barR_fill.color = YELLOW;
        }
    }

    private void ResetHeatBar() {
        barL_fill.fillAmount = 0;
        barR_fill.fillAmount = 0;

        for (int i = 0; i < 4; i++) {
            mcbt.is_overheat[i] = false;
            curValue[i] = 0;
        }
    }

    public void IncreaseHeatBarL(float value) { //value : [0,100]
        curValue[weaponOffset] += value;
        if (curValue[weaponOffset] >= MaxHeat) {
            curValue[weaponOffset] = MaxHeat;
            mcbt.is_overheat[weaponOffset] = true;
            barL_fill.color = RED;
        }
    }

    public void IncreaseHeatBarR(float value) {
        curValue[weaponOffset + 1] += value;
        if (curValue[weaponOffset + 1] >= MaxHeat) {
            curValue[weaponOffset + 1] = MaxHeat;
            mcbt.is_overheat[weaponOffset + 1] = true;
            barR_fill.color = RED;
        }
    }

    private void EnableHeatBar(int hand, bool b) {
        if (hand == 0) {
            barL.enabled = b;
            barL_fill.enabled = b;
        } else {
            barR.enabled = b;
            barR_fill.enabled = b;
        }
    }

    private void DrawBarL() {
        barL_fill.fillAmount = curValue[weaponOffset] / MaxHeat;
    }

    private void DrawBarR() {
        barR_fill.fillAmount = curValue[weaponOffset + 1] / MaxHeat;
    }
}