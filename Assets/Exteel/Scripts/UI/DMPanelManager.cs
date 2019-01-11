using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DMPanelManager : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] private GameObject PlayerStat;
    [SerializeField] private GameObject Panel, ScorePanel;
    [SerializeField] private RawImage MapRawImage;
    private Dictionary<string, GameObject> playerScorePanels = new Dictionary<string, GameObject>();
    private Dictionary<string, Score> playerScores;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();

        AssignMapRenderTexture();
    }

    private void AssignMapRenderTexture()
    {
        GameObject Map = gm.GetMap();
        MapPanelController MapPanelController = Map.GetComponentInChildren<MapPanelController>();
        Camera MapCamera = Map.GetComponentInChildren<Camera>();
        MapRawImage.texture = MapCamera.targetTexture;
        MapRawImage.rectTransform.sizeDelta = new Vector2(MapPanelController.Width, MapPanelController.Height);
    }

    public void RegisterPlayer(PhotonPlayer player)
    {
        if (playerScores == null)
        {
            playerScores = new Dictionary<string, Score>();
        }

        GameObject ps = Instantiate(PlayerStat, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        ps.transform.Find("Pilot Name").GetComponent<Text>().text = player.NickName;
        ps.transform.Find("Kills").GetComponent<Text>().text = "0";
        ps.transform.Find("Deaths").GetComponent<Text>().text = "0";

        Score score = new Score();
        string kills, deaths;
        kills = (player.CustomProperties["Kills"] == null) ? "0" : player.CustomProperties["Kills"].ToString();
        deaths = (player.CustomProperties["Deaths"] == null) ? "0" : player.CustomProperties["Deaths"].ToString();
        ps.transform.Find("Kills").GetComponent<Text>().text = kills;
        ps.transform.Find("Deaths").GetComponent<Text>().text = deaths;

        score.Kills = int.Parse(kills);
        score.Deaths = int.Parse(deaths);

        playerScores.Add(player.NickName, score);

        //Parent player stat to scorepanel
        ps.transform.SetParent(Panel.transform);

        ps.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        ps.transform.localPosition = Vector3.zero;
        ps.transform.localRotation = Quaternion.identity;

        playerScorePanels.Add(player.NickName, ps);
    }

    public void RegisterKill(string shooter_name, int newKills)
    {
        Score newShooterScore = new Score();
        newShooterScore.Kills = newKills;
        newShooterScore.Deaths = playerScores[shooter_name].Deaths;
        playerScores[shooter_name] = newShooterScore;

        playerScorePanels[shooter_name].transform.Find("Kills").GetComponent<Text>().text = newKills.ToString();
    }

    public void RegisterDeath(string victim_name, int newDeaths)
    {
        Score newVictimScore = new Score();
        newVictimScore.Kills = playerScores[victim_name].Kills;
        newVictimScore.Deaths = newDeaths;
        playerScores[victim_name] = newVictimScore;
        playerScorePanels[victim_name].transform.Find("Deaths").GetComponent<Text>().text = newDeaths.ToString();
    }

    public int GetPlayerKillCount(string name)
    {
        return playerScores[name].Kills;
    }

    public int GetPlayerDeathCount(string name)
    {
        return playerScores[name].Deaths;
    }


    public void ShowPanel(bool b)
    {
        ScorePanel.SetActive(b);
    }

    public void PlayerDisconnected(PhotonPlayer player)
    {
        RemovePlayerDataFromScorePanel(player);
    }

    private void RemovePlayerDataFromScorePanel(PhotonPlayer player)
    {
        playerScorePanels.Remove(player.NickName);
        playerScores.Remove(player.NickName);

        //Remove datas from scoreboard
        Text[] Ts = ScorePanel.GetComponentsInChildren<Text>();
        foreach (Text text in Ts)
        {
            if (text.text == player.NickName)
            {
                Destroy(text.transform.parent.gameObject);
                break;
            }
        }
    }

    public Dictionary<string, Score> PlayerScores() {
        return playerScores;
    }
    /*
if (PhotonNetwork.isMasterClient) {
            ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable();
            h2.Add("Kills", playerScores[shooter_name].Kills + 1);

            ExitGames.Client.Photon.Hashtable h3 = new ExitGames.Client.Photon.Hashtable();
            h3.Add("Deaths", playerScores[victim_name].Deaths + 1);
            shooter_player.SetCustomProperties(h2);
            victime_player.SetCustomProperties(h3);

            if (GameInfo.GameMode == "Team Deathmatch") {
                if (shooter_player.GetTeam() == PunTeams.Team.blue || shooter_player.GetTeam() == PunTeams.Team.none) {
                    blueScore++;
                    BlueScoreText.text = blueScore.ToString();
                    ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
                    h.Add("BlueScore", blueScore);
                    PhotonNetwork.room.SetCustomProperties(h);
                } else {
                    redScore++;
                    RedScoreText.text = redScore.ToString();
                    ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
                    h.Add("RedScore", redScore);
                    PhotonNetwork.room.SetCustomProperties(h);
                }
            }
        } else {
            if (GameInfo.GameMode == "Team Deathmatch") {
                if (shooter_player.GetTeam() == PunTeams.Team.blue || shooter_player.GetTeam() == PunTeams.Team.none) {
                    blueScore++;
                    BlueScoreText.text = blueScore.ToString();
                } else {
                    redScore++;
                    RedScoreText.text = redScore.ToString();
                }
            }
        }
*/


    //register kill
    //if (newShooterScore.Kills > CurrentMaxKills) CurrentMaxKills = newShooterScore.Kills;
}