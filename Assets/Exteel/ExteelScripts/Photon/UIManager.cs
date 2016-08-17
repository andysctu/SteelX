using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {

	[SerializeField] GameObject CreateRoomModal;
	private bool showCreateRoom;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShowCreateRoomModal() {
		CreateRoomModal.SetActive (true);
	}
}
