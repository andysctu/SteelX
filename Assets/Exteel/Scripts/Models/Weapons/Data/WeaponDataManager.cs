using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu]
public class WeaponDataManager : ScriptableObject {

    public SwordData[] Swords;
    public SpearData[] Spears;
    public ShieldData[] Shields;
    public SMGData[] SMGs;
    public RifleData[] Rifles;
    public ShotgunData[] Shotguns;
    public RectifierData[] Rectifiers;
    public RocketData[] Rockets;
    public CannonData[] Cannons;
    
    public WeaponData FindData(string name) {
        if (name.Contains("APS") || name.Contains("LMG")) {
            return SearchInArray(SMGs, name);
        } else if (name.Contains("RF")) {
            return SearchInArray(Rifles, name);
        } else if (name.Contains("SGN")) {
            return SearchInArray(Shotguns, name);
        } else if (name.Contains("BCN") || name.Contains("MSR")) {
            return SearchInArray(Cannons, name);
        } else if (name.Contains("ENG")) {
            return SearchInArray(Rectifiers, name);
        } else if (name.Contains("RCL")) {
            return SearchInArray(Rockets, name);
        } else if (name.Contains("DR")) {
            return SearchInArray(Spears, name);
        } else if (name.Contains("SHL") || name.Contains("LSN")) {
            return SearchInArray(Swords, name);
        } else if (name.Contains("SHS")) {
            return SearchInArray(Shields, name);
        } else {
            Debug.Log("Weapon name does not match any : "+name);
            return null;
        }
    }

    WeaponData SearchInArray(WeaponData[] weapons, string name) {
        for(int i = 0; i < weapons.Length; i++) {
            if (weapons[i].GetWeaponPrefab(0).name == name || (weapons[i].GetWeaponPrefab(1) != null && weapons[i].GetWeaponPrefab(1).name == name)) {
                return weapons[i];
            }
        }
        Debug.LogError("Can't find prefab : " + name);
        return null;
    }

    public WeaponData[] GetAllWeaponss() {
        int length = Swords.Length + Spears.Length + Shields.Length+ SMGs.Length + Rifles.Length + Shotguns.Length + Rectifiers.Length + Rockets.Length + Cannons.Length;
        WeaponData[] weapons = new WeaponData[length];

        List<WeaponData[]> weapon_list = new List<WeaponData[]>();
        weapon_list.Add(Swords);
        weapon_list.Add(Spears);
        weapon_list.Add(Shields);
        weapon_list.Add(SMGs);
        weapon_list.Add(Rifles);
        weapon_list.Add(Shotguns);
        weapon_list.Add(Rectifiers);
        weapon_list.Add(Rockets);
        weapon_list.Add(Cannons);

        int i = 0;
        foreach(WeaponData[] w in weapon_list) {
            foreach(WeaponData w2 in w) {
                weapons[i++] = w2;
            }
        }

        return weapons;
    }
}

