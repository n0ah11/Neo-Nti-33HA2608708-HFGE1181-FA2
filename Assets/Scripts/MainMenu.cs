using UnityEngine;
using UnityEngine.SceneManagement;

// Put this on an empty "MainMenu" object in the MainMenu scene.
// Play button  -> MainMenu.PlayGame
// Quit button  -> MainMenu.QuitGame
public class MainMenu : MonoBehaviour
{
    [Tooltip("Exact name of your first level scene, e.g. Level1")]
    public string firstLevelScene = "Level1";

    private void Start()
    {
        Time.timeScale = 1f; // in case we arrived here from the pause menu
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelScene);
    }

    public void QuitGame()
    {
        Debug.Log("Quit pressed");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // so Quit also works while testing in the Editor
#endif
    }
}
