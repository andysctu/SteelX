using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	private MechCombat MechCombat;
	// Sound clip
	[HideInInspector]public AudioClip[] ShotSounds = new AudioClip[4]; // init in MechCombat 
	[HideInInspector]public AudioClip[] SlashL = new AudioClip[6];
	[HideInInspector]public AudioClip[] SlashR = new AudioClip[6];
	[HideInInspector]public AudioClip[] SlashOnHit = new AudioClip[4];
	//[HideInInspector]public AudioClip[] SmashOnHit = new AudioClip[4];  //no files

	[SerializeField]PhotonView pv;
	[SerializeField]AudioClip Lock;
	[SerializeField]AudioClip OnLocked;
	[SerializeField]AudioClip Smash;
	[SerializeField]AudioClip SwitchWeapon;
	[SerializeField]AudioClip BCNload,BCNPose;
	//[SerializeField]AudioClip WalkSound;

	[SerializeField]private AudioSource Source;
	[SerializeField]private AudioSource MovementSource;

	// Use this for initialization
	void Start () {
		initComponent ();
		SetVolume (0.3f);
	}

	void initComponent(){
		MechCombat = transform.root.GetComponent<MechCombat> ();
	}

	void SetVolume(float v){
		if(MovementSource!=null)
			MovementSource.volume = v;

		if(Source!=null)
			Source.volume = v;
	}

	public void UpdateSounds(int Offset){
		MechCombat.weaponOffset = Offset;
	}
	
	public void PlayShotL() {  // RCL is also using this
		if(ShotSounds[MechCombat.weaponOffset]!=null)
			Source.PlayOneShot(ShotSounds[MechCombat.weaponOffset]);
	}
	public void PlayShotR() {
		if(ShotSounds[MechCombat.weaponOffset+1]!=null)
			Source.PlayOneShot(ShotSounds[MechCombat.weaponOffset+1]);
	}

	public void PlaySlashL(int num){
		Source.PlayOneShot (SlashL[num + MechCombat.weaponOffset/2*3]);
	}
	public void PlaySlashR(int num){
		Source.PlayOneShot (SlashR[num + MechCombat.weaponOffset/2*3]);
	}

	public void PlayLock(){
		Source.PlayOneShot (Lock);
	}
	public void PlayOnLocked(){
		Source.PlayOneShot (OnLocked);
	}

	public void PlayBCNPose(){
		Source.PlayOneShot (BCNPose);
	}

	public void PlayBCNload(){
		Source.PlayOneShot (BCNload);
	}

	public void PlaySlashOnHit(int num){
		pv.RPC("RPCPlaySlashOnHit", PhotonTargets.All, num);
	}

	/*public void PlayWalk(bool b){
		if(b){
			MovementSource.Play ();
		}else{
			MovementSource.Stop ();
		}
	}*/

	public void PlaySwitchWeapon(){
		Source.PlayOneShot (SwitchWeapon);
	}

	[PunRPC]
	void RPCPlaySlashOnHit(int num){
		if(SlashOnHit[num]!=null)
			Source.PlayOneShot (SlashOnHit[num]);
	}

	/*[PunRPC]
	void RPCPlaySmashOnHit(int num){
		Source.PlayOneShot (SmashOnHit[num]);
	}*/
}
