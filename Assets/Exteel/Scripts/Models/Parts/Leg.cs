using UnityEngine;

[CreateAssetMenu(menuName = "Part/Leg")]
class Leg : Part {
    [Header("Part Special")]
    public int BasicSpeed;
	public int Capacity;
	public int Deceleration;

    public AudioClip WalkSound;
}
