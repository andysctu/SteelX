using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MechPartSheetImplement : MonoBehaviour {

    MechPartSheet MechPartSheet;

    void Start() {
        //Load 
        MechPartSheet = Resources.Load<MechPartSheet>("MechPartSheet");

        if (MechPartSheet == null) {
            Debug.Log("Can't find MechPartSheet.");
            return;
        }
        //Load assets to Mech Part manager
        MechPartManager MechPartManager = AssetDatabase.LoadAssetAtPath(AssetDatabase.FindAssets("MechPartManager" + "  t:MechPartManager")[0], typeof(MechPartManager)) as MechPartManager;
        if(MechPartManager == null) {
            Debug.LogError("MechPartManager is null");
            return;
        }

        ImplementData();


        
    }

	void ImplementData() {
        foreach(MechPartSheetData data in MechPartSheet.dataArray) {
            string dataName = data.Name.Replace(" ", "");//remove spaces in name


            string[] assets;
            switch (dataName[0]) {
                case 'H':
                assets = AssetDatabase.FindAssets(dataName + "  t:Head");
                Head head = null;
                if(assets.Length == 0) {//asset does not exist
                    //creat one
                    head = ScriptableObject.CreateInstance(typeof(Head)) as Head;
                    AssetDatabase.CreateAsset(head, "Assets/Data/MechParts/Head/" + dataName + ".asset");
                } else {//override
                    head = AssetDatabase.LoadAssetAtPath(assets[0], typeof(Head)) as Head;
                }
                
                head.displayName = data.Displayname;
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
                    arm = AssetDatabase.LoadAssetAtPath(assets[0], typeof(Arm)) as Arm;
                }

                arm.displayName = data.Displayname;
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
                    leg = AssetDatabase.LoadAssetAtPath(assets[0], typeof(Leg)) as Leg;
                }

                leg.displayName = data.Displayname;
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
                    core = AssetDatabase.LoadAssetAtPath(assets[0], typeof(Core)) as Core;
                }

                core.displayName = data.Displayname;
                core.EN = (data.EN == 0) ? 3000 : data.EN;
                core.ENOutputRate = (data.En_output == 0) ? 900 : data.En_output;
                core.MinENRequired = (data.Minenrequired == 0) ? 700 : data.Minenrequired;
                core.HP = (data.HP == 0) ? 1200 : data.HP;
                core.Weight = (data.Weight == 0) ? 24000 : data.Weight;
                core.EnergyDrain = (data.Endrain == 0) ? 150 : data.Endrain;
                core.Size = (data.Size == 0) ? 5000 : data.Size;
                break;

                case 'P':
                assets = AssetDatabase.FindAssets(dataName + "  t:Booster");
                Booster booster = null;
                if (assets.Length == 0) {//asset does not exist
                    //creat one
                    booster = ScriptableObject.CreateInstance(typeof(Booster)) as Booster;
                    AssetDatabase.CreateAsset(booster, "Assets/Data/MechParts/Booster/" + dataName + ".asset");
                } else {//override
                    booster = AssetDatabase.LoadAssetAtPath(assets[0], typeof(Booster)) as Booster;
                }

                booster.displayName = data.Displayname;
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

}
