using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Rocket", order = 5)]
public class Rocket : RangedWeapon {
    Rocket() {
        weaponType = "Rocket";
        slowDown = true;
        twoHanded = true;
    }
}