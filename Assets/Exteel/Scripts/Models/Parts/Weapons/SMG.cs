using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/SMG", order = 3)]
public class SMG : RangedWeapon {
    SMG() {
        slowDown = false;
        twoHanded = false;
    }
}