using UnityEngine;

public abstract class Part : ScriptableObject {
    [Header("Part General")]
    public GameObject part;
    public int Weight;
	public int HP = 0;
	public int EnergyDrain = 0;
	public int Size = 0;
}
