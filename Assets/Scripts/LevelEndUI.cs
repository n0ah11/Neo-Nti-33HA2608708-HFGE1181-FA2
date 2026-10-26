using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Shows a Win or Game Over panel with the final score emphasised (count-up + pulse).
// Put on an object inside your HUD Canvas and wire the fields in the Inspector.
public class LevelEndUI : MonoBehaviour
{
    public static LevelEndUI Instance;

    [Header("Panel")]
    public GameObject endPanel;            // full-screen panel, starts disabled
    public TextMeshProUGUI titleText;      // "LEVEL COMPLETE!" / "GAME OVER"
    public TextMeshProUGUI finalScoreText; // big score number

    [Header("Look")]
    public Color winColour = new Color(1f, 0.82f, 0.2f);   // gold
    public Color loseColour = new Color(0.9f, 0.25f, 0.25f); // red
    public float countUpTime = 1f;
    public float reloadDelayOnLose = 2.5f;

    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (endPanel != null) endPanel.SetActive(false);
    }

    public void ShowWin()  { Show("LEVEL COMPLETE!", winColour, false); }
    public void ShowLose() { Show("GAME OVER", loseColour, true); }

    private void Show(string title, Color colour, bool reload)
    {
        if (IsShowing) return;
        IsShowing = true;

        // stop the Escape key from un-pausing behind the end screen
        if (UIManager.Instance != null) UIManager.Instance.enabled = false;

        Time.timeScale = 0f;
        endPanel.SetActive(true);
        titleText.text = title;
        titleText.color = colour;
        finalScoreText.color = colour;

        int score = GameMNGR.Instance != null ? GameMNGR.Instance.GetScore() : 0;
        StartCoroutine(CountUp(score, reload));
    }

    private IEnumerator CountUp(int target, bool reload)
    {
        // count the score up (uses unscaled time because the game is paused)
        float t = 0f;
        while (t < countUpTime)
        {
            t += Time.unscaledDeltaTime;
            int shown = Mathf.RoundToInt(Mathf.Lerp(0, target, t / countUpTime));
            finalScoreText.text = "Score: " + shown;
            yield return null;
        }
        finalScoreText.text = "Score: " + target;

        // pulse the score a few times so it stands out
        Vector3 baseScale = finalScoreText.transform.localScale;
        for (int i = 0; i < 3; i++)
        {
            float p = 0f;
            while (p < 0.3f)
            {
                p += Time.unscaledDeltaTime;
                float s = 1f + Mathf.Sin(p / 0.3f * Mathf.PI) * 0.25f;
                finalScoreText.transform.localScale = baseScale * s;
                yield return null;
            }
        }
        finalScoreText.transform.localScale = baseScale;

        if (reload)
        {
            yield return new WaitForSecondsRealtime(reloadDelayOnLose);
            Retry();
        }
    }

    // Button: Play Again / Retry
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Button: Main Menu
    public void LoadMainMenu(string menuScene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }
}
