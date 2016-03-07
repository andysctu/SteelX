using System.Collections;

public struct Score {
	public int Kills, Deaths;
	public int IncrKill() { return ++Kills; }
	public int IncrDeaths() { return ++Deaths; }
}