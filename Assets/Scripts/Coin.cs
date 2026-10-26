using UnityEngine;

// FIXED version of the FA1 Coin script (Section C bug fix):
// FA1 version had no Player check, so enemies/boxes could "collect" coins.
public class Coin : MonoBehaviour
{
    public int value = 10;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true; // stops double-counting if two player colliders touch it in one frame

        GameMNGR.Instance.IncrementScore(value);
        Destroy(gameObject);
    }
}
