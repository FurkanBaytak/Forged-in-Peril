using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI playerLogText;
    public TextMeshProUGUI enemyLogText;

    [Header("Scroll Rects")]
    public ScrollRect playerScrollRect;
    public ScrollRect enemyScrollRect;

    [Header("Log Settings")]
    public int maxLogEntries = 300;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AppendPlayerLog(string message)
    {
        if (playerLogText != null)
        {
            string timestamp = GetTimestamp();
            playerLogText.text += $"[{timestamp}] {message}\n";

            LimitLogEntries(playerLogText);

        }
    }

    public void AppendEnemyLog(string message)
    {
        if (enemyLogText != null)
        {
            string timestamp = GetTimestamp();
            enemyLogText.text += $"[{timestamp}] {message}\n";

            LimitLogEntries(enemyLogText);

        }
    }

    public void AppendSpecialLog(string source, string effectName, string result)
    {
        if (playerLogText != null)
        {
            string timestamp = GetTimestamp();
            playerLogText.text += $"<b>{source}</b>: <i>{effectName}</i> → <color=yellow>{result}</color>\n";

            LimitLogEntries(playerLogText);

        }
    }

    public void ClearLogs()
    {
        if (playerLogText != null)
            playerLogText.text = "";
        if (enemyLogText != null)
            enemyLogText.text = "";
    }

    public void ClearPlayerLogs()
    {
        if (playerLogText != null)
            playerLogText.text = "";
    }

    public void ClearEnemyLogs()
    {
        if (enemyLogText != null)
            enemyLogText.text = "";
    }


    private void LimitLogEntries(TextMeshProUGUI logText)
    {
        var entries = logText.text.Split('\n');
        if (entries.Length > maxLogEntries)
        {
            logText.text = string.Join("\n", entries, entries.Length - maxLogEntries, maxLogEntries);
        }
    }

    private string GetTimestamp()
    {
        return System.DateTime.Now.ToString("HH:mm:ss");
    }

    private IEnumerator ScrollToBottom(ScrollRect scrollRect)
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
