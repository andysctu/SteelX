using UnityEngine;

[CreateAssetMenu(menuName = "Part/Booster")]
class Booster : Part {
    [Header("Part Special")]
    public int DashOutput;
	public int DashENDrain;
	public int JumpENDrain;
}
