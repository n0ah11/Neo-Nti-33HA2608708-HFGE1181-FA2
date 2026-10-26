using System;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage;
    public float knockbackForce = 999;
    
    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(.5f, .3f);
            }

            col.gameObject.GetComponent<HealthSystem>().Damage(damage);
            HealthManager.Instance.CheckHealth();
            col.gameObject.GetComponent<PlayerController>().Knockback(knockbackForce);
            
        }
    }
}
