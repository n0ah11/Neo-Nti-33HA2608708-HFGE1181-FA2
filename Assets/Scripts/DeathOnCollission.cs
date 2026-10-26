using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DeathOnCollission : MonoBehaviour
{
    private string sceneName;
    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
            //Time.timeScale = 0;
        }
    }
}
