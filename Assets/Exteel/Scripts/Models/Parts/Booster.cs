using UnityEngine;

[CreateAssetMenu(menuName = "Part/Booster")]
public class Booster : Part {
    [Header("Part Special")]
    public int DashOutput;
	public int DashENDrain;
	public int JumpENDrain;
    public int VerticalBoostSpeed = 35;

    [SerializeField]private AnimationClip open, close;

    public override void LoadPartInfo(ref MechProperty mechProperty) {
        LoadPartBasicInfo(ref mechProperty);
        mechProperty.DashOutput = DashOutput;
        mechProperty.DashENDrain += DashENDrain;
        mechProperty.JumpENDrain = JumpENDrain;
        mechProperty.VerticalBoostSpeed = VerticalBoostSpeed;
    }

    public AnimationClip GetOpenAnimation() {
        return open;
    }

    public AnimationClip GetCloseAnimation() {
        return close;
    }
}
