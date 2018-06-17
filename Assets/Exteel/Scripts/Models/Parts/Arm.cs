using UnityEngine;

[CreateAssetMenu(menuName = "Part/Arm")]
public class Arm : Part {
    [Header("Part Special")]
    public int MaxHeat;
	public int CooldownRate;
	public int Marksmanship;//set how well mech uses weapons
    public Vector3 attachWeaponOffset;

    public override void LoadPartInfo(ref MechProperty mechProperty) {
        LoadPartBasicInfo(ref mechProperty);
        mechProperty.MaxHeat += MaxHeat;
        mechProperty.CooldownRate += CooldownRate;
        mechProperty.Marksmanship += Marksmanship;
    }
}
