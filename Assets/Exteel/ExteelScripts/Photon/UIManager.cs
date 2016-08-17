using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	[SerializeField] GameObject CreateRoomModal;
	[SerializeField] GameObject RoomPanel;
	[SerializeField] Transform RoomsWrapper;

	private GameObject[] rooms;
	private float roomHeight = 50;

	// Use this for initialization
	void Start () {
//		Refresh ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnReceivedRoomListUpdate() {
		Debug.Log("Received: " + PhotonNetwork.GetRoomList().Length);
		Refresh();
	}

	public void Refresh() {
		if (rooms != null) {
			for (int i = 0; i < rooms.Length; i++) {
				Destroy (rooms [i]);
			}
		}

		RoomInfo[] roomsInfo = PhotonNetwork.GetRoomList ();
		Debug.Log (roomsInfo.Length);
		rooms = new GameObject[roomsInfo.Length];
		for (int i = 0; i < roomsInfo.Length; i++) {
			GameObject roomPanel = Instantiate (RoomPanel);
			roomPanel.transform.SetParent(RoomsWrapper);
			RectTransform rt = roomPanel.GetComponent<RectTransform> ();
			rt.localPosition = new Vector3(0, -1*roomHeight*i, 0);
			rt.localScale = new Vector3 (1, 1, 1);
			int index = i;
			roomPanel.GetComponent<Button> ().onClick.AddListener (() => {
				Debug.Log("Room selected");
			});
			rooms [i] = roomPanel;
		}
		RoomsWrapper.GetComponent<RectTransform> ().sizeDelta = new Vector2 (600, 50 * roomsInfo.Length);
	}

	public void ShowCreateRoomModal() {
		CreateRoomModal.SetActive (true);
	}
}
