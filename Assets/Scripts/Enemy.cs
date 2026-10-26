using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public int damage;

    public int patrolIndex = 0;
    public Transform[] patrolPoints;

    public float movementSpeed;
    public float delay;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("Idle");
    }

  
    public IEnumerator Idle()
    {
        
        anim.SetBool("Idle", true);
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine("Patrol");
    }

    public IEnumerator Patrol()
    {
        anim.SetBool("Idle", false);
        if (transform.position.x > patrolPoints[patrolIndex].position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        
        while (Vector2.Distance(transform.position, patrolPoints[patrolIndex].position) > 0.1f)
        {
            yield return new WaitForSecondsRealtime(.01f);
            transform.position = Vector2.MoveTowards(transform.position, patrolPoints[patrolIndex].position,
                movementSpeed * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, patrolPoints[patrolIndex].position) < 0.1f)
        {
            
            if (patrolIndex >= patrolPoints.Length - 1)
            {
                patrolIndex = 0;
            }
            else
            {
                patrolIndex++;
            }
            
            yield return StartCoroutine("Idle");
        }
        
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(.5f, .3f);
            }

            Destroy(this.gameObject);
        }
    }

    public void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(.5f, .3f);
            }

            col.gameObject.GetComponent<HealthSystem>().Damage(damage);
            HealthManager.Instance.CheckHealth();

        }
    }


}
