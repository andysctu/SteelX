using Newtonsoft.Json;

public struct Score {
    [JsonProperty(PropertyName = "kills")]
    public int Kills;

    [JsonProperty(PropertyName = "deaths")]
    public int Deaths;

    [JsonProperty(PropertyName = "assists")]
    public int Assists;

    [JsonProperty(PropertyName = "team")]
    public string Team;

	public int IncrKill() { return ++Kills; }
	public int IncrDeaths() { return ++Deaths; }
}