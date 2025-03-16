using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretFiringSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem muzzleFlash;
    
    [Header("Firing Parameters")]
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private float fireRate = 1f; 
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    
    private bool canFire = true;
    private float nextFireTime;

    private void Start()
    {
        if (firePoint == null)
        {
            Debug.LogError("No fire point assigned to TurretFiringSystem");
        }
        
        if (projectilePrefab == null)
        {
            Debug.LogError("No projectile prefab assigned to TurretFiringSystem");
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(fireKey) && canFire && Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    private void Fire()
    {
        nextFireTime = Time.time + 1f/fireRate;
        
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        
        // instantiate projectile
        if (firePoint != null && projectilePrefab != null)
        {
            GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            
            // set projectile velocity
            if (projectile != null)
            {
                projectile.Initialize(projectileSpeed);
            }
            else
            {
                // if no Projectile script, add velocity directly to rigidbody
                Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = firePoint.forward * projectileSpeed;
                }
            }
        }
    }
}