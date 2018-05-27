using UnityEngine;

public class SkillCam : MonoBehaviour {

    [SerializeField]private Transform player;
    private Transform target;
    private Vector3 idealPosition;
    private float skill_length, startTime, curTime;
    public float Cam_Distance_To_Mech = 15, lerpSpeed = 5;
    public Vector3 height = new Vector3(0, 10, 0);

    public void SetTarget(Transform target) {
        this.target = target;
    }

    public void SetSkillCameraDuration(float skill_length) {
        this.skill_length = skill_length;
    }

    // Update is called once per frame
    private void Update () {
        curTime = Time.time - startTime;

        //lerp pos
        idealPosition = player.position + height + (player.position - target.position).normalized * Cam_Distance_To_Mech;
        transform.position = Vector3.Lerp(transform.position, idealPosition, Time.deltaTime * lerpSpeed);

        transform.LookAt((target.position + player.position)/2 + new Vector3(0,5,0));
        //transform.LookAt(player.position + new Vector3(0,5,0));
    }

    private void OnEnable() {
        transform.localPosition = Vector3.zero;
        InitTime();
        enabled = true;

        transform.SetParent(null);//skill cam move independently
    }

    private void InitTime() {
        startTime = Time.time;
        curTime = 0;
    }

    private void OnDisable() {
        transform.SetParent(player);
        enabled = false;
    }

    private void SetParent(Transform parent) {
        transform.SetParent(null);
    }
}
