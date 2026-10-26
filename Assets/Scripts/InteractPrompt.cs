using UnityEngine;

// Put on the "Press E" prompt that is a CHILD of the Player.
// The Player flips its scale to -1 when facing left, which would mirror the text;
// this keeps the prompt readable and gives it a small bob so it catches the eye.
public class InteractPrompt : MonoBehaviour
{
    public float bobHeight = 0.08f;
    public float bobSpeed = 4f;

    private Vector3 startLocalPos;
    private Vector3 startLocalScale;

    private void Awake()
    {
        startLocalPos = transform.localPosition;
        startLocalScale = transform.localScale;
    }

    private void LateUpdate()
    {
        float parentSign = transform.parent != null ? Mathf.Sign(transform.parent.localScale.x) : 1f;
        transform.localScale = new Vector3(startLocalScale.x * parentSign, startLocalScale.y, startLocalScale.z);
        transform.localPosition = startLocalPos + Vector3.up * Mathf.Sin(Time.unscaledTime * bobSpeed) * bobHeight;
    }
}
