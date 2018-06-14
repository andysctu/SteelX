using UnityEngine;

[CreateAssetMenu(menuName = "Part/Booster")]
public class Booster : Part {
    [Header("Part Special")]
    public int DashOutput;
	public int DashENDrain;
	public int JumpENDrain;

    public override void LoadPartInfo(MechProperty mechProperty) {
        LoadPartBasicInfo(mechProperty);
        mechProperty.DashOutput += DashOutput;
        mechProperty.DashENDrain += DashENDrain;
        mechProperty.JumpENDrain += JumpENDrain;
    }
}
