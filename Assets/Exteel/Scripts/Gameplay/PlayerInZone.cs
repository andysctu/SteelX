using System.Collections.Generic;
using UnityEngine;

public class PlayerInZone : MonoBehaviour {
    private int player_viewID = -1;
    private int playerCount = 0;
    private bool isThePlayerInside = false;
    private int blueTeamPlayerCount = 0, redTeamPlayerCount = 0;
    private float LastCheckTime = 0;
    private float checkDeltaTime = 0.2f;
    private List<MechCombat> players = new List<MechCombat>();
    private List<MechCombat> playersToRemove = new List<MechCombat>();

    public void SetPlayer(GameObject player) {
        player_viewID = player.GetComponent<PhotonView>().viewID;
    }

    private void FixedUpdate() {
        if (Time.time - LastCheckTime >= checkDeltaTime) {
            LastCheckTime = Time.time;
            CountPlayer();//check dead player
        }
    }

    public void SetPlayerID(int id) {
        player_viewID = id;
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.tag == "Drone") return;

        PhotonView pv = collider.transform.root.GetComponent<PhotonView>();//TODO : improve this
        if (pv.viewID == player_viewID) {
            isThePlayerInside = true;
        }
        playerCount++;
        if (pv.owner.GetTeam() == PunTeams.Team.red) {
            redTeamPlayerCount++;
        } else {
            blueTeamPlayerCount++;
        }

        players.Add(collider.transform.root.GetComponent<MechCombat>());
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.tag == "Drone") return;

        PhotonView pv = collider.transform.root.GetComponent<PhotonView>();
        if (pv.viewID == player_viewID) {
            isThePlayerInside = false;
        }
        playerCount--;
        if (pv.owner.GetTeam() == PunTeams.Team.red) {
            redTeamPlayerCount--;
        } else {
            blueTeamPlayerCount--;
        }

        players.Remove(collider.transform.root.GetComponent<MechCombat>());
    }

    public void CountPlayer() {
        int tempPlayerCount = 0, tempRTcount = 0, tempBTcount = 0;
        foreach (MechCombat player in players) {
            if (player == null) {//Remove player disconnected
                playersToRemove.Add(player);
                continue;
            }

            if (!player.isDead) {
                tempPlayerCount++;
                if (player.photonView.owner.GetTeam() == PunTeams.Team.red) {
                    tempRTcount++;
                } else {
                    tempBTcount++;
                }
            }
        }

        if (playersToRemove.Count > 0) {
            foreach(MechCombat m in playersToRemove) {
                players.Remove(m);                
            }
            playersToRemove.Clear();
        }

        playerCount = tempPlayerCount;
        redTeamPlayerCount = tempRTcount;
        blueTeamPlayerCount = tempBTcount;
    }

    public int getNotFullHPPlayerCount() {
        int tempCount = 0;
        foreach (MechCombat player in players) {
            if (player == null) {//Remove player disconnected
                playersToRemove.Add(player);
                continue;
            }

            if (!(player.IsHpFull() || player.isDead)) { //ignore full hp & dead player
                tempCount++;
            }
        }
        if (playersToRemove.Count > 0) {
            foreach (MechCombat m in playersToRemove) {
                players.Remove(m);
            }
            playersToRemove.Clear();
        }

        return tempCount;
    }

    public int PlayerCountDiff() {
        if (whichTeamDominate() == 0) {
            return blueTeamPlayerCount - redTeamPlayerCount;
        } else {
            return redTeamPlayerCount - blueTeamPlayerCount;
        }
    }

    public int whichTeamDominate() {
        if (redTeamPlayerCount > blueTeamPlayerCount) {
            return 1;
        } else if (blueTeamPlayerCount > redTeamPlayerCount) {
            return 0;
        } else
            return -1;
    }

    public bool IsThePlayerInside() {
        return (player_viewID != -1 && isThePlayerInside);//-1 : not init
    }
}