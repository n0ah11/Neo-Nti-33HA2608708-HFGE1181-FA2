using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMNGR : MonoBehaviour
{
   public string SceneName;

   public void LoadScene()
   {
      Time.timeScale = 1;
      SceneManager.LoadScene(SceneName);
   }
}
