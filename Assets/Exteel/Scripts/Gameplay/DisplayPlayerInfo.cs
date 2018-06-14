using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPlayerInfo : MonoBehaviour {
	[SerializeField]private TextMesh textMesh;
	[SerializeField]private Canvas barcanvas;
	[SerializeField]private Image bar;

    private MechCombat mcbt;
    private DroneCombat drone_mcbt;
    private Camera cam;
    private GameObject player;
	private string name_text ; //name
	private float LastInitRequestTime;
	private Color32 color_ally = new Color32 (223, 234, 11, 255);
    private delegate int GetCurrentHp();//get mech or drone
    private GetCurrentHp getCurrentHp, getCurrentMaxHp;

	void Start () {
        PhotonView pv = transform.root.GetComponent<PhotonView>();
        mcbt = transform.root.GetComponent<MechCombat> ();

        if (mcbt == null) {//drone
            drone_mcbt = transform.root.GetComponent<DroneCombat>();
            getCurrentHp = GetCurrentDroneHP;
            getCurrentMaxHp = GetCurrentDroneMaxHP;
        } else {//player
            gameObject.SetActive(!pv.isMine);//do not show my name & hp bar
            getCurrentHp = GetCurrentMechHP;
            getCurrentMaxHp = GetCurrentMechMaxHP;
        }

        name_text = (pv.owner == null) ? "Drone"+Random.Range(0,999) : pv.owner.NickName;
		textMesh.text = name_text;

		if(GameManager.isTeamMode){
			if(PhotonNetwork.player.GetTeam() != pv.owner.GetTeam()){
				bar.color = Color.red; //enemy
				textMesh.color = Color.red; 
			} else {
				bar.color = color_ally;
				textMesh.color = Color.white;
			}
		}else{
			bar.color = Color.red; //enemy
			textMesh.color = Color.red; 
		}
	}
	

	void Update () {
		if (cam != null) {            
			transform.LookAt (cam.transform);
            Vector3 angle = transform.rotation.eulerAngles;
           
            transform.rotation = Quaternion.Euler(0, angle.y, angle.z);

			//update scale
			float distance = Vector3.Distance (transform.position, cam.transform.position);
			distance = Mathf.Clamp (distance, 0, 200f);
			transform.localScale = new Vector3 (1 + distance / 100 * 1.5f, 1 + distance / 100 * 1.5f, 1);
		}else{
			if (Time.time - LastInitRequestTime >0.5f) {
				player = GameObject.Find (PhotonNetwork.playerName);
				if (player != null)
					cam = player.GetComponentInChildren<Camera> ();
				LastInitRequestTime = Time.time;
			}
		}

		//update bar value
		bar.fillAmount = (float)getCurrentHp() / getCurrentMaxHp();
	}

    int GetCurrentMechHP() {
        return mcbt.CurrentHP;
    }

    int GetCurrentMechMaxHP() {
        return mcbt.GetMaxHp();
    }

    int GetCurrentDroneHP() {
        return drone_mcbt.CurrentHP;
    }

    int GetCurrentDroneMaxHP() {
        return drone_mcbt.GetMaxHp();
    }
}
