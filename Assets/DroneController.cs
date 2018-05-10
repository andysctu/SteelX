using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour {

    [SerializeField]private int hor_boostingSpeed = 60, ver_boostingSpeed = 30;
    [SerializeField]private int Moving_mode = 0;


    public float Gravity = 4.5f;
    public float maxDownSpeed = -140f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
