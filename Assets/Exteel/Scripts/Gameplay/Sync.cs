using UnityEngine;
using Photon;
using System.Collections;
using System;

public class Sync : Photon.MonoBehaviour {

	[SerializeField]PhotonView pv;
    [SerializeField]SkillController SkillController;
	[SerializeField]Camera cam;

    //sync vals
    private Vector3 trueLoc;
    private Quaternion trueRot;

    private bool onSkill = false;

    private void Awake() {
        RegisterOnSkill();    
    }

    private void RegisterOnSkill() {
        if(SkillController!=null)SkillController.OnSkill += OnSkill;
    }
    
    public void OnSkill(bool b) {
        enabled = !b;
    }

    void Update () {
		if(!pv.isMine){
			transform.position = Vector3.Lerp(transform.position, trueLoc, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, trueRot, Time.deltaTime * 5);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		//we are reicieving data
		if (stream.isReading)
		{
			if(!pv.isMine){
				this.trueLoc = (Vector3)stream.ReceiveNext();
				this.trueRot = (Quaternion)stream.ReceiveNext();
				cam.transform.rotation = (Quaternion)stream.ReceiveNext ();
			}
		}else{
			if(pv.isMine){
				stream.SendNext(transform.position);
				stream.SendNext(transform.rotation);
				stream.SendNext (cam.transform.rotation);
			}
		}
	}
}
