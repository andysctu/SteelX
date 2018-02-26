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
	public string[] Parts;

	Mech(int u, string a, string l, string c, string h, string w1l, string w1r, string w2l, string w2r, string b, bool p) {
		Uid = u;
		Arms = a;
		Legs = l;
		Core = c;
		Head = h;
		Weapon1L = w1l;
		Weapon1R = w1r;
		Weapon2L = w2l;
		Weapon2R = w2r;
		Booster = b;
		isPrimary = p;
		Parts = new string[] {a,l,c,h,w1l,w1r,w2l,w2r,b};
	}

	public void PopulateParts() {
		Parts = new string[] {Arms,Legs,Core,Head,Weapon1L,Weapon1R,Weapon2L,Weapon2R,Booster};
	}
}

[System.Serializable]
public struct Data {
	public Mech[] Mech;
	public User User;
	public string[] Owns;
}
