using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class TennisBall : MonoBehaviour
{
    [SerializeField] private float collectAnimationDuration = 0.5f;
    
    private Rigidbody rb;
    private Collider ballCollider;
    private bool isCollected = false;
    private Transform currentSpot;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<Collider>();
    }
    
    public void Collect(Transform spot)
    {
        if (isCollected) return;
        
        isCollected = true;
        currentSpot = spot;
        
        rb.isKinematic = true;
        ballCollider.enabled = false;
        
        transform.position = spot.position;
        transform.rotation = spot.rotation;
        transform.SetParent(spot);
    }
    
    public void MoveToSpot(Transform newSpot)
    {
        if (!isCollected) return;
        
        currentSpot = newSpot;
        transform.position = newSpot.position;
        transform.rotation = newSpot.rotation;
        transform.SetParent(newSpot);
    }
}
