using UnityEngine;

public class BuildMech : Photon.MonoBehaviour {
    //Components
    [SerializeField] private Transform RootBone;    
    private MechCombat MechCombat;
    private MechIK MechIK;
    private WeaponDataManager WeaponDataManager;
    private SkillManager SkillManager;
    private MechPartManager MechPartManager;    
    private SkillController SkillController;

    private GameManager gm;
    private OperatorStatsUI OperatorStatsUI;

    //Weapons
    [HideInInspector] public WeaponData[] WeaponDatas = new WeaponData[4];
    [HideInInspector] public Weapon[] Weapons = new Weapon[4];
    private int weaponOffset = 0;
    private Transform[] hands;

    //Mech parts
    public Part[] curMechParts = new Part[5];
    public MechProperty MechProperty;

    //Mech animator settings
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;
    private MovementClips defaultMovementClips, TwoHandedMovementClips;

    //Settings    
    private bool buildLocally = false, isDataGetSaved = true;
    private int Total_Mech = 4;
    public int Mech_Num = 0;
    public bool onPanel = false;

    public delegate void BuildWeaponAction();
    public event BuildWeaponAction OnMechBuilt;

    private string[] defaultParts = { "CES301", "AES104", "LTN411", "HDS003", "PBS016", "SHL009", "SHL501", "APS043", "SHS309", "RCL034", "BCN029", "BRF025", "SGN150", "LMG012", "ENG041", "ADR000", "Empty" };

    private void Awake() {
        //For not starting from login
        if (UserData.myData.Mech == null) {
            GameObject g = new GameObject();
            UserData u = g.AddComponent<UserData>();
            if(UserData.myData.Mech == null)Debug.Log("user data null");
        }

        FindGameManager();
        LoadMovementClips();
        LoadManagers();
        InitComponents();
        FindHands();
    }

    private void FindGameManager() {
        gm = FindObjectOfType<GameManager>();
    }

    private void LoadMovementClips() {
        defaultMovementClips = Resources.Load<MovementClips>("Data/MovementClip/Default");
        TwoHandedMovementClips = Resources.Load<MovementClips>("Data/MovementClip/TwoHanded");
    }

    private void LoadManagers() {
        WeaponDataManager = Resources.Load<WeaponDataManager>("Data/Managers/WeaponDataManager");
        SkillManager = Resources.Load<SkillManager>("Data/Managers/SkillManager");
        MechPartManager = Resources.Load<MechPartManager>("Data/Managers/MechPartManager");
    }

    private void InitComponents() {
        Transform CurrentMech = transform.Find("CurrentMech");        
        animator = CurrentMech.GetComponent<Animator>();
        MechCombat = GetComponent<MechCombat>();
        MechIK = CurrentMech.GetComponent<MechIK>();
        SkillController = GetComponent<SkillController>();
    }

    private void FindHands() {
        Transform shoulderL = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_L_Clavicle");
        Transform shoulderR = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_R_Clavicle");

        hands = new Transform[2];
        hands[0] = shoulderL.Find("Bip01_L_UpperArm/Bip01_L_ForeArm/Bip01_L_Hand/Weapon_lft_Bone");
        hands[1] = shoulderR.Find("Bip01_R_UpperArm/Bip01_R_ForeArm/Bip01_R_Hand/Weapon_rt_Bone");
    }

    private void Start() {        
        CheckIfBuildLocally();
        CheckIsDataGetSaved();

        // If this is not me, don't build this mech. Someone else will RPC build it
        if (!buildLocally && !photonView.isMine) return;

        InitMechData();
        InitAnimatorControllers();

        if (buildLocally) {
            if(!onPanel)OperatorStatsUI = FindObjectOfType< OperatorStatsUI >();
            buildMech(UserData.myData.Mech[0]);
        } else if (tag != "Drone") { // Register my name on all clients
            photonView.RPC("SetName", PhotonTargets.AllBuffered, PhotonNetwork.player);
        }
    }

    private void InitMechData() {
        if (UserData.myData.Mech == null) {
            UserData.myData.Mech = new Mech[Total_Mech];
        }

        for (int i = 0; i < Total_Mech; i++) {
            SetMechDefaultIfEmpty(i);
        }
    }

    private void InitAnimatorControllers() {//do not call this in game otherwise mechcombat gets null parameter
        if (!buildLocally) return;

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);//write clips into clipOverrides
    }

    [PunRPC]
    private void SetName(PhotonPlayer player) {//TODO : consider not putting here
        gameObject.name = player.NickName;
        gm.RegisterPlayer(player);
    }

    public void Build(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r, int[] skillIDs) {
        photonView.RPC("buildMech", PhotonTargets.AllBuffered, c, a, l, h, b, w1l, w1r, w2l, w2r, skillIDs);
    }

    private void buildMech(Mech m) {
        buildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
    }

    [PunRPC]
    public void buildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r, int[] skill_IDs) {        
        string[] parts = new string[9] { c, a, l, h, b, w1l, w1r, w2l, w2r };

        for (int i = 0; i < parts.Length - 4; i++) {            
            parts[i] = string.IsNullOrEmpty(parts[i]) ? defaultParts[i] : parts[i];
        }

        //set weapons if null (in offline)
        if (string.IsNullOrEmpty(parts[5])) parts[5] = defaultParts[6];
        if (string.IsNullOrEmpty(parts[6])) parts[6] = defaultParts[15];
        if (string.IsNullOrEmpty(parts[7])) parts[7] = defaultParts[13];
        if (string.IsNullOrEmpty(parts[8])) parts[8] = defaultParts[13];

        if (skill_IDs == null) {//TODO : remake this
            Debug.Log("skill_ids is null. Set defualt skills");
            SkillManager.GetAllSkills();
            skill_IDs = new int[4] { 0, 1, 3, 4 };
        }

        // Create new array to store skinned mesh renderers
        SkinnedMeshRenderer[] newSMR = new SkinnedMeshRenderer[4];

        for (int i = 0; i < 4; i++) {
            // Load mech part & info
            Part part = MechPartManager.FindData(parts[i]);
            if (part != null) {
                curMechParts[i] = part;
            } else {
                curMechParts[i] = null;
                Debug.LogError("Can't find the part in MechPartManager");
                continue;
            }
            // Extract Skinned Mesh
            newSMR[i] = part.GetPartPrefab().GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
        }

        // Replace all
        SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < 4; i++) {//TODO : remake the order part
                                     //Note the order of parts in MechFrame.prefab matters
            if (newSMR[i] == null) { Debug.LogError(i + " is null."); continue; }

            ProcessBonedObject(newSMR[i], curSMR[i]);

            curSMR[i].sharedMesh = newSMR[i].sharedMesh;

            Material[] mats = new Material[2];
            mats[0] = Resources.Load("MechPartMaterials/" + parts[i] + "mat", typeof(Material)) as Material;
            mats[1] = Resources.Load("MechPartMaterials/" + parts[i] + "_2mat", typeof(Material)) as Material;
            curSMR[i].materials = mats;

            curSMR[i].enabled = true;
        }

        LoadBooster(parts[4]);

        LoadAllPartInfo();

        buildSkills(skill_IDs);

        // Replace weapons
        BuildWeapons(new string[4] { parts[5], parts[6], parts[7], parts[8] });

        if (!buildLocally) {
            UpdateMechCombatVars();
        } else if(!onPanel) {//display properties            
            OperatorStatsUI OperatorStatsUI = FindObjectOfType<OperatorStatsUI>();
            if (OperatorStatsUI != null) {
                OperatorStatsUI.DisplayMechProperties();
            }
        }
    }

    public void ProcessBonedObject(SkinnedMeshRenderer newPart, SkinnedMeshRenderer partToSwitch) {
        Transform[] MyBones = new Transform[newPart.bones.Length];

        for (var i = 0; i < newPart.bones.Length; i++) {
            if (newPart.bones[i].name.Contains(newPart.name)) {
                string boneName = newPart.bones[i].name.Remove(0, 6);
                string boneToFind = "Bip01" + boneName;
                MyBones[i] = TransformExtension.FindDeepChild(RootBone, boneToFind);
            }

            if (MyBones[i] == null) {
                MyBones[i] = TransformExtension.FindDeepChild(RootBone.transform, newPart.bones[i].name, newPart.bones[i].parent.name);
            }

            if (MyBones[i] == null) {
                Transform parent;
                if (newPart.bones[i].parent.name == "Bip01") {//the root bone is not checked
                    RootBone.transform.rotation = Quaternion.identity;
                    parent = RootBone.transform;

                    GameObject newbone = new GameObject(newPart.bones[i].name); //TODO : improve this (mesh on hip has rotation bug , temp. use this to solve)
                    newbone.transform.parent = parent;
                    newbone.transform.localPosition = Vector3.zero;
                    newbone.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    newbone.transform.localScale = new Vector3(1, 1, 1);
                    MyBones[i] = newbone.transform;
                } else {
                    parent = TransformExtension.FindDeepChild(RootBone.transform, newPart.bones[i].parent.name);
                    MyBones[i] = parent;
                }

                if (parent == null) {
                    Debug.LogError("Can't locate the bone : " + newPart.bones[i].name);
                }
            }
        }
        partToSwitch.bones = MyBones;
    }

    public void ReplaceMechPart(string toReplace, string newPart) {
        Part p = MechPartManager.FindData(newPart);
        if (p == null) {
            Debug.LogError("Can't find the new part");
            return;
        }

        for (int i = 0; i < 5; i++) {
            if (curMechParts[i] != null) {
                if (curMechParts[i].name == toReplace) {
                    curMechParts[i] = p;
                    LoadAllPartInfo();
                    return;
                }
            }
        }
        Debug.Log("Fail to replace");
    }

    private void LoadAllPartInfo() {
        MechProperty = new MechProperty();

        for (int i = 0; i < 5; i++) {
            if (curMechParts[i] != null) {
                curMechParts[i].LoadPartInfo(ref MechProperty);
            }
        }
    }

    private void LoadBooster(string booster_name) {
        Transform boosterbone = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/BackPack_Bone");
        if (boosterbone == null) { Debug.LogError("boosterbone is null"); return; }

        GameObject booster = (boosterbone.childCount == 0) ? null : boosterbone.GetChild(0).gameObject;
        if (booster != null) {
            DestroyImmediate(booster);
        }
        Part booster_part = MechPartManager.FindData(booster_name);
        curMechParts[4] = booster_part;
        if (booster_part == null) { Debug.Log("Can't find booster"); return; };

        GameObject newBooster_prefab = booster_part.GetPartPrefab();

        GameObject newBooster = Instantiate(newBooster_prefab, boosterbone);
        newBooster.transform.localPosition = Vector3.zero;
        newBooster.transform.localRotation = Quaternion.Euler(90, 0, 0);

        //Load info
        booster_part.LoadPartInfo(ref MechProperty);

        //switch booster aniamtion clips
        if (newBooster != null && newBooster.GetComponent<Animator>() != null && newBooster.GetComponent<Animator>().runtimeAnimatorController != null) {
            AnimatorOverrideController boosterAnimtor_OC = new AnimatorOverrideController(newBooster.GetComponent<Animator>().runtimeAnimatorController);
            newBooster.GetComponent<Animator>().runtimeAnimatorController = boosterAnimtor_OC;

            AnimationClipOverrides boosterClipOverrides = new AnimationClipOverrides(boosterAnimtor_OC.overridesCount);
            boosterAnimtor_OC.GetOverrides(boosterClipOverrides);

            boosterClipOverrides["open"] = ((Booster)booster_part).GetOpenAnimation();
            boosterClipOverrides["close"] = ((Booster)booster_part).GetCloseAnimation();

            boosterAnimtor_OC.ApplyOverrides(boosterClipOverrides);
        }
    }

    private void BuildWeapons(string[] weaponNames) {
        //Destroy previous weapons
        if (Weapons != null) {
            for (int i = 0; i < Weapons.Length; i++) {
                WeaponDatas[i] = null;
                if (Weapons[i] != null) {
                    Weapons[i].OnDestroy();
                    Weapons[i] = null;
                }
            }
        }

        //Find and create corresponding weapon script
        for (int i = 0; i < WeaponDatas.Length; i++) {
            WeaponDatas[i] = (i >= weaponNames.Length || weaponNames[i] == "Empty" || string.IsNullOrEmpty(weaponNames[i])) ? null : WeaponDataManager.FindData(weaponNames[i]);

            if (WeaponDatas[i] == null) {
                if (i < weaponNames.Length && (weaponNames[i] == "Empty" || string.IsNullOrEmpty(weaponNames[i])))
                    Debug.LogError("Can't find weapon data : " + weaponNames[i]);
                continue;
            }

            Weapons[i] = (Weapon)(WeaponDatas[i].GetWeaponObject());
        }

        //Init weapon scripts
        for(int i = 0; i < WeaponDatas.Length; i++) {
            Transform weapPos = (WeaponDatas[i].twoHanded) ? hands[(i + 1) % 2] : hands[i % 2];
            Weapons[i].Init(WeaponDatas[i], i, weapPos, MechCombat, animator);
        }

        if (buildLocally) UpdateAnimatorState();

        //Enable renderers
        for(int i = 0; i < Weapons.Length; i++) {
            Weapons[i].ActivateWeapon( (i == weaponOffset || i == weaponOffset +1) );
        }
    }

    private void buildSkills(int[] skill_IDs) {
        if (skill_IDs == null) {Debug.Log("skill_IDs is null");return;}
        SkillConfig[] skills = new SkillConfig[4];
        for (int i = 0; i < skill_IDs.Length; i++) {
            skills[i] = SkillManager.GetSkillConfig(skill_IDs[i]);
        }

        if (SkillController != null) SkillController.SetSkills(skills);
    }

    public void EquipWeapon(string weapon, int pos) {
        WeaponData data = WeaponDataManager.FindData(weapon);
        if (data == null) {Debug.LogError("Can't find weapon data : " + weapon);return;}

        //if previous is two-handed => also destroy left hand
        if (pos == 3) {
            if (Weapons[2] != null && WeaponDatas[2].twoHanded) {
                if (isDataGetSaved) UserData.myData.Mech[Mech_Num].Weapon2L = "Empty";

                Weapons[2].OnDestroy();
                Weapons[2] = null;
                WeaponDatas[2] = null;
            }
        } else if (pos == 1) {
            if (Weapons[0] != null && WeaponDatas[0].twoHanded) {
                if (isDataGetSaved) UserData.myData.Mech[Mech_Num].Weapon1L = "Empty";

                Weapons[0].OnDestroy();
                Weapons[0] = null;
                WeaponDatas[0] = null;
            }
        }

        //if the new one is two-handed => also destroy right hand
        if (data.twoHanded) {
            if (Weapons[pos + 1] != null) {
                Weapons[pos + 1].OnDestroy();
                Weapons[pos + 1] = null;
            }

            if (pos == 0) {
                if (isDataGetSaved)UserData.myData.Mech[Mech_Num].Weapon1R = "Empty";
                WeaponDatas[1] = null;
            } else if (pos == 2) {
                if (isDataGetSaved)UserData.myData.Mech[Mech_Num].Weapon2R = "Empty";
                WeaponDatas[3] = null;
            }
        }

        //destroy the current weapon on the hand position
        if (Weapons[pos] != null) {
            Weapons[pos].OnDestroy();
            Weapons[pos] = null;
        }

        //Init
        WeaponDatas[pos] = data;
        Weapons[pos] = (Weapon)(WeaponDatas[pos].GetWeaponObject());
        Transform weapPos = (WeaponDatas[pos].twoHanded) ? hands[(pos + 1) % 2] : hands[pos % 2];
        Weapons[pos].Init(WeaponDatas[pos], pos % 2, weapPos, MechCombat, animator);

        Weapons[pos].ActivateWeapon(pos == weaponOffset || pos == weaponOffset + 1);

        UpdateAnimatorState();

        if (buildLocally) {
            MechIK.UpdateMechIK(weaponOffset);
        } else {
            UpdateMechCombatVars();
        }

        //Display properties
        if (!onPanel && OperatorStatsUI != null) {//!onPanel : hargar,lobby,store
            OperatorStatsUI.DisplayMechProperties();
        }
    }

    public void DisplayWeapons(int weaponOffset) {
        this.weaponOffset = weaponOffset;
        for (int i = 0; i < 4; i++) if (Weapons[i] != null) Weapons[i].ActivateWeapon( i == weaponOffset || i == weaponOffset+1 );

        if (!onPanel && OperatorStatsUI != null) {//!onPanel : hargar,lobby,store
            OperatorStatsUI.DisplayMechProperties();
        }
    }

    public int GetWeaponOffset() {
        return weaponOffset;
    }

    public void UpdateAnimatorState() {
        if (animator == null) { Debug.LogError("Animator is null"); return; };

        MovementClips movementClips = (WeaponDatas[weaponOffset]!= null && WeaponDatas[weaponOffset].twoHanded) ? TwoHandedMovementClips : defaultMovementClips;
        for (int i = 0; i < movementClips.clips.Length; i++) {
            clipOverrides[movementClips.clipnames[i]] = movementClips.clips[i];
        }
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    private void SetMechDefaultIfEmpty(int mech_num) {
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Core))UserData.myData.Mech[mech_num].Core = defaultParts[0];    
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Arms))UserData.myData.Mech[mech_num].Arms = defaultParts[1];
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Legs))UserData.myData.Mech[mech_num].Legs = defaultParts[2];
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Head))UserData.myData.Mech[mech_num].Head = defaultParts[3];
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Booster))UserData.myData.Mech[mech_num].Booster = defaultParts[4];
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Weapon1L))UserData.myData.Mech[mech_num].Weapon1L = defaultParts[5];
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Weapon1R))UserData.myData.Mech[mech_num].Weapon1R = defaultParts[5];
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Weapon2L))UserData.myData.Mech[mech_num].Weapon2L = defaultParts[5];
        if (string.IsNullOrEmpty(UserData.myData.Mech[mech_num].Weapon2R)) UserData.myData.Mech[mech_num].Weapon2R = defaultParts[11];
        if (UserData.myData.Mech[mech_num].skillIDs == null)UserData.myData.Mech[mech_num].skillIDs = new int[4] { -1, -1, -1, -1 };
    }

    public void SetMechNum(int num) {
        Mech_Num = num;
    }

    private void UpdateMechCombatVars() {
        if (MechCombat == null) return;

        if (OnMechBuilt != null) OnMechBuilt();
        if (MechCombat.OnWeaponSwitched != null) MechCombat.OnWeaponSwitched();

        MechCombat.EnableAllRenderers(true);//TODO : check this
        MechCombat.EnableAllColliders(true);
    }

    private void CheckIfBuildLocally() {
        buildLocally = (SceneStateController.ActiveScene == HangarManager._sceneName ||
            SceneStateController.ActiveScene == LobbyManager._sceneName || SceneStateController.ActiveScene == StoreManager._sceneName || onPanel);
    }

    private void CheckIsDataGetSaved() {
        isDataGetSaved = (SceneManagerHelper.ActiveSceneName != StoreManager._sceneName);
    }

    private void OnPhotonInstantiate(PhotonMessageInfo info) {
        info.sender.TagObject = gameObject;
    }
}