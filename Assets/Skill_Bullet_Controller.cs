using System.Collections;
using UnityEngine;

public class Skill_Bullet_Controller : MonoBehaviour, RequireSkillInfo {
    //this script controll all the bullets in skill , by the same method in MechCombat
    [SerializeField] private int bullet_num = 0;
    [SerializeField] private float interval = 1;
    [SerializeField] private bool onTarget = false;//does bullets follow the target? //TODO : implement this

    private Transform target;
    private MechCombat mechCombat;

    private int hand = 0, weaponOffset = 0;

    // Use this for initialization
    void Start () {
        mechCombat = transform.root.GetComponent<MechCombat>();
	}

    private void OnEnable() {
        if(mechCombat==null) mechCombat = transform.root.GetComponent<MechCombat>();
        mechCombat.SetTargetInfo(hand, target);

        StartCoroutine(IntantiateBullets());
    }

    IEnumerator IntantiateBullets() {
        for(int i = 0; i < bullet_num; i++) {
            mechCombat.InstantiateBulletTrace(hand);
            yield return new WaitForSeconds(interval);
        }
    }

    public void SetCurWeaponOffset(int weaponOffset) {
        this.weaponOffset = weaponOffset;
    }

    public void SetHand(int hand) {
        this.hand = hand;
    }

    //this is called when casting skill
    public void SetTarget(Transform target) {
        Debug.Log("called set target with : "+gameObject.name);
        this.target = target;
    }
}

public interface RequireSkillInfo {
    void SetCurWeaponOffset(int weaponOffset);
    void SetHand(int hand);
    void SetTarget(Transform target);
}
