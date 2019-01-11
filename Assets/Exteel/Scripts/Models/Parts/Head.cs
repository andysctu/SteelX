using UnityEngine;

[CreateAssetMenu(menuName = "Part/Head")]
public class Head : Part {
    [Header("Part Special")]
    public int SP;
	public int MPU;
	public int ScanRange;

    public override void LoadPartInfo(ref MechProperty mechProperty) {
        LoadPartBasicInfo(ref mechProperty);
        mechProperty.SP += SP;
        mechProperty.MPU += MPU;
        mechProperty.ScanRange += ScanRange;
    }
}
