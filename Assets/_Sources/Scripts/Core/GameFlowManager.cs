using System.Collections.Generic;

public class GameFlowManager : Singleton<GameFlowManager>
{
    public List<LocationData> currentSchedule = new List<LocationData>();
    public int currentEventIndex = 0;
    public int currentDay = 1;
    public int maxDays = 5;
    public bool isGameOver = false;

    public void StartWeek(List<LocationData> weekSchedule)
    {
        currentSchedule = new List<LocationData>(weekSchedule);
        currentEventIndex = 0;
    }

    public void NextEvent()
    {
        currentEventIndex++;
        if (currentEventIndex >= currentSchedule.Count)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("EventScene");
        }

    }
}