using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class MechPartManager : ScriptableObject {

    public Head[] Heads;
    public Core[] Cores;
    public Arm[] Arms;
    public Leg[] Legs;
    public Booster[] Boosters;

    public Part FindData(string name) {
        if(name == null) {
            Debug.LogError("empty name");
            return null;
        }
            
        switch (name[0]) {
            case 'H':
            return SearchInArray(Heads, name);
            case 'C':
            return SearchInArray(Cores, name);
            case 'A':
            return SearchInArray(Arms, name);
            case 'L':
            return SearchInArray(Legs, name);
            case 'P':
            return SearchInArray(Boosters, name);
            default:
            Debug.LogError("Can find data : " + name);
            return null;
        }
    }

    public System.Type GetPartType(string name) {
        switch (name[0]) {
            case 'H':
            return typeof(Head);
            case 'C':
            return typeof(Core);
            case 'A':
            return typeof(Arm);
            case 'L':
            return typeof(Leg);
            case 'P':
            return typeof(Booster);
            default:
            Debug.LogError("Can catarize : " + name);
            return null;
        }
    }

    Part SearchInArray(Part[] parts, string name) {
        for (int i = 0; i < parts.Length; i++) {
            if (parts[i]!= null && parts[i].GetPartPrefab()!=null &&parts[i].GetPartPrefab().name == name) {
                return parts[i];
            }
        }
        Debug.LogError("Can't find prefab : " + name);
        return null;
    }

    public Part[] GetAllParts() {
        int length = Heads.Length + Cores.Length + Arms.Length + Legs.Length + Boosters.Length;
        Part[] Parts = new Part[length];

        List<Part[]> part_list = new List<Part[]>();
        part_list.Add(Heads);
        part_list.Add(Cores);
        part_list.Add(Arms);
        part_list.Add(Legs);
        part_list.Add(Boosters);

        int i = 0;
        foreach (Part[] w in part_list) {
            foreach (Part w2 in w) {
                Parts[i++] = w2;
            }
        }

        return Parts;
    }
}