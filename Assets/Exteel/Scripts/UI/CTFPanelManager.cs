using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CTFPanelManager : MonoBehaviour {
    private GameManager gm;
    [SerializeField] private GameObject PlayerStat;
    [SerializeField] private GameObject Panel_RedTeam, Panel_BlueTeam;
    [SerializeField] private GameObject ScorePanel, RedScore, BlueScore;
    [SerializeField] private Text RedScoreText, BlueScoreText;
    [SerializeField] private Transform Map;
    private Dictionary<string, GameObject> playerScorePanels = new Dictionary<string, GameObject>();
    private Dictionary<string, Score> playerScores;

    private void Start() {
        gm = FindObjectOfType<GameManager>();

        InitMap();
    }

    private void InitMap() {
        GameObject MapToInstantiate = gm.GetMap();
        GameObject map = Instantiate(MapToInstantiate, Map);
        map.transform.localPosition = Vector3.zero;
    }

    public void UpdateScoreText(int blueScore, int redScore) {
        BlueScoreText.text = blueScore.ToString();
        RedScoreText.text = redScore.ToString();
    }

    public void RegisterPlayer(int player_viewID) {
        PhotonView pv = PhotonView.Find(player_viewID);
        string name;

        name = (pv.tag == "Drone") ? "Drone" + Random.Range(0, 9999) : pv.owner.NickName;

        //TODO : remove this if no errors
        //if (pv.isMine) {
        //    ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable();
        //    h2.Add("Kills", 0);
        //    h2.Add("Deaths", 0);
        //    PhotonNetwork.player.SetCustomProperties(h2);
        //}

        if (playerScores == null) {
            playerScores = new Dictionary<string, Score>();
        }

        GameObject ps = Instantiate(PlayerStat, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        ps.transform.Find("Pilot Name").GetComponent<Text>().text = name;
        ps.transform.Find("Kills").GetComponent<Text>().text = "0";
        ps.transform.Find("Deaths").GetComponent<Text>().text = "0";

        Score score = new Score();
        if (pv.tag != "Drone") {
            string kills, deaths;
            kills = (pv.owner.CustomProperties["Kills"]==null)? "0" : pv.owner.CustomProperties["Kills"].ToString();
            deaths = (pv.owner.CustomProperties["Deaths"] == null) ? "0" : pv.owner.CustomProperties["Deaths"].ToString();
            ps.transform.Find("Kills").GetComponent<Text>().text = kills;
            ps.transform.Find("Deaths").GetComponent<Text>().text = deaths;

            score.Kills = int.Parse(kills);
            score.Deaths = int.Parse(deaths);
        }
        playerScores.Add(name, score);

        //Parent player stat to scorepanel
        if (pv.tag != "Drone") {
            if (pv.owner.GetTeam() == PunTeams.Team.blue || pv.owner.GetTeam() == PunTeams.Team.none) {
                ps.transform.SetParent(Panel_BlueTeam.transform);
            } else {
                ps.transform.SetParent(Panel_RedTeam.transform);
            }
        } else {
            ps.transform.SetParent(Panel_BlueTeam.transform);
        }

        ps.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        ps.transform.localPosition = Vector3.zero;
        ps.transform.localRotation = Quaternion.identity;

        playerScorePanels.Add(name, ps);
    }

    public void RegisterKill(int shooter_viewID, int victim_viewID) {
        PhotonView shooter_pv = PhotonView.Find(shooter_viewID),
            victime_pv = PhotonView.Find(victim_viewID);

        if (shooter_pv == null || victime_pv == null || victime_pv.tag == "Drone") return;

        PhotonPlayer shooter_player = null, victime_player = null;
        shooter_player = shooter_pv.owner;
        victime_player = victime_pv.owner;

        string shooter_name = shooter_player.NickName, victim_name = victime_player.NickName;

        //only master update the room properties
        if (PhotonNetwork.isMasterClient) {
            ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable();
            h2.Add("Kills", playerScores[shooter_name].Kills + 1);

            ExitGames.Client.Photon.Hashtable h3 = new ExitGames.Client.Photon.Hashtable();
            h3.Add("Deaths", playerScores[victim_name].Deaths + 1);
            shooter_player.SetCustomProperties(h2);
            victime_player.SetCustomProperties(h3);
        }

        Score newShooterScore = new Score();
        newShooterScore.Kills = playerScores[shooter_name].Kills + 1;
        newShooterScore.Deaths = playerScores[shooter_name].Deaths;
        playerScores[shooter_name] = newShooterScore;

        Score newVictimScore = new Score();
        newVictimScore.Kills = playerScores[victim_name].Kills;
        newVictimScore.Deaths = playerScores[victim_name].Deaths + 1;
        playerScores[victim_name] = newVictimScore;

        playerScorePanels[shooter_name].transform.Find("Kills").GetComponent<Text>().text = playerScores[shooter_name].Kills.ToString();
        playerScorePanels[victim_name].transform.Find("Deaths").GetComponent<Text>().text = playerScores[victim_name].Deaths.ToString();

        //if (newShooterScore.Kills > CurrentMaxKills) CurrentMaxKills = newShooterScore.Kills;
    }

    public void ShowPanel(bool b) {
        ScorePanel.SetActive(b);
    }

    public void PlayerDisconnected(PhotonPlayer player) {
        RemovePlayerDataFromScorePanel(player);
    }

    private void RemovePlayerDataFromScorePanel(PhotonPlayer player) {
        playerScorePanels.Remove(player.NickName);
        playerScores.Remove(player.NickName);

        //Remove datas from scoreboard
        Text[] Ts = ScorePanel.GetComponentsInChildren<Text>();
        foreach (Text text in Ts) {
            if (text.text == player.NickName) {
                Destroy(text.transform.parent.gameObject);
                break;
            }
        }
    }
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