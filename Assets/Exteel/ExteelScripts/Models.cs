using System.Collections;

[System.Serializable]
public struct User {
	public int Uid;
	public string Username;
	public string PilotName;
	public int Level;
	public string Rank;
	public int Credits;
}

[System.Serializable]
public struct Mech {
	public int Uid;
	public string Arms;
	public string Legs;
	public string Core;
	public string Head;
	public string Weapon1L;
	public string Weapon1R;
	public string Weapon2L;
	public string Weapon2R;
	public string Booster;
	public bool isPrimary;
}

[System.Serializable]
public struct Data {
	public Mech Mech;
	public User User;
	public string[] Owns;
}
