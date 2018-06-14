using UnityEngine;

public abstract class Part : ScriptableObject {
    [Header("Part General")]
    [SerializeField]private GameObject part;

    [Tooltip("This name will be display in hangar")]
    public string displayName;
    public int Weight;
	public int HP = 0;
	public int EnergyDrain = 0;
	public int Size = 0;

    public void LoadPartBasicInfo(MechProperty mechProperty) {
        mechProperty.HP += HP;
        mechProperty.Size += Size;
        mechProperty.Weight += Weight;
        mechProperty.EnergyDrain += EnergyDrain;
    }

    public abstract void LoadPartInfo(MechProperty mechProperty);

    public GameObject GetPartPrefab() {
        return part;
    }
}
