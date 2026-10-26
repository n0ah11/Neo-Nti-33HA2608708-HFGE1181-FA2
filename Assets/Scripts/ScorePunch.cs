using TMPro;
using UnityEngine;

// Put on the Score text. Pops and flashes the text whenever the score goes up.
[RequireComponent(typeof(TextMeshProUGUI))]
public class ScorePunch : MonoBehaviour
{
    public float punchScale = 1.35f;
    public float punchTime = 0.2f;
    public Color flashColour = new Color(1f, 0.85f, 0.2f);

    private TextMeshProUGUI text;
    private Vector3 baseScale;
    private Color baseColour;
    private int lastScore;
    private float timer;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        baseScale = transform.localScale;
        baseColour = text.color;
        lastScore = GameMNGR.Instance != null ? GameMNGR.Instance.GetScore() : 0;
    }

    private void Update()
    {
        if (GameMNGR.Instance == null) return;

        int score = GameMNGR.Instance.GetScore();
        if (score > lastScore) timer = punchTime;
        lastScore = score;

        if (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(timer / punchTime);       // 1 -> 0
            transform.localScale = baseScale * Mathf.Lerp(1f, punchScale, k);
            text.color = Color.Lerp(baseColour, flashColour, k);
        }
        else
        {
            transform.localScale = baseScale;
            text.color = baseColour;
        }
    }
}
