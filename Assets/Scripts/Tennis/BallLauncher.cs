using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AutonomousTennisRobot))]
public class BallLauncher : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private Transform launchTarget;
    [SerializeField] private float launchHeight = 3f;
    [SerializeField] private float launchDuration = 1.5f;
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private KeyCode launchKey = KeyCode.Space;
    
    [Header("Physics Settings")]
    [SerializeField] private float initialVelocity = 8f;
    [SerializeField] private bool adjustDurationByDistance = true;
    [SerializeField] private float distanceSpeedFactor = 0.2f;
    [SerializeField] private float bounceVelocityLoss = 0.3f;
    [SerializeField] private float spinEffect = 0.2f;
    
    [Header("Tennis Bounce Settings")]
    [SerializeField] private float bounceDistance = 0.6f;
    [SerializeField] private float initialHeight = 2f;
    [SerializeField] private float bounceStrength = 0.5f;
    [SerializeField] private float groundLevel = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showTrajectory = true;
    [SerializeField] private int trajectorySteps = 30;
    [SerializeField] private Color trajectoryColor = Color.yellow;
    
    private AutonomousTennisRobot robotController;
    private bool isLaunching = false;
    private Transform currentLaunchBall;
    
    private void Start()
    {
        robotController = GetComponent<AutonomousTennisRobot>();
        
        if (launchTarget == null)
        {
            Debug.LogError("No launch target assigned to BallLauncher!");
        }
        
        if (heightCurve.length == 0)
        {
            heightCurve = CreateDefaultHeightCurve();
        }
    }
    
    private AnimationCurve CreateDefaultHeightCurve()
    {
        AnimationCurve curve = new AnimationCurve();
        
        curve.AddKey(new Keyframe(0f, 0f));
        curve.AddKey(new Keyframe(bounceDistance * 0.5f, initialHeight));
        curve.AddKey(new Keyframe(bounceDistance, 0f));
        curve.AddKey(new Keyframe(bounceDistance + (1 - bounceDistance) * 0.5f, initialHeight * bounceStrength));
        curve.AddKey(new Keyframe(1f, 0f));
        
        return curve;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(launchKey) && !isLaunching && robotController.GetCollectedBallsCount() > 0)
        {
            PrepareAndLaunchBall();
        }
    }
    
    private void PrepareAndLaunchBall()
    {
        currentLaunchBall = robotController.GetBallFromSpot(0);
        
        if (currentLaunchBall == null)
            return;
        
        robotController.RemoveBallFromStorage(0);
        
        StartCoroutine(OrientRobotTowardsTarget(() => {
            StartCoroutine(LaunchBallCoroutine());
        }));
    }
    
    private IEnumerator OrientRobotTowardsTarget(System.Action onComplete)
    {
        isLaunching = true;
        
        Vector3 targetDirection = launchTarget.position - transform.position;
        targetDirection.y = 0;
        
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        float rotationDuration = 1.0f;
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        
        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = targetRotation;
        
        onComplete?.Invoke();
    }
    
    private IEnumerator LaunchBallCoroutine()
    {
        TennisBall ball = currentLaunchBall.GetComponent<TennisBall>();
        ball.transform.GetComponent<SphereCollider>().enabled = true;
        currentLaunchBall.SetParent(null);
        
        Vector3 startPos = transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;
        Vector3 targetPos = launchTarget.position;
        Vector3 bouncePos = Vector3.Lerp(startPos, targetPos, bounceDistance);
        bouncePos.y = groundLevel;
        
        float distanceToTarget = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), 
                                                 new Vector3(targetPos.x, 0, targetPos.z));
        
        float actualLaunchDuration = launchDuration;
        if (adjustDurationByDistance)
        {
            actualLaunchDuration = distanceToTarget / initialVelocity;
            actualLaunchDuration = Mathf.Clamp(actualLaunchDuration, 1.0f, 3.0f);
        }
        
        float elapsed = 0f;
        Vector3 lastPosition = startPos;
        
        while (elapsed < actualLaunchDuration)
        {
            float normalizedTime = elapsed / actualLaunchDuration;
            
            float currentVelocity = initialVelocity;
            if (normalizedTime > bounceDistance)
            {
                currentVelocity *= (1f - bounceVelocityLoss);
            }
            
            if (normalizedTime < 0.1f)
            {
                currentVelocity *= normalizedTime / 0.1f;
            }
            
            float horizontalProgress = Mathf.Pow(normalizedTime, 0.9f);
            Vector3 horizontalPosition = Vector3.Lerp(startPos, targetPos, horizontalProgress);
            
            if (normalizedTime > bounceDistance && normalizedTime < 0.9f)
            {
                float spinFactor = (normalizedTime - bounceDistance) / (0.9f - bounceDistance);
                float lateralOffset = spinEffect * spinFactor * (1f - spinFactor) * distanceToTarget * 0.05f;
                
                horizontalPosition += transform.right * lateralOffset;
            }
            
            float heightOffset = CalculateTennisBounceHeight(normalizedTime);
            Vector3 currentPosition = horizontalPosition;
            currentPosition.y = groundLevel + heightOffset;
            
            currentLaunchBall.position = currentPosition;
            
            Vector3 direction = (currentPosition - lastPosition).normalized;
            if (direction != Vector3.zero)
            {
                currentLaunchBall.forward = direction;
                
                currentLaunchBall.Rotate(Vector3.forward, currentVelocity * 30f * Time.deltaTime, Space.Self);
            }
            
            lastPosition = currentPosition;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentLaunchBall.position = targetPos;
        
        isLaunching = false;
    }
    
    private float CalculateTennisBounceHeight(float normalizedTime)
    {
        if (heightCurve != null && heightCurve.length > 0)
        {
            return heightCurve.Evaluate(normalizedTime) * launchHeight;
        }
        
        float height;
        
        if (normalizedTime < bounceDistance)
        {
            float t = normalizedTime / bounceDistance;
            height = initialHeight * 4 * t * (1 - t);
        }
        else
        {
            float t = (normalizedTime - bounceDistance) / (1 - bounceDistance);
            height = initialHeight * bounceStrength * 4 * t * (1 - t);
        }
        
        return height;
    }
    
    private void OnDrawGizmos()
    {
        if (showTrajectory && launchTarget != null)
        {
            Vector3 startPos = transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;
            Vector3 targetPos = launchTarget.position;
            Vector3 bouncePos = Vector3.Lerp(startPos, targetPos, bounceDistance);
            bouncePos.y = groundLevel;
            
            float distanceToTarget = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), 
                                                     new Vector3(targetPos.x, 0, targetPos.z));
            
            Gizmos.color = trajectoryColor;
            
            Vector3 previousPos = startPos;
            Vector3 lastDrawnPoint = startPos;
            
            for (int i = 1; i <= trajectorySteps; i++)
            {
                float normalizedTime = (float)i / trajectorySteps;
                
                float horizontalProgress = Mathf.Pow(normalizedTime, 0.9f);
                Vector3 horizontalPosition = Vector3.Lerp(startPos, targetPos, horizontalProgress);
                
                if (normalizedTime > bounceDistance && normalizedTime < 0.9f)
                {
                    float spinFactor = (normalizedTime - bounceDistance) / (0.9f - bounceDistance);
                    float lateralOffset = spinEffect * spinFactor * (1f - spinFactor) * distanceToTarget * 0.05f;
                    
                    horizontalPosition += transform.right * lateralOffset;
                }
                
                float heightOffset = CalculateTennisBounceHeight(normalizedTime);
                Vector3 currentPosition = horizontalPosition;
                currentPosition.y = groundLevel + heightOffset;
                
                Gizmos.DrawLine(previousPos, currentPosition);
                
                previousPos = currentPosition;
                lastDrawnPoint = currentPosition;
            }
            
            if (Vector3.Distance(lastDrawnPoint, targetPos) > 0.1f)
            {
                Gizmos.DrawLine(lastDrawnPoint, targetPos);
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(bouncePos, 0.1f);
        }
    }
}
