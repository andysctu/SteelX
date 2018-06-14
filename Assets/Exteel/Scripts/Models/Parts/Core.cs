using UnityEngine;

[CreateAssetMenu(menuName = "Part/Core")]
class Core : Part {
    [Header("Part Special")]
    public int EN;
	public int ENOutputRate;
	public int MinENRequired;
}
