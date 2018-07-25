using UnityEngine;

public class DMManager : ScriptableObject {
    //[MenuItem("Tools/MyTool/Do It in C#")]
    //static void DoIt() {
    //    EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
    //}
}

//TODO : remove this
//public int MaxKills = 2, CurrentMaxKills = 0;

//protected virtual void LoadGameInfo() {
//    GameInfo.Map = PhotonNetwork.room.CustomProperties["Map"].ToString();
//    GameInfo.GameMode = PhotonNetwork.room.CustomProperties["GameMode"].ToString();
//    GameInfo.MaxTime = int.Parse(PhotonNetwork.room.CustomProperties["MaxTime"].ToString());
//    GameInfo.MaxPlayers = PhotonNetwork.room.MaxPlayers;

//    //GameInfo.MaxKills = int.Parse(PhotonNetwork.room.CustomProperties["MaxKills"].ToString());
//    Debug.Log("Map : " + GameInfo.Map + "Gamemode :" + GameInfo.GameMode);
//}

//MaxKills = GameInfo.MaxKills; //in start