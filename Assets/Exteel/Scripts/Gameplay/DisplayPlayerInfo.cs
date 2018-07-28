using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayPlayerInfo : MonoBehaviour {
    private TextMesh textMesh;
    private MeshRenderer textMeshRenderer;
    private Image bar;
    private Combat Combat;
    private Camera cam;
    private GameObject targetPlayer;
    private string thisPlayerName; 

    private void Awake() {
        InitComponents();
        RegisterOnMechEnabled();
    }

    private void InitComponents() {
        Combat = transform.root.GetComponent<Combat>();
        textMesh = GetComponentInChildren<TextMesh>();
        textMeshRenderer = textMesh.GetComponent<MeshRenderer>();
        bar = FindBar();
    }

    private void RegisterOnMechEnabled() {
        Combat.OnMechEnabled += OnMechEnabled;
    }

    private void OnMechEnabled(bool b) {
        textMeshRenderer.enabled = b;

        Image[] child_images = GetComponentsInChildren<Image>();
        foreach(Image i in child_images) {
            i.enabled = b;
        }
    }

    private void Start() {
        PhotonView pv = Combat.photonView;

        //Do not show my name & hp bar
        gameObject.SetActive(!pv.isMine || tag=="Drone");
        
        if(pv.isMine)return;

        //Init name
        thisPlayerName = (tag == "Drone") ? "Drone" + Random.Range(0, 999) : pv.owner.NickName;
        textMesh.text = thisPlayerName;

        if (tag != "Drone" && GameManager.isTeamMode && PhotonNetwork.player.GetTeam() == pv.owner.GetTeam()) {
            bar.color = Color.white;
            textMesh.color = Color.white;
        } else {
            bar.color = Color.red; //enemy
            textMesh.color = Color.red;
        }

        GameManager gm = FindObjectOfType<GameManager>();
        StartCoroutine(GetThePlayer(gm));
    }

    private IEnumerator GetThePlayer(GameManager gm) {
        GameObject ThePlayer;
        int request_times = 0;
        while ((ThePlayer = gm.GetThePlayer()) == null && request_times < 10) {
            request_times++;
            yield return new WaitForSeconds(0.5f);
        }

        if(request_times >= 10) {
            Debug.LogError("Can't get the player");
            yield break;
        }

        InitPlayerRelatedComponents(ThePlayer);
        yield break;
    }

    private void InitPlayerRelatedComponents(GameObject player) {
        cam = player.GetComponentInChildren<Camera>();
    }

    private Image FindBar() {
        Image[] child_images = GetComponentsInChildren<Image>();
        foreach(Image i in child_images) {
            if(i.type == Image.Type.Filled) {
                return i;
            }
        }
        Debug.LogError("Can't find hp bar");
        return null;
    }

    private void Update() {
        if (cam != null) {
            transform.LookAt(cam.transform);
            Vector3 angle = transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(0, angle.y, angle.z);

            //update scale
            float distance = Vector3.Distance(transform.position, cam.transform.position);
            distance = Mathf.Clamp(distance, 0, 200f);
            transform.localScale = new Vector3(1 + distance / 100 * 1.5f, 1 + distance / 100 * 1.5f, 1);
        }

        //update bar value
        bar.fillAmount = (float)Combat.CurrentHP / Combat.MAX_HP;
    }
}