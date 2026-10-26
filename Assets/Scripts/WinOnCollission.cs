using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinOnCollission : MonoBehaviour
{
    private string sceneName;

    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    private void OnTriggerEnter2D(Collider2D trig)
    {
        if (trig.gameObject.CompareTag("Player"))
        {
            Debug.Log("LEVEL FINISHED");
            SceneManager.LoadScene(sceneName);
        }
    }

}
