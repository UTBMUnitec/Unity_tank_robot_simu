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
    
    [Header("Tennis Bounce Settings")]
    [SerializeField] private float bounceDistance = 0.6f; // 0-1 value, position of bounce between start and target
    [SerializeField] private float initialHeight = 2f; // Initial upward trajectory height
    [SerializeField] private float bounceStrength = 0.5f; // How strong the bounce is (0-1)
    [SerializeField] private float groundLevel = 0.1f; // Y position of the ground
    
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
        
        currentLaunchBall.SetParent(null);
        
        Vector3 startPos = transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;
        Vector3 targetPos = launchTarget.position;
        Vector3 bouncePos = Vector3.Lerp(startPos, targetPos, bounceDistance);
        bouncePos.y = groundLevel;
        
        float elapsed = 0f;
        
        while (elapsed < launchDuration)
        {
            float normalizedTime = elapsed / launchDuration;
            
            Vector3 horizontalPosition = Vector3.Lerp(startPos, targetPos, normalizedTime);
            
            float heightOffset = CalculateTennisBounceHeight(normalizedTime);
            Vector3 currentPosition = horizontalPosition;
            currentPosition.y = groundLevel + heightOffset;
            
            currentLaunchBall.position = currentPosition;
            
            if (elapsed > Time.deltaTime)
            {
                Vector3 lastPos = currentLaunchBall.position - currentLaunchBall.forward;
                Vector3 direction = currentPosition - lastPos;
                if (direction != Vector3.zero)
                {
                    currentLaunchBall.forward = direction.normalized;
                }
            }
            
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
            
            Gizmos.color = trajectoryColor;
            
            Vector3 previousPos = startPos;
            
            for (int i = 1; i <= trajectorySteps; i++)
            {
                float normalizedTime = (float)i / trajectorySteps;
                
                Vector3 horizontalPosition = Vector3.Lerp(startPos, targetPos, normalizedTime);
                
                float heightOffset = CalculateTennisBounceHeight(normalizedTime);
                Vector3 currentPosition = horizontalPosition;
                currentPosition.y = groundLevel + heightOffset;
                
                Gizmos.DrawLine(previousPos, currentPosition);
                
                previousPos = currentPosition;
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(bouncePos, 0.1f);
        }
    }
}
