using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {
	private LayerMask Terrain = 10;
	private GameManager gm;
	private PhotonView gmpv;
	public PunTeams.Team team = PunTeams.Team.none;
	[SerializeField]
	private ParticleSystem ps;
	private bool isOnPlay = true;
	public bool isGrounded = true;
	public bool isOnBase = true;

	void Start(){
		ps.Play ();
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		gmpv = gm.GetComponent<PhotonView> ();
	}
	// Update is called once per frame
	void Update () {
		if(!isGrounded){
			if(isOnPlay){
				ps.Stop ();
				isOnPlay = false;
			}
		}else{
			if (!isOnPlay) {
				ps.Play ();
				isOnPlay = true;
			}
		}
	}

	void OnTriggerEnter(Collider collider){
		if (isGrounded == false || collider.gameObject.layer == Terrain)
			return;

		PhotonView pv = collider.transform.root.GetComponent<PhotonView> ();
		PhotonPlayer player = pv.owner;
		print ("flag triggered by : " + player.NickName);
		if (!pv.isMine)
			return;

		if(player.GetTeam() == team){
			//do I hold the other team's flag ? 
			if(bool.Parse(player.CustomProperties["isHoldFlag"].ToString())){
				//Register score
				print ("send register score");
				gmpv.RPC ("GetScoreRequest", PhotonTargets.MasterClient, pv.viewID);
			}else if(!isOnBase){
				//send back the flag
				print ("send back the flag");
				gmpv.RPC ("playerHoldFlag", PhotonTargets.All, -1, (team == PunTeams.Team.red) ? 1 : 0);
			}

		}else{
			//send get flag request to master
			gmpv.RPC ("GetFlagRequest", PhotonTargets.MasterClient, pv.viewID, (team == PunTeams.Team.blue) ? 0 : 1);
		}

	}
}
