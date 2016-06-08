using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NameTags : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        List<GameObject> PlayerNames = gm.playerInfo.Keys.ToList();
        List<GameObject> aList = new List<GameObject> ();
	    for (int i = 0; i < PlayerNames.Count; i++)
        {
            aList.Add(GetComponent<GUIText>().text = PlayerNames[i].name);
        }
        return aList;
        Debug.Log(PlayerNames);
	}
 
}

