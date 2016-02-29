using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine.Networking;

public class MechCreator
{
	#region Fields
	private List<GameObject> parts;
	private GameObject core;
	private string coreName;
	private Stitcher stitcher;
	private RuntimeAnimatorController animator;
	private GameObject mechCam, radar;
	#endregion

	//testing
	private string defaultCore = "CES301";
	private string[] defaultParts = {"AES104","LTN411","HDS003"};

	public MechCreator(string c, List<string> p) {
		coreName = c;
		core = Resources.Load(c, typeof(GameObject)) as GameObject;
		if (core == null ) {
			Debug.Log("null core, using default");
			core = Resources.Load(defaultCore, typeof(GameObject)) as GameObject;
			coreName = defaultCore;
		}
		parts = new List<GameObject>();

		if (p == null || p.Capacity == 0) {
			Debug.Log("null parts, using default");
			for (int i = 0; i < defaultParts.Length; i++) {
				GameObject part = Resources.Load(defaultParts[i], typeof(GameObject)) as GameObject;
				parts.Add(part);
			}
		} else {
			for (int i = 0; i < p.Count; i++) {
				GameObject part = Resources.Load(p[i], typeof(GameObject)) as GameObject;
				if (part == null ) part = Resources.Load(defaultParts[i], typeof(GameObject)) as GameObject;
				parts.Add(part);
			}
		}

		animator = Resources.Load("ThirdAnimator", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController; if (animator == null )Debug.Log("animator");
		mechCam = Resources.Load("MechCam", typeof(GameObject)) as GameObject; if (mechCam == null )Debug.Log("MechCam");
		radar = Resources.Load("Radar", typeof(GameObject)) as GameObject; if (radar == null )Debug.Log("Radar");
		stitcher = new Stitcher();
	}

	public GameObject CreateLobbyMech ()
	{
		core = (GameObject) GameObject.Instantiate(core);

		Animator a = core.GetComponent<Animator>();
		a.runtimeAnimatorController = animator;
		a.applyRootMotion = false;
		foreach (var part in parts) {
			Wear(part);
		}
		core.layer = 8;
		core.tag = "Player";

		SkinnedMeshRenderer[] smr = core.GetComponentsInChildren<SkinnedMeshRenderer>();
		Material coreMat = Resources.Load(coreName+"mat", typeof(Material)) as Material;
		for (int i = 0; i < smr.Length; i++){
			string name = smr[i].name;
			Debug.Log("name: " + name);
			if (name.IndexOf("(Clone)") == -1) {
				smr[i].material = coreMat;
				smr[i].enabled = true;
				continue;
			}

			name = name.Substring(0,name.IndexOf("(Clone)")) + "mat";

			Material newMat = Resources.Load(name, typeof(Material)) as Material;
			smr[i].material = newMat;
			smr[i].enabled = true;
		}
		#if UNITY_EDITOR
		PrefabUtility.CreatePrefab("Assets/Exteel/Prefabs/CurrentMech.prefab", core);
		#endif

		return core;
	}

	public GameObject CreatePlayerMech() {
		core = (GameObject) GameObject.Instantiate(core);

		Animator a = core.GetComponent<Animator>();
		a.runtimeAnimatorController = animator;
		a.applyRootMotion = false;
		foreach (var part in parts) {
			Wear(part);
		}
		core.layer = 8;
		core.tag = "Player";

		SkinnedMeshRenderer[] smr = core.GetComponentsInChildren<SkinnedMeshRenderer>();

		for (int i = 0; i < smr.Length; i++){
			string name = smr[i].name;
			if (name.IndexOf("(Clone)") == -1) {
				continue;
			}

			name = name.Substring(0,name.IndexOf("(Clone)")) + "mat";

			Material newMat = Resources.Load(name, typeof(Material)) as Material;
			smr[i].material = newMat;
			smr[i].enabled = true;
		}

		GameObject wrapper = new GameObject();
		core.transform.parent = wrapper.transform;

		// Attach scripts
		wrapper.AddComponent<CharacterController>();
		CharacterController cc = wrapper.GetComponent<CharacterController>();
		cc.center = new Vector3(0,4);
		cc.radius = 2;
		cc.height = 8;

		wrapper.AddComponent<NewMechController>().enabled = false;
		wrapper.AddComponent<NetworkIdentity>().localPlayerAuthority = true;
		wrapper.AddComponent<EnableLocal>();
		NetworkTransform nt = wrapper.AddComponent<NetworkTransform>();
		nt.sendInterval = 1/20f;
		nt.transformSyncMode = NetworkTransform.TransformSyncMode.SyncCharacterController;
		nt.movementTheshold = 0.001f;
		nt.snapThreshold = 50f;
		nt.interpolateMovement = 1f;
		nt.syncRotationAxis = NetworkTransform.AxisSyncMode.AxisY;
		nt.interpolateRotation = 10f;
		nt.rotationSyncCompression = NetworkTransform.CompressionSyncMode.None;

//		wrapper.AddComponent<PlayerID>();
//		wrapper.AddComponent<PlayerShoot>();
//		PlayerHealth ph = wrapper.AddComponent<PlayerHealth>();
//		ph.health = ph.MaxHealth;
//
//		wrapper.AddComponent<PlayerDeath>();
//		wrapper.AddComponent<PlayerRespawn>();
//		wrapper.AddComponent<MechAnimation>().error = 0.1f;
//		NetworkAnimator na = wrapper.AddComponent<NetworkAnimator>();
//		na.animator = a;
//		for (int i = 0; i < 7; i++){
//			na.SetParameterAutoSend(i, true);
//		}
			
		// Add child objects
		GameObject mc = GameObject.Instantiate(mechCam);
		mc.transform.parent = wrapper.transform;
//		wrapper.GetComponent<PlayerShoot>().camTransform = mc.transform;

		GameObject r = GameObject.Instantiate(radar);
		r.SetActive(true);
		r.transform.parent = wrapper.transform;

		return wrapper;
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
