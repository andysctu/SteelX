﻿using UnityEngine;

[CreateAssetMenu(menuName = "Part/Leg")]
public class Leg : Part {
    [Header("Part Special")]
    public int BasicSpeed;
	public int Capacity;
	public int Deceleration;    
    public enum LegType {
        Light,
        Standard,
        Heavy
    }
    public LegType legType;

    public override void LoadPartInfo(MechProperty mechProperty) {
        LoadPartBasicInfo(mechProperty);
        mechProperty.BasicSpeed += BasicSpeed;
        mechProperty.Capacity += Capacity;
        mechProperty.Deceleration += Deceleration;
    }
}
