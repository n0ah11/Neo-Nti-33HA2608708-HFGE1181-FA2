using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    
    public float disappearTime = 0.5f;
    public SpriteRenderer disappearingSprite;
    public Collider2D disappearingCollider;

    private void Awake()
    {
        disappearingCollider = GetComponent<Collider2D>();
        disappearingSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(Reappear());
    }

    public IEnumerator Disappear()
    {
        disappearingSprite.enabled = false;
        disappearingCollider.enabled = false;
        yield return new WaitForSeconds(disappearTime);
        yield return StartCoroutine(Reappear());
    }

    public IEnumerator Reappear()
    {
        disappearingSprite.enabled = true;
        disappearingCollider.enabled = true;
        yield return new WaitForSeconds(disappearTime);
        yield return StartCoroutine(Disappear());
    }
}
