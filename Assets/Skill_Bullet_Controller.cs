using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bullet_Controller : MonoBehaviour {

    private BuildMech bm;
    private SkillController SkillController;
    private Camera playerCam;
    private MechCamera mechCamera;
    private Transform target;
    private MechCombat mechCombat;

    private int hand = 0;


    // Use this for initialization
    void Start () {
        //Find the bullet prefabs

        //Adjust the bullet prefabs

        //Get Camera

        //

        SkillController = transform.root.GetComponent<SkillController>();
        target = transform.root.GetComponent<SingleTargetSkillBehaviour>().GetCurrentOnSkillTarget();
        //hand = (transform.parent.parent.name[transform.parent.parent.name.Length] == 'R') ? 1 : 0;
	}

    private void OnEnable() {
        
    }

    // Update is called once per frame
    void Update () {
		
	}
}
