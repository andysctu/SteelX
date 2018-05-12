using UnityEngine;

[CreateAssetMenu]
public class WeaponManager : ScriptableObject {

    public Sword[] Swords;
    public Spear[] Spears;
    public Shield[] Shields;
    public SMG[] SMGs;
    public Rifle[] Rifles;
    public Shotgun[] Shotguns;
    public Rectifier[] Rectifiers;
    public Rocket[] Rockets;
    public Cannon[] Cannons;
    
    public Weapon FindData(string name) {

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

    Weapon SearchInArray(Weapon[] weapons, string name) {
        for(int i = 0; i < weapons.Length; i++) {
            if(weapons[i].weaponPrefab.name == name) {
                return weapons[i];
            }
        }
        Debug.LogError("Can't find prefab : " + name);
        return null;
    }
}

