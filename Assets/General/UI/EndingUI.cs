using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingUI : MonoBehaviour
{
    [Title("UI")]
    [SerializeField, Required] private TMP_Text timeText;
    [SerializeField, Required] private TMP_Text deathText;
    [SerializeField, Required] private TMP_Text pickup2Text;

    [Title("Outside References")]
    [SerializeField, Required] private Player player;
    [SerializeField, Required] private GameObject otherUI;

    private void OnEnable()
    {
        pickup2Text.text = $"{player.pickup2Count}/{player.pickup2Total}";
        deathText.text = player.deaths.ToString();
        timeText.text = GetTimeString();
        otherUI.SetActive(false);
    }

    public static string GetTimeString()
    {
        int minutes = (int)(Time.timeSinceLevelLoad / 60f);
        int seconds = (int)(Time.timeSinceLevelLoad % 60);
        string minutesString = minutes >= 10 ? minutes.ToString() : $"0{minutes}";
        string secondsString = seconds >= 10 ? seconds.ToString() : $"0{seconds}";
        return $"{minutesString}:{secondsString}";
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }
}