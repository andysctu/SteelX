using UnityEngine;
using UnityEngine.UI;

public class Timer {
    private Text time_Text;
    private int timerDuration;
    private bool OnSyncTimeRequest = false;
    private int storedStartTime = 0, storedDuration = 0, gameBeginTime = 0;
    private int currentTimer = 999;
    private bool endGameImmediately = false;//debug use
    public int MaxTimeInSeconds = 300;

    public void Init() {
        GameObject TimerPanel = GameObject.Find("PanelCanvas/TimerPanel");
        time_Text = TimerPanel.GetComponentInChildren<Text>();
    }

    public void UpdateTime() {
        timerDuration = (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
        currentTimer = storedDuration - timerDuration;

        //TODO : debug take out
        if (endGameImmediately) {
            endGameImmediately = false;
            currentTimer = 0;
        }

        int seconds = currentTimer % 60;
        int minutes = currentTimer / 60;
        time_Text.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }

    public bool SyncTime() {
        storedStartTime = int.Parse(PhotonNetwork.room.CustomProperties["startTime"].ToString());
        storedDuration = int.Parse(PhotonNetwork.room.CustomProperties["duration"].ToString());
        return storedStartTime != 0 || storedDuration != 0;
    }

    public void MasterSyncTime() {
        int startTime = PhotonNetwork.ServerTimestamp;
        ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "startTime", startTime }, { "duration", MaxTimeInSeconds } };
        Debug.Log("Setting " + startTime + ", " + MaxTimeInSeconds);
        PhotonNetwork.room.SetCustomProperties(ht);
        currentTimer = storedDuration - (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
    }

    public int GetCurrentTime() {
        return currentTimer;
    }

    public void SetStoredTime(int newStoredStartTime, int newStoredDuration) {
        storedStartTime = newStoredStartTime;
        storedDuration = newStoredDuration;
    }

    public void SetGameBeginTime(int gameBeginTime) {
        this.gameBeginTime = gameBeginTime;
    }

    public void EndGameImmediately() {//TODO : remove this
        endGameImmediately = true;
    }

    public bool CheckIfGameEnd() {
        return currentTimer <= 0 && (storedStartTime != 0 || storedDuration != 0);
    }

    public bool CheckIfGameBegin() {
        return currentTimer < gameBeginTime && ( storedStartTime != 0 || storedDuration != 0);
    }
}