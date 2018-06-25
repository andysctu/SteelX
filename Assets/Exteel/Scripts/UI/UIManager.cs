using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	[SerializeField] GameObject CreateRoomModal;
	[SerializeField] GameObject RoomPanel;
	[SerializeField] Transform RoomsWrapper;

	private GameObject[] rooms;
	private float roomHeight = 50;
	private string selectedRoom = "";
    private string[] SceneName = new string[5]{ "Login", "Lobby", "Hangar", "GameLobby", "Store"};

	void Start () {

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
		Debug.Log ("roomsInfo.length :"+roomsInfo.Length);
		rooms = new GameObject[roomsInfo.Length];
		for (int i = 0; i < roomsInfo.Length; i++) {
			GameObject roomPanel = Instantiate (RoomPanel);
			Text[] info = roomPanel.GetComponentsInChildren<Text> ();
			Debug.Log (roomsInfo [i].name);
			info [3].text = "Players: " + roomsInfo [i].playerCount + "/" + roomsInfo [i].MaxPlayers;
			info [2].text = "GameMode: " + roomsInfo [i].CustomProperties ["GameMode"];
			info [1].text = "Map: "+roomsInfo [i].CustomProperties ["Map"];
			info [0].text = "Room Name: " + roomsInfo [i].name;

			roomPanel.transform.SetParent(RoomsWrapper);
			RectTransform rt = roomPanel.GetComponent<RectTransform> ();
			rt.localPosition = new Vector3(0, -1*roomHeight*i, 0);
			rt.localScale = new Vector3 (1, 1, 1);
			int index = i;
			roomPanel.GetComponent<Button> ().onClick.AddListener (() => {
				selectedRoom = roomsInfo[index].name;
			});
			rooms [i] = roomPanel;
		}
		RoomsWrapper.GetComponent<RectTransform> ().sizeDelta = new Vector2 (600, 50 * roomsInfo.Length);
	}

	public void ShowCreateRoomModal() {
		CreateRoomModal.SetActive (true);
	}

	public void HideCreateRoomModel() {
		CreateRoomModal.SetActive(false);
	}

	public void JoinRoom() {
		if (!string.IsNullOrEmpty (selectedRoom)) {
			Debug.Log ("Joining Room " + selectedRoom);
			PhotonNetwork.JoinRoom (selectedRoom);
		}
	}

	public void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom");
		PhotonNetwork.LoadLevel ("GameLobby");
	}
}
