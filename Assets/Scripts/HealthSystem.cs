using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float invulnerableDuration = 2f;     
    public float blinkInterval = 0.1f;   

    public enum PlayerState { vulnerable, invulnerable};
    public PlayerState state;

    [SerializeField]
    private int hearts;
    [SerializeField]
    private int maxHearts;


    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        UIManager.Instance.InitializeHealthUI(this);
    }

    public int GetHearts()
    {
        return hearts;
    }

    public void Damage(int damage)
    {
        if (state == PlayerState.vulnerable)
        {
            hearts -= damage;
            state = PlayerState.invulnerable;
            StopAllCoroutines();
            StartCoroutine(Invulnerable());
            UIManager.Instance.HandleHealthVisual(this);
        }
        else 
        { 

        }
        
    }

    public IEnumerator Invulnerable()
    {
        float elapsed = 0f;

        while (elapsed < invulnerableDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        spriteRenderer.enabled = true;
        state = PlayerState.vulnerable;
    }

}
