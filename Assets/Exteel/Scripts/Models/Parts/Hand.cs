using UnityEngine;

[CreateAssetMenu(menuName = "Part/Hand")]
class Hand : Part {
    [Header("Part Special")]
    public int MaxHeat;
	public int CooldownRate;
	public int Marksmanship;//set how well mech uses weapons
}
