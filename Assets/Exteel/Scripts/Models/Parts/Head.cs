using UnityEngine;

[CreateAssetMenu(menuName = "Part/Head")]
class Head : Part {
    [Header("Part Special")]
    public int SP;
	public int MPU;
	public int ScanRange;
}
