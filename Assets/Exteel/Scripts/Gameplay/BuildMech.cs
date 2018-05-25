using UnityEngine;
using XftWeapon;

public class BuildMech : Photon.MonoBehaviour {

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003", "PBS000", "SHL009", "SHL501", "APS043", "SHS309","RCL034", "BCN029","BRF025","SGN150","LMG012","ENG041", "ADR000", "" };
																																								        //eng : 14
	[SerializeField]private GameObject RespawnPanel;
	[SerializeField]private MechCombat mcbt = null;
	[SerializeField]private Sounds Sounds;
    [SerializeField]private AnimationEventController AnimationEventController;
    private GameManager gm;
    private WeaponManager WeaponManager;
    private Animator animator;
	private AnimatorOverrideController animatorOverrideController;
	private AnimationClipOverrides clipOverrides;
	private MovementClips MovementClips;

    public delegate void BuildWeaponAction();
    public event BuildWeaponAction OnWeaponBuilt;

    private Transform shoulderL;
	private Transform shoulderR;
	private Transform[] hands;

	public AudioClip[] ShotSounds, ReloadSounds;
	public Weapon[] weaponScripts;
    public string[] curWeaponNames = new string[4];
	public GameObject[] weapons;
	public GameObject[] bulletPrefabs;

	private ParticleSystem Muz;
	private int weaponOffset = 0;

	private bool inHangar = false;
	private bool inStore = false;
	public bool onPanel = false;
	public int Total_Mech = 4;
	public int Mech_Num = 0;
	private const int BLUE = 0, RED = 1;

    //mech properties
    int HP,EN,SP,MPU;
	int ENOutputRate;
	int MinENRequired;
	int Size, Weight;
	int EnergyDrain;

	int MaxHeat, CooldownRate;
	int Marksmanship;

	int ScanRange;

	int BasicSpeed;
	int Capacity;
	int Deceleration;

	int DashOutput;
	int DashENDrain, JumpENDrain;


    public Vector3 handOffset = Vector3.zero;//every hand has dif offset

	void Start () {
        if(WeaponManager==null)WeaponManager = Resources.Load<WeaponManager>("WeaponManager");

        if (SceneManagerHelper.ActiveSceneName == "Hangar" || SceneManagerHelper.ActiveSceneName == "Lobby" || onPanel) inHangar = true;

		if (SceneManagerHelper.ActiveSceneName == "Store")inStore = true;

		// If this is not me, don't build this mech. Someone else will RPC build it
		if (!photonView.isMine && !inHangar && !inStore) return;

		if (UserData.myData.Mech == null) {
			UserData.myData.Mech = new Mech[Total_Mech];
		}
		for(int i=0;i<Total_Mech;i++){//init all datas
			SetMechDefaultIfEmpty (i);
		}

		// Get parts info
		Data data = UserData.myData;
		animator = transform.Find("CurrentMech").GetComponent<Animator> ();

		MovementClips = GetComponent<MovementClips> ();
		if(inHangar || inStore)//do not call this in game otherwise mechcombat gets null parameter
			initAnimatorControllers ();


		weaponOffset = 0;

		if (inHangar || inStore) {
			buildMech(data.Mech[Mech_Num]);
		} else { // Register my name on all clients
			photonView.RPC("SetName", PhotonTargets.AllBuffered, PhotonNetwork.playerName);
		}

		if(onPanel){
			gameObject.transform.SetParent (RespawnPanel.transform);
		}
	}

	void initAnimatorControllers(){
        animatorOverrideController = new AnimatorOverrideController (animator.runtimeAnimatorController);
		animator.runtimeAnimatorController = animatorOverrideController;

		clipOverrides = new AnimationClipOverrides (animatorOverrideController.overridesCount);
		animatorOverrideController.GetOverrides (clipOverrides);
	}

	[PunRPC]
	void SetName(string name) {
		gameObject.name = name;
		findGameManager();
		gm.RegisterPlayer(photonView.viewID, (photonView.owner.GetTeam()==PunTeams.Team.red)? RED : BLUE);// blue & none team => set to blue
	}
		
	public void Build(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r) {
		photonView.RPC("buildMech", PhotonTargets.AllBuffered, c, a, l, h, b, w1l, w1r, w2l, w2r);
	}
				
	private void findHands() {
		shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		hands = new Transform[2];
		hands [0] = shoulderL.Find("upper_arm.L/forearm.L/hand.L");
		hands [1] = shoulderR.Find("upper_arm.R/forearm.R/hand.R");
	}

	private void buildMech(Mech m) {
		buildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
	}

	[PunRPC]
	public void buildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r) {
		findHands ();
		string[] parts = new string[9]{ c, a, l, h, b, w1l, w1r, w2l, w2r };

		for (int i = 0; i < parts.Length-4; i++) {
			parts [i] = string.IsNullOrEmpty(parts[i])? defaultParts [i] : parts [i];
		}

        //set weapons if null (in offline )
        parts[5] = defaultParts[7];
        parts[6] = defaultParts[13];
        if (string.IsNullOrEmpty(parts[5])) parts[5] = defaultParts[7];
        if (string.IsNullOrEmpty(parts[6])) parts[6] = defaultParts[6];
        if (string.IsNullOrEmpty(parts[7])) parts[7] = defaultParts[6];
        if (string.IsNullOrEmpty(parts[8])) parts[8] = defaultParts[13];

        // Create new array to store skinned mesh renderers 
        SkinnedMeshRenderer[] newSMR = new SkinnedMeshRenderer[5];

		Material[] materials = new Material[5];
		for (int i = 0; i < 5; i++) {
			// Load mech part
			GameObject part = Resources.Load (parts [i], typeof(GameObject)) as GameObject;

			// Extract Skinned Mesh
			newSMR [i] = part.GetComponentInChildren<SkinnedMeshRenderer> () as SkinnedMeshRenderer;

			// Load texture
			materials [i] = Resources.Load (parts [i] + "mat", typeof(Material)) as Material;
		}

		// Replace all
		SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer> ();

		for (int i = 0; i < 5; i++) {
			//Note the order of parts in MechFrame.prefab matters

			if (newSMR[i] == null) Debug.LogError(i + " is null.");
			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
			curSMR[i].material = materials[i];
			curSMR[i].enabled = true;
		}

		//set all properties to 0
		initMechProperties ();
		//Load parts info
		LoadCoreInfo (parts [0]);
		LoadHandInfo (parts [1]);
		LoadLegInfo (parts [2]);
		LoadHeadInfo (parts [3]);
		LoadBoosterInfo (parts [4]);

		// Replace weapons
		buildWeapons(new string[4]{parts[5],parts[6],parts[7],parts[8]});
	}

    
	private void initMechProperties(){
		HP = EN = SP = MPU = 0;
		ENOutputRate = 0;
		MinENRequired = 0;
		Size = Weight = 0;
		EnergyDrain = 0;

		MaxHeat = CooldownRate = 0;
		Marksmanship = 0;

		ScanRange = 0;

		BasicSpeed = 0;
		Capacity = 0;
		Deceleration = 0;

		DashOutput = 0;
		DashENDrain = JumpENDrain = 0;
	}

	private void LoadCoreInfo(string part){
		switch(part){
		case "CES301":
			CoreProperties (new CES301 ());
			break;
		}
	}
	private void CoreProperties(Core core){
		EN += core.EN;
		ENOutputRate += core.ENOutputRate;
		MinENRequired += core.MinENRequired;
		HP += core.HP;
		Size += core.Size;
		Weight += core.Weight;
		EnergyDrain += core.EnergyDrain;
	}

	private void LoadHandInfo(string part){
		switch(part){
		case "AES104":
			HandProperties (new AES104 ());
			break;
		}
	}
	private void HandProperties(Hand hand){
		HP += hand.HP;
		MaxHeat += hand.MaxHeat;
		CooldownRate += hand.CooldownRate;
		Marksmanship += hand.Marksmanship;
		Size += hand.Size;
		Weight += hand.Weight;
	}

	private void LoadHeadInfo(string part){
		switch(part){
		case "HDS003":
			HeadProperties (new HDS003 ());
			break;
		}
	}
	private void HeadProperties(Head head){
		SP += head.SP;
		MPU += head.MPU;
		ScanRange += head.ScanRange;
		HP += head.HP;
		Weight += head.Weight;
		EnergyDrain += head.EnergyDrain;
		Size += head.Size;
	}

	private void LoadLegInfo(string part){
		switch(part){
		case "LTN411":
			LegProperties (new LTN411 ());
			break;
		}
	}
	private void LegProperties(Leg leg){
		BasicSpeed += leg.BasicSpeed;
		Capacity += leg.Capacity;
		Deceleration += leg.Deceleration;
		HP += leg.HP;
		Size += leg.Size;
		Weight += leg.Weight;
	}

	private void LoadBoosterInfo(string part){
		switch(part){
		case "PBS000":
			BoosterProperties (new PBS000 ());
			break;
		}
	}
	private void BoosterProperties(Booster booster){
		DashOutput += booster.DashOutput;
		DashENDrain += booster.DashENDrain;
		JumpENDrain += booster.JumpENDrain;
		HP += booster.HP;
		Size += booster.Size;
		Weight += booster.Weight;
		EnergyDrain += booster.EnergyDrain;
	}

    private void buildWeapons (string[] weaponNames) {
        if(WeaponManager==null) WeaponManager = Resources.Load<WeaponManager>("WeaponManager");

        if (weapons != null) for (int i = 0; i < weapons.Length; i++) if (weapons[i] != null) Destroy(weapons[i]);
		weapons = new GameObject[4];
		weaponScripts = new Weapon[4];
		bulletPrefabs = new GameObject[4];
		ShotSounds = new AudioClip[4];
        ReloadSounds = new AudioClip[4];

        for (int i = 0; i < weaponNames.Length; i++) {
            weaponScripts[i] = WeaponManager.FindData(weaponNames[i]);
            if (weaponScripts[i] == null) {
                Debug.Log("Can't find weapon data : " + weaponNames[i]);
                continue;
            }

            weapons[i] = Instantiate(weaponScripts[i].weaponPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            //TODO : remake this (it distorts on respawn panel )
            weapons[i].transform.localScale = new Vector3(weapons[i].transform.localScale.x * transform.root.localScale.x,
               weapons[i].transform.localScale.y * transform.root.localScale.y, weapons[i].transform.localScale.z * transform.root.localScale.z);

            if (weaponScripts[i].twoHanded) {
                weapons[i].transform.SetParent(hands[(i + 1) % 2]);
                if (weaponScripts[i].Grip[(i+1) % 2] == null) { Debug.LogError("The right hand grip is null, two handed weapons must have right hand grip"); continue; }
                weapons[i].transform.localRotation = weaponScripts[i].Grip[(i+1) % 2].transform.rotation;
            } else {
                weapons[i].transform.SetParent(hands[i % 2]);
                if (weaponScripts[i].Grip[i % 2] == null) { Debug.LogError(i+" weapon grip is null"); continue; }
                weapons[i].transform.localRotation = weaponScripts[i].Grip[i % 2].transform.rotation;
            }

            //Adjust weapon local position by hand offset
            weapons[i].transform.localPosition = handOffset;

            switch (weaponScripts[i].weaponType) {
                case "Sword":
                    bulletPrefabs[i] = null;
                    Sounds.LoadSlashClips(i, ((Sword)weaponScripts[i]).slash_sound);
                    Sounds.LoadSlashOnHitClips(i, ((Sword)weaponScripts[i]).slash_hit_sound);
                break;
                case "Spear":
                    bulletPrefabs[i] = null;
                    Sounds.LoadSmashClips(i, ((Spear)weaponScripts[i]).smash_sound);
                    Sounds.LoadSmashOnHitClips(i, ((Spear)weaponScripts[i]).smash_hit_sound);
                break;
                case "Shield":
                    bulletPrefabs[i] = null;
                    ShieldUpdater shieldUpdater = weapons[i].GetComponentInChildren<ShieldUpdater>();
                    shieldUpdater.SetDefendEfficiency(((Shield)weaponScripts[i]).defend_melee_efficiency, ((Shield)weaponScripts[i]).defend_ranged_efficiency);
                    shieldUpdater.SetHand(i % 2);
                break;
                case "Cannon":
                case "Rocket":
                    bulletPrefabs[i] = ((RangedWeapon)weaponScripts[i]).bulletPrefab;
                    ShotSounds[i] = ((RangedWeapon)weaponScripts[i]).shoot_sound;
                    ReloadSounds[i] = ((RangedWeapon)weaponScripts[i]).reload_sound;
                    weapons[i + 1] = null;
                    bulletPrefabs[i + 1] = null;
                    i++;
                break;
		        default://other ranged weapon
                    bulletPrefabs[i] = ((RangedWeapon)weaponScripts[i]).bulletPrefab;
                    ShotSounds[i] = ((RangedWeapon)weaponScripts[i]).shoot_sound;
                    ReloadSounds[i] = ((RangedWeapon)weaponScripts[i]).reload_sound;
                break;
			}
		}

        Sounds.LoadReloadClips(ReloadSounds);
        Sounds.LoadShotClips(ShotSounds);

        UpdateCurWeaponNames();

        animator = transform.Find("CurrentMech").GetComponent<Animator> ();//if in game , then animator is not ini. in start
		if (animator != null && (inHangar || inStore))CheckAnimatorState ();

        if (weapons[(weaponOffset + 2) % 4] != null) weapons [(weaponOffset+2)%4].SetActive (false);
		if(weapons[(weaponOffset + 3) % 4] != null) weapons [(weaponOffset+3)%4].SetActive (false);

		if(mcbt!=null)UpdateMechCombatVars ();//this will turn trail on ( enable all renderer)
		for (int i = 0; i < 4; i++)//turn off trail
			ShutDownTrail (weapons [i]);
	}


	public void EquipWeapon(string weapon, int weapPos) {
        Weapon newWeapon = WeaponManager.FindData(weapon);
        if (newWeapon == null) {
            Debug.LogError("Can't find weapon data : " + weapon);
            return;
        }

        //if previous is two-handed => also destroy left hand 
        if (weapPos==3){
			if (weapons [2] != null) {
				if (weaponScripts [2].twoHanded) {
					if (!inStore)UserData.myData.Mech [Mech_Num].Weapon2L = "";

					Destroy (weapons [2]);
                    weaponScripts[2] = null;
				}
			}
		}else if(weapPos==1){
			if (weapons [0] != null) {
				if (weaponScripts[0].twoHanded) {
                    if (!inStore)UserData.myData.Mech [Mech_Num].Weapon1L = "";

					Destroy (weapons [0]);
                    weaponScripts [0] = null;
				}
			}
		}

		//if the new one is two-handed => also destroy right hand
		if(newWeapon.twoHanded){
			if(weapons[weapPos+1]!=null)
				Destroy (weapons[weapPos + 1]);
			if(weapPos==0){
				if(!inStore)
					UserData.myData.Mech [Mech_Num].Weapon1R = "";
                weaponScripts[1] = null;
			}else if(weapPos==2){
				if(!inStore)
					UserData.myData.Mech [Mech_Num].Weapon2R = "";
                weaponScripts[3] = null;
			}
		}

        //destroy the current weapon on the hand position
		if (weapons [weapPos] != null) 
			Destroy (weapons [weapPos]);

        switch (newWeapon.weaponType) {
            case "Cannon":
            case "Rocket":
                weapPos = (weapPos >= 2) ? 2 : 0; //script is on left hand

                weapons[weapPos + 1] = null;
                if (weapPos >= 2) {
                    if (!inStore)
                        UserData.myData.Mech[Mech_Num].Weapon2R = "";
                    weaponScripts[3] = null;
                } else {
                    if (!inStore)
                        UserData.myData.Mech[Mech_Num].Weapon1R = "";
                    weaponScripts[1] = null;
                }

                weapons[weapPos] = Instantiate(newWeapon.weaponPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                weapons[weapPos].transform.SetParent(hands[(weapPos+1) % 2]);
                weapons[weapPos].transform.localPosition = handOffset;
                weapons[weapPos].transform.localRotation = newWeapon.Grip[(weapPos+1) % 2].transform.rotation;

            break;
            default:

            weapons[weapPos] = Instantiate(newWeapon.weaponPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            weapons[weapPos].transform.SetParent(hands[weapPos % 2]);
            weapons[weapPos].transform.localPosition = handOffset;
            weapons[weapPos].transform.localRotation = newWeapon.Grip[weapPos % 2].transform.rotation;

            break;
        }

        //replace the script
        weaponScripts[weapPos] = newWeapon;
        UpdateCurWeaponNames();
        weapons[weapPos].SetActive (weapPos == weaponOffset || weapPos == weaponOffset+1);

		ShutDownTrail (weapons[weapPos]);
		if(animator!=null)CheckAnimatorState ();
        UpdateMechCombatVars();
    }
		
	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}

	public void DisplayFirstWeapons(){
		weaponOffset = 0;

		for(int i=0;i<4;i++)if(weaponScripts[i]!=null)EquipWeapon (weaponScripts[i].weaponPrefab.name, i);
	}

	public void DisplaySecondWeapons(){
		weaponOffset = 2;

		for(int i=0;i<4;i++)if(weaponScripts[i] != null) EquipWeapon (weaponScripts[i].weaponPrefab.name, i);
	}

	public void CheckAnimatorState(){
		if (animator == null)
			return;

        int num = (weaponScripts [weaponOffset].twoHanded)? 1 : 0;

		clipOverrides ["Idle"] = MovementClips.Idle [num];
		clipOverrides ["Run_Left"] = MovementClips.Run_Left[num];
		clipOverrides ["Run_Front"] = MovementClips.Run_Front[num];;
		clipOverrides ["Run_Right"] = MovementClips.Run_Right[num];
		clipOverrides ["BackWalk"] = MovementClips.BackWalk [num];

		clipOverrides ["Hover_Back_01"] = MovementClips.Hover_Back_01[num];
		clipOverrides ["Hover_Back_02"] = MovementClips.Hover_Back_02[num];
		clipOverrides ["Hover_Back_03"] = MovementClips.Hover_Back_03[num];

		clipOverrides ["Hover_Left_01"] = MovementClips.Hover_Left_01[num];
		clipOverrides ["Hover_Left_02"] = MovementClips.Hover_Left_02[num];
		clipOverrides ["Hover_Left_03"] = MovementClips.Hover_Left_03[num];
		clipOverrides ["Hover_Right_01"] = MovementClips.Hover_Right_01[num];
		clipOverrides ["Hover_Right_02"] = MovementClips.Hover_Right_02[num];
		clipOverrides ["Hover_Right_03"] = MovementClips.Hover_Right_03[num];
		clipOverrides ["Hover_Front_01"] = MovementClips.Hover_Front_01[num];
		clipOverrides ["Hover_Front_02"] = MovementClips.Hover_Front_02[num];
		clipOverrides ["Hover_Front_03"] = MovementClips.Hover_Front_03[num];

		clipOverrides ["Jump01"] = MovementClips.Jump01[num];
		clipOverrides ["Jump02"] = MovementClips.Jump02[num];
		clipOverrides ["Jump03"] = MovementClips.Jump03[num];
		clipOverrides ["Jump06"] = MovementClips.Jump06[num];
		clipOverrides ["Jump07"] = MovementClips.Jump07[num];
		clipOverrides ["Jump08"] = MovementClips.Jump08[num];

		animatorOverrideController.ApplyOverrides (clipOverrides);
	}

	void SetMechDefaultIfEmpty(int mehc_num){
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Core)){
			UserData.myData.Mech[mehc_num].Core = defaultParts [0];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Arms)){
			UserData.myData.Mech[mehc_num].Arms = defaultParts [1];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Legs)){
			UserData.myData.Mech[mehc_num].Legs = defaultParts [2];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Head)){
			UserData.myData.Mech[mehc_num].Head = defaultParts [3];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Booster)){
			UserData.myData.Mech[mehc_num].Booster = defaultParts [4];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon1L)){
			UserData.myData.Mech[mehc_num].Weapon1L = defaultParts [5];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon1R)){
			UserData.myData.Mech[mehc_num].Weapon1R = defaultParts [5];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon2L)){
			UserData.myData.Mech[mehc_num].Weapon2L = defaultParts [5];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon2R)){
			UserData.myData.Mech[mehc_num].Weapon2R = defaultParts [11];
		}
	}

    void UpdateCurWeaponNames() {
        for(int i = 0; i < 4; i++) {
            if(weaponScripts[i]!=null)
                curWeaponNames[i] = weaponScripts[i].weaponPrefab.name;
        }
    }
	public void SetMechNum(int num){
		Mech_Num = num;
	}

	void ShutDownTrail(GameObject weapon){
        if (weapon == null)
            return;

        Transform trail_transform = weapon.transform.Find("trail");
        XWeaponTrail trail = (trail_transform == null )? null : trail_transform.GetComponent<XWeaponTrail>();

        if (trail == null)
			return;

		trail.Deactivate ();
	}

	void UpdateMechCombatVars(){
		if (mcbt == null)
			return;
        if (OnWeaponBuilt != null)OnWeaponBuilt();
        if(mcbt.OnWeaponSwitched!=null)mcbt.OnWeaponSwitched();

		mcbt.EnableAllRenderers (true);
		mcbt.EnableAllColliders (true);
	}
}