using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour {
    [SerializeField] private Image barL, barR;
    [SerializeField] private Image barL_fill, barR_fill;
    [SerializeField] private BuildMech bm;
    [SerializeField] private MechCombat mcbt;
    [SerializeField] private PhotonView pv;//mech combat's pv

    private WeaponData[] weaponScripts;
    private float[] curValue = new float[4];
    private bool[] isOverHeat = new bool[4];
    private const float CooldownCoeffWhenNotOverHeat = 0.2f;
    private int cooldownRate, weaponOffset;
    private int MaxHeat;
    private Color32 RED = new Color32(255, 0, 0, 200), YELLOW = new Color32(255, 255, 85, 200);

    private void Awake() {
        RegisterOnMechBuilt();
        RegisterOnWeaponSwitched();
        RegisterOnMechEnabled();
    }

    private void RegisterOnMechBuilt() {
        bm.OnMechBuilt += InitVars;
        bm.OnMechBuilt += ResetHeatBar;
    }

    private void RegisterOnMechEnabled() {
        mcbt.OnMechEnabled += ActivateHeatBar;
    }

    private void RegisterOnWeaponSwitched() {
        mcbt.OnWeaponSwitched += UpdateHeatBar;
    }

    private void InitVars() {//called when finished buildmech
        weaponScripts = bm.WeaponDatas;
        cooldownRate = bm.MechProperty.CooldownRate;
        MaxHeat = bm.MechProperty.MaxHeat;
    }

    private void Start() {
        gameObject.SetActive(pv.isMine);
    }

    private void FixedUpdate() {
        for (int i = 0; i < 4; i++) {
            curValue[i] -= ((isOverHeat[i]) ? cooldownRate : cooldownRate * CooldownCoeffWhenNotOverHeat) * Time.fixedDeltaTime;// cooldown faster when overheat

            if (curValue[i] <= 0) {
                if (isOverHeat[i]) { // if previous is overheated => change color
                    if (i == weaponOffset) {
                        barL_fill.color = YELLOW;
                    } else if (i == weaponOffset + 1) {
                        barR_fill.color = YELLOW;
                    }
                }
                isOverHeat[i] = false;
                curValue[i] = 0;
            }
        }

        DrawBarL();
        DrawBarR();
    }

    private void UpdateHeatBar() {
        weaponOffset = mcbt.GetCurrentWeaponOffset();

        if (bm.WeaponDatas[weaponOffset].twoHanded) {
            EnableHeatBar(weaponOffset, true);
            EnableHeatBar(weaponOffset + 1, false);
        } else {
            EnableHeatBar(weaponOffset, bm.WeaponDatas[weaponOffset] != null);
            EnableHeatBar(weaponOffset + 1, bm.WeaponDatas[weaponOffset + 1] != null);
        }

        if (isOverHeat[weaponOffset]) {//update color
            barL_fill.color = RED;
        } else {
            barL_fill.color = YELLOW;
        }

        if (isOverHeat[weaponOffset + 1]) {
            barR_fill.color = RED;
        } else {
            barR_fill.color = YELLOW;
        }
    }

    private void ResetHeatBar() {
        barL_fill.fillAmount = 0;
        barR_fill.fillAmount = 0;

        for (int i = 0; i < 4; i++) {
            isOverHeat[i] = false;
            curValue[i] = 0;
        }
    }

    //TODO : check this
    public void IncreaseHeatBar(int pos, float value) { //value : [0,100]
        curValue[pos] += value;
        if (curValue[pos] >= MaxHeat) {
            curValue[pos] = MaxHeat;
            isOverHeat[pos] = true;
            if(pos % 2 == 0) {
                barL_fill.color = RED;
            } else {
                barR_fill.color = RED;
            }            
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

    private void ActivateHeatBar(bool b) {
        if(!pv.isMine)return;

        gameObject.SetActive(b);
    }

    private void DrawBarL() {
        barL_fill.fillAmount = curValue[weaponOffset] / MaxHeat;
    }

    private void DrawBarR() {
        barR_fill.fillAmount = curValue[weaponOffset + 1] / MaxHeat;
    }

    public bool IsOverHeat(int pos) {
        return isOverHeat[pos];
    }
}