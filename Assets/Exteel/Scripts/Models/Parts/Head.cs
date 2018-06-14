using UnityEngine;

[CreateAssetMenu(menuName = "Part/Head")]
public class Head : Part {
    [Header("Part Special")]
    public int SP;
	public int MPU;
	public int ScanRange;

    public override void LoadPartInfo(MechProperty mechProperty) {
        LoadPartBasicInfo(mechProperty);
        mechProperty.SP += SP;
        mechProperty.MPU += MPU;
        mechProperty.ScanRange += ScanRange;
    }
}
