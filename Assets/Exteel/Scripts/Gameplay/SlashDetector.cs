using System.Collections.Generic;
using UnityEngine;

public class SlashDetector : MonoBehaviour {
    [SerializeField] private MechCamera cam;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private MechController mctrl;
    [SerializeField] private GameObject User;
    private List<Transform> Target = new List<Transform>();
    private float clamped_cam_angle_x;
    private float clampAngle = 75;
    private float mech_Midpoint = 5;
    private float clamp_angle_coeff = 0.3f;//how much the cam angle affecting the y pos of box collider
    private float inair_start_z = 14f;
    private Vector3 inair_c = new Vector3(0, 0, 3.6f), inair_s = new Vector3(10, 18, 36);
    private Vector3 onground_c = new Vector3(0, 0, 2.5f), onground_s = new Vector3(10, 11, 15);
    private bool on_original_place = false;

    private void Update() {
        if (!mctrl.Grounded) {
            on_original_place = false;
            clamped_cam_angle_x = Mathf.Clamp(cam.GetCamAngle(), -clampAngle, clampAngle);
            transform.parent.localPosition = new Vector3(transform.parent.localPosition.x, mech_Midpoint, transform.parent.localPosition.z);

            //set collider size
            SetCenter(new Vector3(inair_c.x, inair_c.y, inair_start_z));
            SetSize(inair_s);
            SetlocalRotation(new Vector3(-clamped_cam_angle_x, transform.parent.localRotation.eulerAngles.y, transform.parent.localRotation.eulerAngles.z));
        } else {
            if (!on_original_place) {
                on_original_place = true;
                transform.parent.localPosition = new Vector3(transform.parent.localPosition.x, mech_Midpoint, transform.parent.localPosition.z);

                SetCenter(onground_c);
                SetSize(onground_s);
                SetlocalRotation(Vector3.zero);
            }
        }
    }

    private void OnTriggerEnter(Collider target) {
        if (target.gameObject != User && target.tag[0] != 'S') {//in player layer but not shield => player
            if (GameManager.IsTeamMode) {
                PhotonView pv = target.GetComponent<PhotonView>();
                if (pv.owner.GetTeam() == PhotonNetwork.player.GetTeam() && pv.owner != PhotonNetwork.player) {return; }
            } 
            Target.Add(target.transform);
        }
    }

    private void OnTriggerExit(Collider target) {
        if (target.gameObject != User && target.tag[0] != 'S') {
            if (GameManager.IsTeamMode) {
                PhotonView pv = target.GetComponent<PhotonView>();
                if (pv.owner.GetTeam() == PhotonNetwork.player.GetTeam() && pv.owner != PhotonNetwork.player)
                    return;
            }
            Target.Remove(target.transform);
        }
    }

    public void SetlocalRotation(Vector3 v) {
        transform.parent.localRotation = Quaternion.Euler(v);
    }

    private void SetCenter(Vector3 v) {
        boxCollider.center = v;
    }

    private void SetSize(Vector3 v) {
        boxCollider.size = v;
    }

    public List<Transform> getCurrentTargets() {
        return Target;
    }

    public void EnableDetector(bool b) {
        boxCollider.enabled = b;
        enabled = b;

        Target.Clear();
    }
}