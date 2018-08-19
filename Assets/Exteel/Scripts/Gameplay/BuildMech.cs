using System.Collections.Generic;
using UnityEngine;
using XftWeapon;

public class BuildMech : Photon.MonoBehaviour {
    private string[] defaultParts = { "CES301", "AES104", "LTN411", "HDS003", "PBS016", "SHL009", "SHL501", "APS043", "SHS309", "RCL034", "BCN029", "BRF025", "SGN150", "LMG012", "ENG041", "ADR000", "Empty" };
                                                                                                                                                                                //eng : 14
    [SerializeField] private Transform RootBone;
    [SerializeField] private MechCombat MechCombat;
    [SerializeField] private MechController MechController;
    [SerializeField] private Sounds Sounds;
    [SerializeField] private MechIK MechIK;
    [SerializeField] private WeaponDataManager WeaponManager;
    [SerializeField] private SkillManager SkillManager;
    [SerializeField] private MechPartManager MechPartManager;
    [SerializeField] private MovementClips defaultMovementClips, TwoHandedMovementClips;
    [SerializeField] private SkillController SkillController;

    [HideInInspector] public AudioClip[] ShotSounds, ReloadSounds;
    [HideInInspector] public WeaponData[] weaponScripts;
    [HideInInspector] public string[] curWeaponNames = new string[4];
    [HideInInspector] public GameObject[] weapons;
    [HideInInspector] public GameObject[] bulletPrefabs;

    private GameManager gm;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;

    private Transform shoulderL, shoulderR;
    private Transform[] hands;
    public Part[] curMechParts = new Part[5];
    private int weaponOffset = 0;

    private bool buildLocally = false, isDataGetSaved = true;
    private int Total_Mech = 4;
    private const int BLUE = 0, RED = 1;
    public MechProperty MechProperty;
    public int Mech_Num = 0;
    public bool onPanel = false;

    public delegate void BuildWeaponAction();
    public event BuildWeaponAction OnMechBuilt;

    private void Awake() {
        //For not starting from login
        if (UserData.myData.Mech == null) {
            Debug.Log("Not starting from login -> Init Mech data");
            UserData.myData.Mech0 = new Mech();
            UserData.myData.Mech = new Mech[4];
            UserData.data = new Dictionary<int, Data>();
        }
    }

    private void Start() {
        InitComponents();
        CheckIfBuildLocally();
        CheckIsDataGetSaved();

        // If this is not me, don't build this mech. Someone else will RPC build it
        if (!buildLocally && !photonView.isMine) return;

        InitMechData();
        InitAnimatorControllers();

        if (buildLocally) {
            buildMech(UserData.myData.Mech[0]);
        } else { // Register my name on all clients
            photonView.RPC("SetName", PhotonTargets.AllBuffered, PhotonNetwork.playerName);
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

    private void InitComponents() {
        animator = transform.Find("CurrentMech").GetComponent<Animator>();
    }

    private void InitAnimatorControllers() {//do not call this in game otherwise mechcombat gets null parameter
        if (!buildLocally) return;

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);//write clips into clipOverrides
    }

    [PunRPC]
    private void SetName(string name) {//TODO : consider not putting here
        gameObject.name = name;
        FindGameManager();
        gm.RegisterPlayer(photonView.viewID);
    }

    public void Build(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r, int[] skillIDs) {
        photonView.RPC("buildMech", PhotonTargets.AllBuffered, c, a, l, h, b, w1l, w1r, w2l, w2r, skillIDs);
    }

    private void findHands() {
        shoulderL = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_L_Clavicle");
        shoulderR = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_R_Clavicle");

        hands = new Transform[2];
        hands[0] = shoulderL.Find("Bip01_L_UpperArm/Bip01_L_ForeArm/Bip01_L_Hand/Weapon_lft_Bone");
        hands[1] = shoulderR.Find("Bip01_R_UpperArm/Bip01_R_ForeArm/Bip01_R_Hand/Weapon_rt_Bone");
    }

    private void buildMech(Mech m) {
        buildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
    }

    [PunRPC]
    public void buildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r, int[] skill_IDs) {
        findHands();
        string[] parts = new string[9] { c, a, l, h, b, w1l, w1r, w2l, w2r };

        for (int i = 0; i < parts.Length - 4; i++) {            
            parts[i] = string.IsNullOrEmpty(parts[i]) ? defaultParts[i] : parts[i];
        }

        //set weapons if null (in offline)
        if (string.IsNullOrEmpty(parts[5])) parts[5] = defaultParts[15];
        if (string.IsNullOrEmpty(parts[6])) parts[6] = defaultParts[6];
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
                Debug.LogError("Can't find part in MechPartManager");
                continue;
            }
            // Extract Skinned Mesh
            newSMR[i] = part.GetPartPrefab().GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
        }

        // Replace all
        SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < 4; i++) {//TODO : improve this so the order does not matter
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
        buildWeapons(new string[4] { parts[5], parts[6], parts[7], parts[8] });

        if (!buildLocally) {
            UpdateMechCombatVars();//this will turn trail on ( enable all renderer)
            for (int i = 0; i < 4; i++)//turn off trail
                ShutDownTrail(weapons[i]);
        } else {
            //display properties
            if (!onPanel) {//hargar,lobby,store
                OperatorStatsUI OperatorStatsUI = FindObjectOfType<OperatorStatsUI>();
                if (OperatorStatsUI != null) {
                    OperatorStatsUI.DisplayMechProperties();
                }
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
        if (boosterbone == null) return;

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

    private void buildWeapons(string[] weaponNames) {
        if (weapons != null) for (int i = 0; i < weapons.Length; i++) if (weapons[i] != null) Destroy(weapons[i]);
        weapons = new GameObject[4];
        weaponScripts = new WeaponData[4];
        bulletPrefabs = new GameObject[4];
        ShotSounds = new AudioClip[4];
        ReloadSounds = new AudioClip[4];
        for (int i = 0; i < weaponNames.Length; i++) {
            weaponScripts[i] = (weaponNames[i] == "Empty") ? null : WeaponManager.FindData(weaponNames[i]);

            if (weaponScripts[i] == null) {
                if (weaponNames[i] != "Empty")
                    Debug.LogError("Can't find weapon data : " + weaponNames[i]);

                continue;
            }

            weapons[i] = Instantiate(weaponScripts[i].GetWeaponPrefab(i % 2), Vector3.zero, Quaternion.identity) as GameObject;

            //TODO : remake this
            float newscale = transform.root.localScale.x * transform.localScale.x;
            weapons[i].transform.localScale = new Vector3(weapons[i].transform.localScale.x * newscale,
            weapons[i].transform.localScale.y * newscale, weapons[i].transform.localScale.z * newscale);            

            if (weaponScripts[i].twoHanded) {
                weapons[i].transform.SetParent(hands[(i + 1) % 2]);
                weapons[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
            } else {
                weapons[i].transform.SetParent(hands[i % 2]);
                weapons[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
            }

            weapons[i].transform.localPosition = Vector3.zero;

            switch (weaponScripts[i].weaponType) {//TODO : remake this part
                case "Sword":
                bulletPrefabs[i] = null;
                Sounds.LoadSlashClips(i, ((Sword)weaponScripts[i]).slash_sound);
                Sounds.LoadSlashOnHitClips(i, ((Sword)weaponScripts[i]).slash_hit_sound);
                ShutDownTrail(weapons[i]);
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
                break;
                default://other ranged weapon
                bulletPrefabs[i] = ((RangedWeapon)weaponScripts[i]).bulletPrefab;
                ShotSounds[i] = ((RangedWeapon)weaponScripts[i]).shoot_sound;
                ReloadSounds[i] = ((RangedWeapon)weaponScripts[i]).reload_sound;
                break;
            }

            //switch weapon animation clips
            if (weapons[i].GetComponent<Animator>() != null && weapons[i].GetComponent<Animator>().runtimeAnimatorController != null) {//TODO : improve this
                AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(weapons[i].GetComponent<Animator>().runtimeAnimatorController);
                weapons[i].GetComponent<Animator>().runtimeAnimatorController = animatorOverrideController;

                weaponScripts[i].SwitchAnimationClips(weapons[i].GetComponent<Animator>());
            }
        }

        Sounds.LoadReloadClips(ReloadSounds);
        Sounds.LoadShotClips(ShotSounds);

        UpdateCurWeaponNames();

        if (buildLocally) CheckAnimatorState();

        //shut down renderers ( not using setActive because if weapons have their own animations , disabling causes weapon animators to rebind the wrong rotation & position
        if (weapons[(weaponOffset + 2) % 4] != null) {
            Renderer[] renderers = weapons[(weaponOffset + 2) % 4].GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = false;
            }
        }
        if (weapons[(weaponOffset + 3) % 4] != null) {
            Renderer[] renderers = weapons[(weaponOffset + 3) % 4].GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = false;
            }
        }
    }

    private void buildSkills(int[] skill_IDs) {
        if (skill_IDs == null) {
            Debug.Log("skill_IDs is null");
            return;
        }
        SkillConfig[] skills = new SkillConfig[4];
        for (int i = 0; i < skill_IDs.Length; i++) {
            skills[i] = SkillManager.GetSkillConfig(skill_IDs[i]);
        }

        //= null in hangar
        if (SkillController != null) SkillController.SetSkills(skills);
    }

    public void EquipWeapon(string weapon, int weapPos) {
        WeaponData newWeapon = WeaponManager.FindData(weapon);
        if (newWeapon == null) {
            Debug.LogError("Can't find weapon data : " + weapon);
            return;
        }

        //if previous is two-handed => also destroy left hand
        if (weapPos == 3) {
            if (weapons[2] != null) {
                if (weaponScripts[2].twoHanded) {
                    if (isDataGetSaved) UserData.myData.Mech[Mech_Num].Weapon2L = "Empty";

                    Destroy(weapons[2]);
                    weaponScripts[2] = null;
                }
            }
        } else if (weapPos == 1) {
            if (weapons[0] != null) {
                if (weaponScripts[0].twoHanded) {
                    if (isDataGetSaved) UserData.myData.Mech[Mech_Num].Weapon1L = "Empty";

                    Destroy(weapons[0]);
                    weaponScripts[0] = null;
                }
            }
        }

        //if the new one is two-handed => also destroy right hand
        if (newWeapon.twoHanded) {
            if (weapons[weapPos + 1] != null)
                Destroy(weapons[weapPos + 1]);
            if (weapPos == 0) {
                if (isDataGetSaved)
                    UserData.myData.Mech[Mech_Num].Weapon1R = "Empty";
                weaponScripts[1] = null;
            } else if (weapPos == 2) {
                if (isDataGetSaved) {
                    UserData.myData.Mech[Mech_Num].Weapon2R = "Empty";
                }
                weaponScripts[3] = null;
            }
        }

        //destroy the current weapon on the hand position
        if (weapons[weapPos] != null)
            Destroy(weapons[weapPos]);

        switch (newWeapon.weaponType) {
            case "Cannon":
            case "Rocket":
            weapPos = (weapPos >= 2) ? 2 : 0; //script is on left hand

            weapons[weapPos + 1] = null;
            if (weapPos >= 2) {
                if (isDataGetSaved)
                    UserData.myData.Mech[Mech_Num].Weapon2R = "Empty";
                weaponScripts[3] = null;
            } else {
                if (isDataGetSaved)
                    UserData.myData.Mech[Mech_Num].Weapon1R = "Empty";
                weaponScripts[1] = null;
            }

            weapons[weapPos] = Instantiate(newWeapon.GetWeaponPrefab(weapPos % 2), Vector3.zero, Quaternion.identity) as GameObject;

            float newscale = transform.root.localScale.x * transform.localScale.x;
            weapons[weapPos].transform.localScale = new Vector3(weapons[weapPos].transform.localScale.x * newscale,
            weapons[weapPos].transform.localScale.y * newscale, weapons[weapPos].transform.localScale.z * newscale);

            weapons[weapPos].transform.SetParent(hands[(weapPos + 1) % 2]);
            weapons[weapPos].transform.localPosition = Vector3.zero;
            weapons[weapPos].transform.localRotation = Quaternion.Euler(90, 0, 0);
            break;
            default:
            weapons[weapPos] = Instantiate(newWeapon.GetWeaponPrefab(weapPos % 2), Vector3.zero, Quaternion.identity) as GameObject;

            float newscale_2 = transform.root.localScale.x * transform.localScale.x;
            weapons[weapPos].transform.localScale = new Vector3(weapons[weapPos].transform.localScale.x * newscale_2,
            weapons[weapPos].transform.localScale.y * newscale_2, weapons[weapPos].transform.localScale.z * newscale_2);

            weapons[weapPos].transform.SetParent(hands[weapPos % 2]);
            weapons[weapPos].transform.localPosition = Vector3.zero;
            weapons[weapPos].transform.localRotation = Quaternion.Euler(90, 0, 0);
            break;
        }

        //replace the script
        weaponScripts[weapPos] = newWeapon;
        UpdateCurWeaponNames();
        weapons[weapPos].SetActive(weapPos == weaponOffset || weapPos == weaponOffset + 1);

        CheckAnimatorState();

        if (buildLocally) {
            MechIK.UpdateMechIK(weaponOffset);
        } else {
            UpdateMechCombatVars();
        }
        ShutDownTrail(weapons[weapPos]);

        //display properties
        if (!onPanel) {//hargar,lobby,store
            OperatorStatsUI OperatorStatsUI = FindObjectOfType<OperatorStatsUI>();
            if (OperatorStatsUI != null) {
                OperatorStatsUI.DisplayMechProperties();
            }
        }
    }

    private void FindGameManager() {
        if (gm == null) {
            GameObject GameManager_go = GameObject.Find("GameManager");
            if (GameManager_go == null) {//TODO : remake this , this happens when game end
                DestroyImmediate(this.gameObject);
            }else
                gm = GameManager_go.GetComponent<GameManager>();
            
        }
    }

    public void DisplayFirstWeapons() {
        weaponOffset = 0;
        for (int i = 0; i < 4; i++) if (weaponScripts[i] != null) EquipWeapon(weaponScripts[i].GetWeaponPrefab(i % 2).name, i);
    }

    public void DisplaySecondWeapons() {
        weaponOffset = 2;
        for (int i = 0; i < 4; i++) if (weaponScripts[i] != null) EquipWeapon(weaponScripts[i].GetWeaponPrefab(i % 2).name, i);
    }

    public int GetWeaponOffset() {
        return weaponOffset;
    }

    public void CheckAnimatorState() {
        if (animator == null) { Debug.LogError("Animator is null"); return; };

        MovementClips movementClips = (weaponScripts[weaponOffset]!= null && weaponScripts[weaponOffset].twoHanded) ? TwoHandedMovementClips : defaultMovementClips;
        for (int i = 0; i < movementClips.clips.Length; i++) {
            clipOverrides[movementClips.clipnames[i]] = movementClips.clips[i];
        }
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    private void SetMechDefaultIfEmpty(int mehc_num) {
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Core)) {
            UserData.myData.Mech[mehc_num].Core = defaultParts[0];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Arms)) {
            UserData.myData.Mech[mehc_num].Arms = defaultParts[1];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Legs)) {
            UserData.myData.Mech[mehc_num].Legs = defaultParts[2];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Head)) {
            UserData.myData.Mech[mehc_num].Head = defaultParts[3];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Booster)) {
            UserData.myData.Mech[mehc_num].Booster = defaultParts[4];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon1L)) {
            UserData.myData.Mech[mehc_num].Weapon1L = defaultParts[5];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon1R)) {
            UserData.myData.Mech[mehc_num].Weapon1R = defaultParts[5];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon2L)) {
            UserData.myData.Mech[mehc_num].Weapon2L = defaultParts[5];
        }
        if (string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon2R)) {
            UserData.myData.Mech[mehc_num].Weapon2R = defaultParts[11];
        }
        if (UserData.myData.Mech[mehc_num].skillIDs == null) {
            UserData.myData.Mech[mehc_num].skillIDs = new int[4] { -1, -1, -1, -1 };
        }
    }

    private void UpdateCurWeaponNames() {
        for (int i = 0; i < 4; i++) {
            if (weaponScripts[i] != null)
                curWeaponNames[i] = weaponScripts[i].GetWeaponName();
        }
    }

    public void SetMechNum(int num) {
        Mech_Num = num;
    }

    private void ShutDownTrail(GameObject weapon) {
        if (weapon == null) return;

        Transform trail_transform = weapon.transform.Find("trail");
        XWeaponTrail trail = (trail_transform == null) ? null : trail_transform.GetComponent<XWeaponTrail>();

        if (trail != null) trail.Deactivate();
    }

    private void UpdateMechCombatVars() {
        if (MechCombat == null) return;

        if (OnMechBuilt != null) OnMechBuilt();
        if (MechCombat.OnWeaponSwitched != null) MechCombat.OnWeaponSwitched();

        MechCombat.EnableAllRenderers(true);
        MechCombat.EnableAllColliders(true);
    }

    private void CheckIfBuildLocally() {
        buildLocally = (SceneStateController.ActiveScene == HangarManager._sceneName ||
            SceneStateController.ActiveScene == LobbyManager._sceneName || SceneStateController.ActiveScene == StoreManager._sceneName || onPanel);
    }

    private void CheckIsDataGetSaved() {
        isDataGetSaved = (SceneManagerHelper.ActiveSceneName != StoreManager._sceneName);
    }
}

[System.Serializable]
public struct MechProperty {
    public int HP, EN, SP, MPU;
    public int ENOutputRate;
    public int MinENRequired;
    public int Size, Weight;
    public int EnergyDrain;

    public int MaxHeat, CooldownRate;
    public int Marksmanship;

    public int ScanRange;

    public int VerticalBoostSpeed;
    public int BasicSpeed { set; private get; }
    public int Capacity;
    public int Deceleration;

    public int DashOutput { private get; set; }
    public int DashENDrain;
    public int JumpENDrain { private get; set; }

    private float DashAcceleration, DashDecelleration;

    public float GetJumpENDrain(int totalWeight) {
        return JumpENDrain + totalWeight / 160f;//TODO : improve this
    }

    public float GetDashSpeed(int totalWeight) {
        return DashOutput * 1.8f - totalWeight * 0.004f; //DashOutput * 1.8f : max speed  ;  0.004 weight coefficient
    }

    public float GetMoveSpeed(int partWeight, int weaponWeight) {
        int cal_capacity = (Capacity > 195000)? 195000 : Capacity;

        double x1 = 0.0001064 * cal_capacity + 190.2552f, x2 = -0.0000024659 * cal_capacity + 0.69024f;
        //Debug.Log("part weight : "+partWeight + " weapon Weight : "+weaponWeight);
        //Debug.Log("basic speed : "+BasicSpeed + " coeff x1 : "+x1+" , x2 : "+x2);
        return (float)(BasicSpeed - (partWeight * x2 + weaponWeight )/x1);        
    }

    public float GetDashAcceleration(int totalWeight) {
        return GetDashSpeed(totalWeight) / 100f - 1;
    }

    public float GetDashDecelleration(int totalWeight) {
        return Deceleration / 10000f - (totalWeight - Deceleration) / 20000f;
    }
}