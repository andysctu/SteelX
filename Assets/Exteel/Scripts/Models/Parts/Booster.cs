using UnityEngine;

[CreateAssetMenu(menuName = "Part/Booster")]
public class Booster : Part {
    [Header("Part Special")]
    public int DashOutput;
	public int DashENDrain;
	public int JumpENDrain;

    [SerializeField]private AnimationClip open, close;

    public override void LoadPartInfo(MechProperty mechProperty) {
        LoadPartBasicInfo(mechProperty);
        mechProperty.DashOutput += DashOutput;
        mechProperty.DashENDrain += DashENDrain;
        mechProperty.JumpENDrain += JumpENDrain;
    }

    public AnimationClip GetOpenAnimation() {
        return open;
    }

    public AnimationClip GetCloseAnimation() {
        return close;
    }
}
