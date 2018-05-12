using UnityEngine;

public class Weapon : ScriptableObject {
    [Tooltip("Special weapon types")]
    public string weaponType;//APS , LMG , Rocket , Cannon , Shotgun , ...
    public GameObject weaponPrefab;
    public GameObject[] Grip = new GameObject[2];//L&R , only set the rotation , the position is adjusted by hand offset
    public int damage = 0;

    [Tooltip("The range of ranged weapons")]
    [Range(0, 1200)]
    public int Range = 0;

    [Range(0, 5)]
    public float Rate = 0;

    [Tooltip("Crosshair size")]
    [Range(0, 15)]
    public float radius = 0;

    [Range(0,200)]
    public int heat_increase_amount;

    [Tooltip("Does this weapon slow down targets?")]
    public bool slowDown;
    public bool twoHanded;
}


