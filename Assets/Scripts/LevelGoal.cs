using UnityEngine;

// Replaces WinOnCollission on the Flag. The Flag's collider must be "Is Trigger".
public class LevelGoal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (LevelEndUI.Instance != null)
            LevelEndUI.Instance.ShowWin();
        else
            Debug.LogWarning("No LevelEndUI in the scene - add one to show the win screen.");
    }
}
