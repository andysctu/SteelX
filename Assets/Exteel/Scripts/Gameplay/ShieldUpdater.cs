using UnityEngine;

public class ShieldUpdater : MonoBehaviour {
    private CharacterController cc;
    private AnimatorVars AnimatorVars;
    private Animator Animator;
	private GameObject boxcollider;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0);
    private float rotOffset = 10;//make
    private int hand;//which hand holds this?  ;  set in buildMech

    void Start(){
        if ((cc =transform.root.GetComponent<CharacterController>())!=null &&  !cc.enabled)
            enabled = false;
        InitComponents();
	}

    void InitComponents() {
        Transform CurrentMech = transform.root.Find("CurrentMech");

        boxcollider = GetComponentInChildren<BoxCollider>().gameObject;
        if (boxcollider == null) Debug.LogError("Can't find shield collider as a child of Shield Mesh");

        if (CurrentMech == null) {
            enabled = false;
        } else {
            Animator = CurrentMech.GetComponent<Animator>();
            AnimatorVars = CurrentMech.GetComponent<AnimatorVars>();
        }
    } 

    public void SetHand(int hand) {
        this.hand = hand;
    }

    public int GetHand() {
        return hand;
    }

	void LateUpdate () {
        if (Animator.GetBool((hand == 0) ? AnimatorVars.blockL_id : AnimatorVars.blockR_id)) {
            boxcollider.transform.LookAt(transform.root.position + MECH_MID_POINT);
            boxcollider.transform.rotation = Quaternion.Euler(new Vector3(0, boxcollider.transform.rotation.eulerAngles.y + ((hand == 0) ? rotOffset : -rotOffset), 0));
        } else {
            boxcollider.transform.localRotation = Quaternion.Euler(new Vector3(0, ((hand == 0) ? 90 : -90), 0));
            boxcollider.transform.rotation = Quaternion.Euler(new Vector3(0, boxcollider.transform.rotation.eulerAngles.y, 0));
        }
	}
}
