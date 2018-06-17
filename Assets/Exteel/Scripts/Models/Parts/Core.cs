using UnityEngine;

[CreateAssetMenu(menuName = "Part/Core")]
public class Core : Part {
    [Header("Part Special")]
    public int EN;
	public int ENOutputRate;
	public int MinENRequired;

    public override void LoadPartInfo(ref MechProperty mechProperty) {
        LoadPartBasicInfo(ref mechProperty);
        mechProperty.EN += EN;
        mechProperty.ENOutputRate += ENOutputRate;
        mechProperty.MinENRequired += MinENRequired;        
    }
}
