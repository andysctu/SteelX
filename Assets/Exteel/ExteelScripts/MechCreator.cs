using UnityEngine;
using System.Collections.Generic;

public class MechCreator : MonoBehaviour
{
	#region Fields
	public List<GameObject> parts;
	public GameObject core;
	private Stitcher stitcher;
	public RuntimeAnimatorController animator;
	#endregion

	#region Monobehaviour
	public void Awake ()
	{
		stitcher = new Stitcher ();
		core = (GameObject) GameObject.Instantiate(core);
		core.AddComponent("Animator");
		core.GetComponent<Animator>() = animator;
		foreach (var part in parts) {
			Wear(part);
		}
	}

//	public void OnGUI ()
//	{
//		var offset = 0;
//		foreach (var part in parts) {
//			if (GUI.Button (new Rect (0, offset, buttonWidth, buttonHeight), part.name)) {
//				RemoveWorn ();
//				Wear (part);
//			}
//			offset += buttonHeight;
//		}
//	}
	#endregion

	private void RemoveWorn (GameObject worn)
	{
		if (worn == null)
			return;
		GameObject.Destroy (worn);
	}

	private void Wear (GameObject part)
	{
		if (part == null)
			return;
		part = (GameObject)GameObject.Instantiate (part);
		stitcher.Stitch (part, core);
		GameObject.Destroy (part);
	}
}
