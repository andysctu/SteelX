using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	private MechCombat MechCombat;
	// Sound clip
	private AudioClip[] shotClips = new AudioClip[4];
    private AudioClip[] reloadClips = new AudioClip[4];
    private AudioClip[] slashClips = new AudioClip[16];// L : 0~7 , R : 8~15
    private AudioClip[] smashClips = new AudioClip[4];
    private AudioClip[] SlashOnHit = new AudioClip[4];
	private AudioClip[] SmashOnHit = new AudioClip[4];  //no files

	[SerializeField]PhotonView pv;
	[SerializeField]AudioClip Lock, OnLocked;
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

    public void LoadShotClips(AudioClip[] shotClips) {
        this.shotClips = shotClips;
    }
    public void LoadReloadClips(AudioClip[] reloadClips) {
        this.reloadClips = reloadClips;
    }

    public void LoadSlashClips(int weap, AudioClip[] slashClips) {//weap : 0,1,2,3
        for (int i = 0; i < slashClips.Length; i++) {
            if(weap%2==0)
                this.slashClips[4 * (weap/2) + i] = slashClips[i];
            else
                this.slashClips[8 + 4 * ((weap-1)/2) + i] = slashClips[i];
        }
    }

    public void LoadSmashClips(int weap, AudioClip smashClips) {
        this.smashClips[weap] = smashClips;
    }

    public void LoadSlashOnHitClips(int weap, AudioClip slashOnHit) {
        SlashOnHit[weap] = slashOnHit;
    }
    public void LoadSmashOnHitClips(int weap, AudioClip smashOnHit) {
        SmashOnHit[weap] = smashOnHit;
    }
	
	public void PlayShot(int hand) {  // RCL is also using this
		if(shotClips[MechCombat.weaponOffset + hand]!=null)
			Source.PlayOneShot(shotClips[MechCombat.weaponOffset + hand]);
	}

	public void PlaySlashL(int num){//num : 0,1,2,3
		Source.PlayOneShot (slashClips[num + MechCombat.weaponOffset/2*4]);
	}
	public void PlaySlashR(int num){
		Source.PlayOneShot (slashClips[8 + num + MechCombat.weaponOffset/2*4]);
	}

    public void PlayReload(int hand) {
        if (reloadClips[MechCombat.weaponOffset + hand] != null)
            Source.PlayOneShot(reloadClips[MechCombat.weaponOffset + hand]);
    }

    public void PlaySmash(int hand) {
        if(smashClips[MechCombat.weaponOffset + hand] != null)
            Source.PlayOneShot(smashClips[MechCombat.weaponOffset + hand]);
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
