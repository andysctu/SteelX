using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {
    [SerializeField]private MechCombat MechCombat;
    [SerializeField]private PhotonView pv;
    [SerializeField]private AudioClip Lock, OnLocked;
    [SerializeField]private AudioClip SwitchWeapon;
    [SerializeField]private AudioClip BCNload, BCNPose;
    //[SerializeField]AudioClip WalkSound;
    [SerializeField]private AudioSource Source;
    [SerializeField]private AudioSource MovementSource;

	private AudioClip[] shotClips = new AudioClip[4];
    private AudioClip[] reloadClips = new AudioClip[4];
    private AudioClip[] slashClips = new AudioClip[16];// L : 0~7 , R : 8~15
    private AudioClip[] smashClips = new AudioClip[4];
    private AudioClip[] SlashOnHit = new AudioClip[4];
	private AudioClip[] SmashOnHit = new AudioClip[4];//no files

    private int weaponOffset = 0;

    void Awake() {
        if(MechCombat!=null)MechCombat.OnWeaponSwitched += UpdateSounds;
    }
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

    private void UpdateSounds() {
        weaponOffset = MechCombat.GetCurrentWeaponOffset();
    }

    public void LoadShotClips(AudioClip[] shotClips) {
        this.shotClips = shotClips;
    }
    public void LoadReloadClips(AudioClip[] reloadClips) {
        this.reloadClips = reloadClips;
    }

    public void LoadSlashClips(int weap, AudioClip[] slashClips) {//weap : 0,1,2,3
        for (int i = 0; i < slashClips.Length; i++) {
            if (weap % 2 == 0) {
                this.slashClips[4 * (weap / 2) + i] = slashClips[i];
            } else
                this.slashClips[8 + 4 * ((weap - 1) / 2) + i] = slashClips[i];
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
		if(shotClips[weaponOffset + hand]!=null)
			Source.PlayOneShot(shotClips[weaponOffset + hand]);
	}

	public void PlaySlashL(int num){//num : 0,1,2,3
		Source.PlayOneShot (slashClips[num + weaponOffset/2*4]);
	}
	public void PlaySlashR(int num){
		Source.PlayOneShot (slashClips[8 + num + weaponOffset/2*4]);
	}

    public void PlayReload(int hand) {
        if (reloadClips[weaponOffset + hand] != null)
            Source.PlayOneShot(reloadClips[weaponOffset + hand]);
    }

    public void PlaySmash(int hand) {
        if(smashClips[weaponOffset + hand] != null)
            Source.PlayOneShot(smashClips[weaponOffset + hand]);
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
