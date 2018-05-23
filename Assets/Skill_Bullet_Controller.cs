using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bullet_Controller : MonoBehaviour, RequireSkillInfo {

    private BuildMech bm;
    private GameObject bulletPrefab;
    private SkillController SkillController;
    private Camera playerCam;
    private MechCamera mechCamera;
    private Transform target;
    private MechCombat mechCombat;

    private int hand = 0, weaponOffset = 0;


    // Use this for initialization
    void Start () {
        //bm = transform.root.GetComponent<BuildMech>();
        mechCombat = transform.root.GetComponent<MechCombat>();
        //Find the bullet prefabs
        //bulletPrefab = ((RangedWeapon)bm.weaponScripts[weaponOffset + hand]).bulletPrefab;



        //Get Camera

        //

        SkillController = transform.root.GetComponent<SkillController>();
        target = transform.root.GetComponent<SingleTargetSkillBehaviour>().GetCurrentOnSkillTarget();
        //hand = (transform.parent.parent.name[transform.parent.parent.name.Length] == 'R') ? 1 : 0;
	}

    

    private void OnEnable() {
        if(mechCombat==null) mechCombat = transform.root.GetComponent<MechCombat>();
        mechCombat.SetTargetInfo(hand, target);

        StartCoroutine(IntantiateBullets());
    }

    IEnumerator IntantiateBullets() {
        for(int i = 0; i < 6; i++) {
            mechCombat.InstantiateBulletTrace(hand);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetCurWeaponOffset(int weaponOffset) {
        this.weaponOffset = weaponOffset;
    }

    public void SetHand(int hand) {
        this.hand = hand;
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }
}

public interface RequireSkillInfo {
    void SetCurWeaponOffset(int weaponOffset);
    void SetHand(int hand);
    void SetTarget(Transform target);
}
