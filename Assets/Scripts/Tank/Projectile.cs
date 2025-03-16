using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionRadius = 5f;
    
    private Rigidbody rb;
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
    }
    
    public void Initialize(float speed)
    {
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }
    
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, explosionEffectPrefab.transform.rotation);
            
            // destroy explosion effect after some time
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(explosion, duration);
            }
            else
            {
                Destroy(explosion, 2f);
            }
        }
        
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        
        // apply explosion damage/force to nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // add force to rigidbodies
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(500f, transform.position, explosionRadius);
            }
            
            // TODO: apply damage to enemies or other objects
        }
        
        Destroy(gameObject);
    }
}