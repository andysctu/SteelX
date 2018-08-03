using UnityEngine;
using UnityEngine.UI;

public class Timer {
    private Text time_Text;
    private int timerDuration, MaxTimeInSeconds = 300;
    private bool OnSyncTimeRequest = false;
    private int storedStartTime = 0, storedDuration = 0, gameBeginTimeDiff = 999, currentTimer = 999;

    public void Init() {
        GameObject TimerPanel = GameObject.Find("PanelCanvas/TimerPanel");
        time_Text = TimerPanel.GetComponentInChildren<Text>();

        currentTimer = int.Parse(PhotonNetwork.room.CustomProperties["MaxTime"].ToString()) * 60;
        int seconds = currentTimer % 60;
        int minutes = currentTimer / 60;
        time_Text.text = UIExtensionMethods.FillStringWithSpaces(minutes.ToString("D2") + ":" + seconds.ToString("D2"));
    }

    public void UpdateTime() {
        timerDuration = (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000 - gameBeginTimeDiff;
        currentTimer = storedDuration - timerDuration;

        int seconds = currentTimer % 60;
        int minutes = currentTimer / 60;
        time_Text.text = UIExtensionMethods.FillStringWithSpaces(minutes.ToString("D2") + ":" + seconds.ToString("D2"));
    }

    public bool SyncTime() {
        storedStartTime = (PhotonNetwork.room.CustomProperties["startTime"] == null) ? 0 : int.Parse(PhotonNetwork.room.CustomProperties["startTime"].ToString());
        storedDuration =(PhotonNetwork.room.CustomProperties["duration"] == null)? 0 : int.Parse(PhotonNetwork.room.CustomProperties["duration"].ToString());
        return storedDuration != 0;
    }

    public void MasterSyncTime() {
        int startTime = PhotonNetwork.ServerTimestamp;
        ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "startTime", startTime }, { "duration", GameInfo.MaxTime * 60 } };
        Debug.Log("startTime : " + startTime + ", duration : " + GameInfo.MaxTime * 60);
        PhotonNetwork.room.SetCustomProperties(ht);
        currentTimer = storedDuration - (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
    }

    public int GetCurrentTimeDiff() {
        return (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
    }

    public int GetCurrentTime() {
        return currentTimer;
    }

    public string GetCurrentFormatTime(bool separateBySpaces) {
        int seconds = currentTimer % 60;
        int minutes = currentTimer / 60;
        string finalStr = (separateBySpaces)? UIExtensionMethods.FillStringWithSpaces(minutes.ToString("D2") + ":" + seconds.ToString("D2")) : minutes.ToString("D2") + ":" + seconds.ToString("D2");

        return finalStr;
    }

    public void SetStoredTime(int newStoredStartTime, int newStoredDuration) {
        storedStartTime = newStoredStartTime;
        storedDuration = newStoredDuration;
    }

    public void SetGameBeginTimeDiff(int gameBeginTimeDiff) {
        this.gameBeginTimeDiff = gameBeginTimeDiff;
    }

    public bool CheckIfGameEnd() {
        return currentTimer <= 0 && storedDuration != 0;
    }

    public bool CheckIfGameBegin() {
        return (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000 > gameBeginTimeDiff;
    }
}