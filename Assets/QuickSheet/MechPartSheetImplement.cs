using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MechPartSheetImplement : MonoBehaviour {
    static MechPartSheet MechPartSheet;

    [MenuItem("LoadData/LoadMechData")]
    static void LoadData() {
        //Load 
        MechPartSheet = Resources.Load<MechPartSheet>("MechPartSheet");

        if (MechPartSheet == null) {
            Debug.Log("Can't find MechPartSheet.");
            return;
        }
        //Load assets to Mech Part manager
        var guid = AssetDatabase.FindAssets("MechPartManager" + "  t:MechPartManager")[0];
        MechPartManager MechPartManager = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(MechPartManager)) as MechPartManager;

        if(MechPartManager == null) {
            Debug.LogError("MechPartManager is null");
            return;
        }
        
        ImplementData();

        //load heads to manager
        string[] path = new string[1] { "Assets/Data/MechParts/Head"};
        string[] heads_GUID = AssetDatabase.FindAssets("t:Head", path);
        MechPartManager.Heads = new Head[heads_GUID.Length];

        for(int i=0;i< heads_GUID.Length;i++) {
            MechPartManager.Heads[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(heads_GUID[i]), typeof(Head)) as Head;
        }

        //load Arms to manager
        path = new string[1] { "Assets/Data/MechParts/Arm" };
        string[] arms_GUID = AssetDatabase.FindAssets("t:Arm", path);
        MechPartManager.Arms = new Arm[heads_GUID.Length];

        for (int i = 0; i < arms_GUID.Length; i++) {
            MechPartManager.Arms[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(arms_GUID[i]), typeof(Arm)) as Arm;
        }

        //load Cores to manager
        path = new string[1] { "Assets/Data/MechParts/Core" };
        string[] cores_GUID = AssetDatabase.FindAssets("t:Core", path);
        MechPartManager.Cores = new Core[cores_GUID.Length];

        for (int i = 0; i < cores_GUID.Length; i++) {
            MechPartManager.Cores[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(cores_GUID[i]), typeof(Core)) as Core;
        }

        //load Legs to manager
        path = new string[1] { "Assets/Data/MechParts/Leg" };
        string[] legs_GUID = AssetDatabase.FindAssets("t:Leg", path);
        MechPartManager.Legs = new Leg[legs_GUID.Length];

        for (int i = 0; i < legs_GUID.Length; i++) {
            MechPartManager.Legs[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(legs_GUID[i]), typeof(Leg)) as Leg;
        }

        AssetDatabase.SaveAssets();
        //load Boosters to manager
        //not implemented
    }

	static void ImplementData() {
        foreach(MechPartSheetData data in MechPartSheet.dataArray) {
            string dataName = data.Name.Replace(" ", "");//remove spaces in name

            string[] assets, tempassets, folderPath;
            string partPrefabGUID = "";
            switch (dataName[0]) {
                case 'H':             
                assets = AssetDatabase.FindAssets(dataName + "  t:Head");
                Head head = null;
                if(assets.Length == 0) {//asset does not exist
                    //creat one
                    head = ScriptableObject.CreateInstance(typeof(Head)) as Head;
                    AssetDatabase.CreateAsset(head, "Assets/Data/MechParts/Head/" + dataName + ".asset");
                } else {//override
                    head = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(Head)) as Head;
                    if (head == null) {
                        Debug.LogError("head is null");
                        continue;
                    }
                }

                folderPath = new string[1]{"Assets/Exteel/Prefabs/MechParts/Heads"};
                tempassets = AssetDatabase.FindAssets(dataName, folderPath);          
                if(tempassets.Length == 0) {
                    Debug.LogError("Can't find prefab : "+dataName);
                    continue;
                } else {
                    partPrefabGUID = tempassets[0];
                    head.part = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(partPrefabGUID), typeof(GameObject)) as GameObject;                    
                }

                head.displayName = data.Displayname.Replace(" ", "");
                head.SP = (data.SP == 0) ? 1500 : data.SP;
                head.MPU = (data.MPU == 0) ? 4 : data.MPU;
                head.ScanRange = (data.Scan == 0) ? 1500 : data.Scan;
                head.HP = (data.HP == 0) ? 200 : data.HP;
                head.Weight = (data.Weight == 0) ? 5000 : data.Weight;
                head.EnergyDrain = (data.Endrain == 0) ? 100 : data.Endrain;
                head.Size = (data.Size == 0)? 600 : data.Size;
                
                break;
                case 'A':
                assets = AssetDatabase.FindAssets(dataName + "  t:Arm");
                Arm arm = null;
                if (assets.Length == 0) {//asset does not exist
                    //creat one
                    arm = ScriptableObject.CreateInstance(typeof(Arm)) as Arm;
                    AssetDatabase.CreateAsset(arm, "Assets/Data/MechParts/Arm/" + dataName + ".asset");
                } else {//override
                    arm = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(Arm)) as Arm;
                    if (arm == null) {
                        Debug.LogError("arm is null");
                        continue;
                    }
                }

                folderPath = new string[1] { "Assets/Exteel/Prefabs/MechParts/Arms" };
                tempassets = AssetDatabase.FindAssets(dataName, folderPath);             
                if (tempassets.Length == 0) {
                    Debug.LogError("Can't find prefab : " + dataName);
                    continue;
                } else {
                    partPrefabGUID = tempassets[0];
                    arm.part = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(partPrefabGUID), typeof(GameObject)) as GameObject;
                }

                arm.displayName = data.Displayname.Replace(" ", "");
                arm.MaxHeat = (data.Maxheat == 0) ? 170 : data.Maxheat;
                arm.CooldownRate = (data.Cooldown == 0)? 40 : data.Cooldown;
                arm.Marksmanship = (data.Mark == 0)? 30 : data.Mark;
                arm.HP = (data.HP == 0) ? 600 : data.HP;
                arm.Weight = (data.Weight == 0) ? 15000 : data.Weight;
                arm.EnergyDrain = (data.Endrain == 0) ? 100 : data.Endrain;
                arm.Size = (data.Size == 0) ? 3500 : arm.Size;
                break;
                case 'L':
                assets = AssetDatabase.FindAssets(dataName + "  t:Leg");
                Leg leg = null;
                if (assets.Length == 0) {//asset does not exist
                    //creat one
                    leg = ScriptableObject.CreateInstance(typeof(Leg)) as Leg;
                    AssetDatabase.CreateAsset(leg, "Assets/Data/MechParts/Leg/" + dataName + ".asset");
                } else {//override
                    leg = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(Leg)) as Leg;
                    if (leg == null) {
                        Debug.LogError("leg is null");
                        continue;
                    }
                }
                folderPath = new string[1] { "Assets/Exteel/Prefabs/MechParts/Legs" };
                tempassets = AssetDatabase.FindAssets(dataName, folderPath);
                if (tempassets.Length == 0) {
                    Debug.LogError("Can't find prefab : " + dataName);
                    continue;
                } else {
                    partPrefabGUID = tempassets[0];
                    leg.part = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(partPrefabGUID), typeof(GameObject)) as GameObject;
                }

                leg.displayName = data.Displayname.Replace(" ", "");
                leg.BasicSpeed = (data.Basicspeed == 0) ? 600 : data.Basicspeed;
                leg.Capacity = (data.Capacity == 0) ? 195000 : data.Capacity;
                leg.Deceleration = (data.Deceleration == 0) ? 80000 : data.Deceleration;
                leg.HP = (data.HP == 0) ? 750 : data.HP;
                leg.Weight = (data.Weight == 0) ? 20000 : data.Weight;
                leg.EnergyDrain = (data.Endrain == 0) ? 50 : data.Endrain;
                leg.Size = (data.Size == 0) ? 5000 : data.Size;
                break;
                case 'C':
                assets = AssetDatabase.FindAssets(dataName + "  t:Core");
                Core core = null;
                if (assets.Length == 0) {//asset does not exist
                    //creat one
                    core = ScriptableObject.CreateInstance(typeof(Core)) as Core;
                    AssetDatabase.CreateAsset(core, "Assets/Data/MechParts/Core/" + dataName + ".asset");
                } else {//override
                    core = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(Core)) as Core;
                    if (core == null) {
                        Debug.LogError("core is null");
                        continue;
                    }
                }
                folderPath = new string[1] { "Assets/Exteel/Prefabs/MechParts/Cores" };
                tempassets = AssetDatabase.FindAssets(dataName, folderPath);
                if (tempassets.Length == 0) {
                    Debug.LogError("Can't find prefab : " + dataName);
                    continue;
                } else {
                    partPrefabGUID = tempassets[0];
                    core.part = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(partPrefabGUID), typeof(GameObject)) as GameObject;
                }

                core.displayName = data.Displayname.Replace(" ", "");
                core.EN = (data.EN == 0) ? 3000 : data.EN;
                core.ENOutputRate = (data.En_output == 0) ? 900 : data.En_output;
                core.MinENRequired = (data.Minenrequired == 0) ? 700 : data.Minenrequired;
                core.HP = (data.HP == 0) ? 1200 : data.HP;
                core.Weight = (data.Weight == 0) ? 24000 : data.Weight;
                core.EnergyDrain = (data.Endrain == 0) ? 150 : data.Endrain;
                core.Size = (data.Size == 0) ? 5000 : data.Size;
                break;

                case 'P':

                    continue;

                //boosters not import;

                assets = AssetDatabase.FindAssets(dataName + "  t:Booster");
                Booster booster = null;
                if (assets.Length == 0) {//asset does not exist
                    //creat one
                    booster = ScriptableObject.CreateInstance(typeof(Booster)) as Booster;
                    AssetDatabase.CreateAsset(booster, "Assets/Data/MechParts/Booster/" + dataName + ".asset");
                } else {//override
                    booster = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(Booster)) as Booster;
                    if (booster == null) {
                        Debug.LogError("booster is null");
                        return;
                    }
                }
                partPrefabGUID = AssetDatabase.FindAssets(dataName + " t:GameObject")[0];
                if (partPrefabGUID == null) {
                    Debug.LogError("Can't find prefab : " + dataName);
                    continue;
                } else {
                    booster.part = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(partPrefabGUID), typeof(GameObject)) as GameObject;
                }

                booster.displayName = data.Displayname.Replace(" ", "");
                booster.DashOutput = (data.Dashoutput == 0) ? 510 : data.Dashoutput;
                booster.DashENDrain = (data.Dashendrain == 0) ? 360 : data.Dashendrain;
                booster.JumpENDrain = (data.Jumpendrain == 0) ? 250 : data.Jumpendrain;
                booster.HP = (data.HP == 0) ? 80 : data.HP;
                booster.Weight = (data.Weight == 0) ? 5000 : data.Weight;
                booster.EnergyDrain = (data.Endrain == 0) ? 50 : data.Endrain;
                booster.Size = (data.Size == 0) ? 1300 : data.Size;
                break;
            }
        }
    }

    [MenuItem("QuickPrefab/CreatePrefabFromSelection")]
    static void DoCreateSimplePrefab() {
        Transform[] transforms = Selection.transforms;
        foreach (Transform t in transforms) {
            Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/Exteel/Prefabs/MechParts/Legs/" + t.gameObject.name + ".prefab");
            PrefabUtility.ReplacePrefab(t.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }
    }

    [MenuItem("LoadData/MaterialName")]
    static void ChangeAllMaterialNmae() {
        string[] folderToSearch = new string[1] {"Assets/Exteel/Prefabs/Resources/MechPartMaterials"};
        string[] material_guid = AssetDatabase.FindAssets("t:Material", folderToSearch);

        foreach(string guid in material_guid) {
            Material mat = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Material)) as Material;

            if (mat.name.Contains("mat")) {
                continue;
            } else {
                string newName = mat.name + mat;
                AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(guid) , newName);
            }
        }
    }

}
