using System.Collections.Generic;
using UnityEngine;

public class NameTags : MonoBehaviour {
    //private GameManager gm;
    //private GameObject drone;
    //public Camera cam;

    //private void Start() {
    //    // Try to find the drone
    //    drone = GameObject.Find("Drone");
    //    if (drone == null) {
    //        // if we can't find it, it probably did not load yet, we will find it later
    //        Debug.Log("Drone is null");
    //    }
    //    gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    //    if (gm == null) {
    //        Debug.Log("No Players");
    //    }
    //    //        cam = GameObject.Find("Camera").GetComponent<Camera>();
    //}

    //// OnGUI runs every frame, like Update, but just for GUI stuff like labels and scoreboard, etc
    //private void OnGUI() {
    //    if (drone == null || cam == null || gm == null) {
    //        Debug.Log(gameObject.name);
    //        drone = GameObject.Find("Drone");
    //        //			cam = GameObject.Find(gameObject.name + "/Camera").GetComponent<Camera>();
    //        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    //    } else {
    //        // Once we've found it, transform the drone's 3D position to a 2D position corresponding to its position on your 2D screen
    //        Vector3 pos = cam.WorldToScreenPoint(drone.transform.position + new Vector3(0, 10));

    //        // Draw its name there
    //        GUI.Label(new Rect(pos.x, Screen.height - pos.y, 100, 100), "Drone");

    //        Dictionary<string, Score>.KeyCollection playerlist = (gm.playerScores).Keys;
    //        foreach (string name in playerlist) {
    //            GameObject player = GameObject.Find(name);
    //            if (player == null) {
    //                Debug.Log("player is null");
    //                continue;
    //            }
    //            Vector3 posi = cam.WorldToScreenPoint(player.transform.position + new Vector3(0, 10));
    //            GUI.Label(new Rect(posi.x, Screen.height - posi.y, 100, 100), name);
    //        }
    //    }
    //}
}