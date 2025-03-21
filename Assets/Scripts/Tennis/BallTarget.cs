using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTarget : MonoBehaviour
{
    [SerializeField] private Transform ballContainer;
    [SerializeField] private float arrivalAnimationDuration = 0.5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private float colorFadeDuration = 1.0f;
    
    private Renderer targetRenderer;
    
    private void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        
        if (targetRenderer != null)
        {
            targetRenderer.material.color = defaultColor;
        }
        
        if (ballContainer == null)
        {
            ballContainer = transform;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        TennisBall ball = other.GetComponent<TennisBall>();
        
        if (ball != null)
        {
            StartCoroutine(HandleBallArrival(ball));
            
            if (targetRenderer != null)
            {
                StartCoroutine(FlashColor());
            }
        }
    }
    
    private IEnumerator HandleBallArrival(TennisBall ball)
    {
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        Collider ballCollider = ball.GetComponent<Collider>();
        
        if (ballRb != null)
        {
            ballRb.isKinematic = true;
        }
        
        if (ballCollider != null)
        {
            ballCollider.enabled = false;
        }
        
        Vector3 startPosition = ball.transform.position;
        Vector3 startScale = ball.transform.localScale;
        Quaternion startRotation = ball.transform.rotation;
        
        float elapsed = 0;
        
        while (elapsed < arrivalAnimationDuration)
        {
            float t = elapsed / arrivalAnimationDuration;
            
            ball.transform.position = Vector3.Lerp(startPosition, ballContainer.position, t);
            ball.transform.localScale = Vector3.Lerp(startScale, startScale * 0.8f, t);
            ball.transform.rotation = Quaternion.Slerp(startRotation, ballContainer.rotation, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        ball.transform.SetParent(ballContainer);
        ball.transform.localPosition = Vector3.zero;
        ball.transform.localRotation = Quaternion.identity;
    }
    
    private IEnumerator FlashColor()
    {
        targetRenderer.material.color = hitColor;
        
        float elapsed = 0;
        
        while (elapsed < colorFadeDuration)
        {
            targetRenderer.material.color = Color.Lerp(hitColor, defaultColor, elapsed / colorFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        targetRenderer.material.color = defaultColor;
    }
}
