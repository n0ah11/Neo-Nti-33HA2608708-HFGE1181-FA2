using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private int playerScore;

    [SerializeField]
    private GameObject hudUI;
    [SerializeField]
    private GameObject pauseUI;


    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    public GameObject[] heartVisuals;



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

    private void Start()
    {
        playerScore = 0;
        ResetScore();
        hudUI.SetActive(true);
        pauseUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale > 0)
            {
                hudUI.SetActive(false);
                pauseUI.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                hudUI.SetActive(true);
                pauseUI.SetActive(false);
                Time.timeScale = 1;
            }
            
        }
        
        SetScore();

    }

    public void SetScore()
    {
        playerScore = GameMNGR.Instance.GetScore();
        scoreText.text = "Score: " + playerScore;
    }

    public void ResetScore()
    {
        playerScore = 0;
        scoreText.text = "Score: " + playerScore;
    }

    public void InitializeHealthUI(HealthSystem healthSystem)
    {
        healthSystem = HealthManager.Instance.AccessHealthSystem();
        
        if (healthSystem.GetHearts() != heartVisuals.Length)
        {
            Debug.Log("Your Hearts at the start of the game should match up to your visuals");
        }

        foreach (var heart in heartVisuals)
        {
            heart.SetActive(true);
        }
    }

    public void HandleHealthVisual(HealthSystem healthSystem)
    {
        for (int i = 0; i < heartVisuals.Length; i++)
        {
            if (healthSystem.GetHearts() > i)
            {
                heartVisuals[i].SetActive(true);
            }
            else
            {
                heartVisuals[i].SetActive(false);
            }
        }
    }

    public void Resume()
    {
        hudUI.SetActive(true);
        pauseUI.SetActive(false);
        Time.timeScale = 1;
    }

    

}
