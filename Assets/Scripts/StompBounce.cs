using UnityEngine;

// Optional polish: put on the Enemy next to Enemy.cs.
// When the player lands on the head trigger, they get a small bounce (like Mario).
public class StompBounce : MonoBehaviour
{
    public float bounceVelocity = 8f;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        Rigidbody2D rb = col.attachedRigidbody;
        if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceVelocity);
    }
}
