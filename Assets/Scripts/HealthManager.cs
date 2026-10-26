using UnityEngine;
using UnityEngine.SceneManagement;

// Supplied FA2 script with ONE change (marked below): instead of reloading instantly,
// it shows the Game Over screen (score emphasised), which then reloads the level.
// If there's no LevelEndUI in the scene it falls back to the original instant reload.
public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;
    private HealthSystem healthSystem;
    private string sceneName;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        healthSystem = FindFirstObjectByType<PlayerController>().GetComponent<HealthSystem>();
        sceneName = SceneManager.GetActiveScene().name;
    }

    public HealthSystem AccessHealthSystem()
    {
        return healthSystem;
    }

    public void CheckHealth()
    {
        if (healthSystem.GetHearts() < 0)
        {
            // --- CHANGED: show Game Over screen first ---
            if (LevelEndUI.Instance != null)
            {
                LevelEndUI.Instance.ShowLose();
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
