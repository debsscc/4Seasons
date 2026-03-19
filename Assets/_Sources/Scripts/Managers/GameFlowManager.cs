using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.MarkCurrentMapAsCompleted();
            }

            CheckForGameOver();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("EventScene");
        }
    }

    private void CheckForGameOver()
    {
        if (MapSelectionManager.Instance == null) return;

        var totalMaps = MapSelectionManager.Instance.allMaps.Length;
        var completedCount = GameSessionManager.Instance.GetCompletedMaps().Count();

        if (completedCount >= totalMaps)
        {
            isGameOver = true;
            StartCoroutine(GoToCreditsAfterDelay(3f));
        }
        else
        {
            GameSessionManager.Instance.ReturnToMapSelection();
        }
    }

    private IEnumerator GoToCreditsAfterDelay(float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        SceneTransition.Instance.ChangeScene("Credits");
    }
}
