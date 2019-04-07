using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Timer {
    private Text time_Text;
    private int timerDuration;
    //private bool OnSyncTimeRequest = false;
    private int storedStartTime = 0, storedDuration = 0, gameBeginTimeDiff = 999, currentTimer = 999;

    private List<int> EventTimes = new List<int>();//sec
    private List<System.Action> EventActions = new List<System.Action>();
    private List<int> EventIndexToExecute = new List<int>();

    public void Init() {
        GameObject TimerPanel = GameObject.Find("PanelCanvas/TimerPanel");
        time_Text = TimerPanel.GetComponentInChildren<Text>();

        //currentTimer = int.Parse(PhotonNetwork.room.CustomProperties["MaxTime"].ToString()) * 60;
        int seconds = currentTimer % 60;
        int minutes = currentTimer / 60;
        time_Text.text = UIExtensionMethods.FillStringWithSpaces(minutes.ToString("D2") + ":" + seconds.ToString("D2"));
    }

    public void UpdateTime() {
        //timerDuration = (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000 - gameBeginTimeDiff;
        currentTimer = storedDuration - timerDuration;

        int seconds = currentTimer % 60;
        int minutes = currentTimer / 60;
        time_Text.text = UIExtensionMethods.FillStringWithSpaces(minutes.ToString("D2") + ":" + seconds.ToString("D2"));

        ProcessTimeEventCheck();
    }

    private void ProcessTimeEventCheck() {
        if(EventTimes.Count > 0) {
            for(int i = 0; i < EventTimes.Count; i++) {
                if(currentTimer < EventTimes[i]) {
                    int index = i;
                    EventIndexToExecute.Add(index);
                }
            }

            if(EventIndexToExecute.Count > 0) {                
                for(int i = 0; i < EventIndexToExecute.Count; i++) {
                    Debug.Log("execute event : "+i);
                    //Execute event
                    EventActions[i]();

                    //Remove the event
                    EventTimes.RemoveAt(i);
                    EventActions.RemoveAt(i);
                }           
                EventIndexToExecute.Clear();
            }
        }
    }

    public bool SyncTime() {
        //storedStartTime = (PhotonNetwork.room.CustomProperties["startTime"] == null) ? 0 : int.Parse(PhotonNetwork.room.CustomProperties["startTime"].ToString());
        //storedDuration =(PhotonNetwork.room.CustomProperties["duration"] == null)? 0 : int.Parse(PhotonNetwork.room.CustomProperties["duration"].ToString());
        return storedDuration != 0;
    }

    public void MasterSyncTime() {
        //int startTime = PhotonNetwork.ServerTimestamp;
        //ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "startTime", startTime }, { "duration", GameInfo.MaxTime * 60 } };
        //Debug.Log("startTime : " + startTime + ", duration : " + GameInfo.MaxTime * 60);
        //PhotonNetwork.room.SetCustomProperties(ht);
        //currentTimer = storedDuration - (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
    }

    public int GetCurrentTimeDiff() {
		return 0; //(PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
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

    public void RegisterTimeEvent(int time_in_sec, System.Action action) {
        EventTimes.Insert(EventTimes.Count, time_in_sec);
        EventActions.Insert(EventActions.Count, action);
    }

    public bool CheckIfGameEnd() {
        return currentTimer <= 0 && storedDuration != 0;
    }

    public bool CheckIfGameBegin() {
		return false; //(PhotonNetwork.ServerTimestamp - storedStartTime) / 1000 > gameBeginTimeDiff;
    }
}