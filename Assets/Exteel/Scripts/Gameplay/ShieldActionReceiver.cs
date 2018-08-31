using UnityEngine;

public class ShieldActionReceiver : MonoBehaviour { 
    private Animator Animator;
    private GameObject boxcollider;
    private Vector3 MECH_MID_POINT = new Vector3(0, 5, 0);
    private float rotOffset = 10;//make
    private int hand, pos;//which hand holds this?
    private int block_id = 0;

    private void Start() {
        InitComponents();
    }

    private void InitComponents() {//TODO : improve this
        Transform CurrentMech = transform.root.Find("CurrentMech");

        boxcollider = GetComponentInChildren<BoxCollider>().gameObject;
        if (boxcollider == null) Debug.LogError("Can't find shield collider as a child of Shield Mesh");

        if (CurrentMech == null) {
            enabled = false;
        } else {
            Animator = CurrentMech.GetComponent<Animator>();            
        }
    }

    public void SetPos(int pos) {
        this.hand = pos%2;
        this.pos = pos;

        block_id = Animator.StringToHash((hand==0)? "BlockL" : "BlockR");
    }

    public int GetPos() {
        return pos;
    }

    private void LateUpdate() {
        if(Animator != null && block_id != 0)
            if (Animator.GetBool(block_id)) {
                boxcollider.transform.LookAt(transform.root.position + MECH_MID_POINT);
                boxcollider.transform.rotation = Quaternion.Euler(new Vector3(0, boxcollider.transform.rotation.eulerAngles.y + ((hand == 0) ? rotOffset : -rotOffset), 0));
            } else {
                boxcollider.transform.localRotation = Quaternion.Euler(new Vector3(0, ((hand == 0) ? 90 : -90), 0));
                boxcollider.transform.rotation = Quaternion.Euler(new Vector3(0, boxcollider.transform.rotation.eulerAngles.y, 0));
            }
    }
}