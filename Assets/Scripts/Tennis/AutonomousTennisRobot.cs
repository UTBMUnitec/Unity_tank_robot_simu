using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonomousTennisRobot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CreateTennisBall ballManager;
    [SerializeField] private GameObject leftRearWheel;
    [SerializeField] private GameObject rightRearWheel;
    [SerializeField] private GameObject leftFrontWheel;
    [SerializeField] private GameObject rightFrontWheel;
    [SerializeField] private Collider ballCollector;
    
    [Header("Ball Storage")]
    [SerializeField] private Transform[] ballStorageSpots = new Transform[5];
    [SerializeField] private Transform ballContainer;
    
    [Header("Tennis Court")]
    [SerializeField] private Collider courtCollider;
    [SerializeField] private float courtAvoidanceMargin = 1f;
    
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 120.0f;
    [SerializeField] private float wheelRadius = 0.15f;
    [SerializeField] private float stoppingDistance = 0.5f;
    
    private List<Transform> collectedBalls = new List<Transform>();
    private Transform targetBall;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private Bounds courtBounds;
    
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Vector3 courtEntryPoint;
    private bool isGoingToBall = false;
    private bool isReturningToEntryPoint = false;
    private bool isReturningToSpawn = false;
    
    private float linearVelocity = 0f;
    private float angularVelocity = 0f;

    private bool isPermanentlyStopped = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        spawnRotation = transform.rotation;
        
        if (ballManager == null)
            ballManager = FindObjectOfType<CreateTennisBall>();
            
        if (ballCollector != null && ballCollector.isTrigger == false)
            ballCollector.isTrigger = true;
            
        if (courtCollider == null)
        {
            Debug.LogError("Court collider reference is missing!");
        }
        else
        {
            courtBounds = courtCollider.bounds;
        }
        
        ValidateStorageSpots();
    }
    
    private void ValidateStorageSpots()
    {
        bool spotsValid = true;
        for (int i = 0; i < ballStorageSpots.Length; i++)
        {
            if (ballStorageSpots[i] == null)
            {
                Debug.LogError($"Ball storage spot {i+1} is not assigned!");
                spotsValid = false;
            }
        }
        
        if (!spotsValid)
        {
            Debug.LogError("Please assign all 5 ball storage spots in the inspector!");
        }
    }
    
    void Update()
    {
        if (isPermanentlyStopped)
        {
            return;
        }

        if (isReturningToSpawn)
        {
            MoveToPosition(spawnPosition);
            
            if (Vector3.Distance(transform.position, spawnPosition) < stoppingDistance * 5f && 
                Quaternion.Angle(transform.rotation, spawnRotation) < 20f)
            {
                isReturningToSpawn = false;
                linearVelocity = 0f;
                angularVelocity = 0f;
                
                if (HasNoMoreBallsToCollect())
                {
                    isPermanentlyStopped = true;
                }
            }
        }
        else if (isReturningToEntryPoint)
        {
            MoveToPosition(courtEntryPoint);
            
            if (Vector3.Distance(transform.position, courtEntryPoint) < stoppingDistance * 3)
            {
                isReturningToEntryPoint = false;
                FindClosestBall();
            }
        }
        else if (isGoingToBall && targetBall != null)
        {
            MoveToPosition(targetBall.position);
            
            if (Vector3.Distance(transform.position, targetBall.position) < stoppingDistance * 2)
            {
                isGoingToBall = false;
                isReturningToEntryPoint = true;
            }
        }
        else
        {
            FindClosestBall();
            
            if (targetBall == null && !isReturningToSpawn)
            {
                isReturningToSpawn = true;
            }
        }
        
        RotateWheels();
    }
    
    void FixedUpdate()
    {
        ApplyMovement();
    }
    
    private void FindClosestBall()
    {
        if (isPermanentlyStopped)
            return;

        float closestDistance = float.MaxValue;
        Transform closestBall = null;
        
        foreach (Transform ball in ballManager.tennisBallsList)
        {
            if (ball == null || collectedBalls.Contains(ball))
                continue;
                
            float distance = Vector3.Distance(transform.position, ball.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBall = ball;
            }
        }
        
        if (closestBall != targetBall)
        {
            targetBall = closestBall;
            
            if (targetBall != null)
            {
                bool ballInCourt = IsBallInCourt(targetBall.position);
                
                if (ballInCourt)
                {
                    courtEntryPoint = GetClosestPointOnCourtPerimeter(targetBall.position);
                    MoveToPosition(courtEntryPoint);
                    isGoingToBall = false;
                    isReturningToEntryPoint = false;
                    isReturningToSpawn = false;
                }
                else
                {
                    isGoingToBall = true;
                    isReturningToEntryPoint = false;
                    isReturningToSpawn = false;
                }
            }
        }
    }
    
    private void MoveToPosition(Vector3 position)
    {
        if (isPermanentlyStopped)
        {
            linearVelocity = 0;
            angularVelocity = 0;
            return;
        }

        Vector3 targetPos = new Vector3(position.x, transform.position.y, position.z);
        Vector3 directionToTarget = targetPos - transform.position;
        
        if (directionToTarget.magnitude < stoppingDistance)
        {
            if (!isReturningToSpawn || (isReturningToSpawn && Quaternion.Angle(transform.rotation, spawnRotation) < 5f))
            {
                linearVelocity = 0;
                angularVelocity = 0;
                
                if (targetPos == courtEntryPoint && targetBall != null && IsBallInCourt(targetBall.position))
                {
                    isGoingToBall = true;
                }
                
                return;
            }
        }
        
        Quaternion targetRotation;
        
        if (isReturningToSpawn && Vector3.Distance(transform.position, spawnPosition) < 2f)
        {
            targetRotation = spawnRotation;
        }
        else
        {
            targetRotation = Quaternion.LookRotation(directionToTarget);
        }
        
        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        
        if (angle > 10f)
        {
            if (isReturningToSpawn && Vector3.Distance(transform.position, spawnPosition) < stoppingDistance * 2)
            {
                float rotDir = Mathf.Sign(Mathf.DeltaAngle(transform.eulerAngles.y, spawnRotation.eulerAngles.y));
                angularVelocity = rotDir * rotationSpeed;
                linearVelocity = 0;
            }
            else
            {
                float rotDir = Mathf.Sign(Vector3.Cross(transform.forward, directionToTarget).y);
                angularVelocity = rotDir * rotationSpeed;
                linearVelocity = moveSpeed * 0.3f;
            }
        }
        else
        {
            angularVelocity = 0;
            linearVelocity = moveSpeed;
        }
    }
    
    private void ApplyMovement()
    {
        if (rb == null) return;
        
        float deltaTime = Time.fixedDeltaTime;
        
        float angularVelocityRad = angularVelocity * Mathf.Deg2Rad;
        
        Vector3 moveDistance = new Vector3(0, 0, linearVelocity * deltaTime);
        
        Vector3 newPosition = rb.position + transform.TransformDirection(moveDistance);
        Quaternion newRotation = rb.rotation * Quaternion.Euler(0, angularVelocity * deltaTime, 0);
        
        rb.MovePosition(newPosition);
        rb.MoveRotation(newRotation);
    }
    
    private void RotateWheels()
    {
        if (wheelRadius <= 0f) return;
        
        float wheelRotationSpeed = linearVelocity / wheelRadius;
        
        float leftWheelSpeed = wheelRotationSpeed - (angularVelocity * Mathf.Deg2Rad * 0.5f);
        float rightWheelSpeed = wheelRotationSpeed + (angularVelocity * Mathf.Deg2Rad * 0.5f);
        
        float leftWheelSpeedDeg = leftWheelSpeed * Mathf.Rad2Deg;
        float rightWheelSpeedDeg = rightWheelSpeed * Mathf.Rad2Deg;
        
        if (leftRearWheel != null)
            leftRearWheel.transform.Rotate(leftWheelSpeedDeg * Time.deltaTime, 0, 0);
        
        if (rightRearWheel != null)
            rightRearWheel.transform.Rotate(rightWheelSpeedDeg * Time.deltaTime, 0, 0);
        
        float frontWheelSpeed = (leftWheelSpeedDeg + rightWheelSpeedDeg) * 0.5f;
        
        if (leftFrontWheel != null)
            leftFrontWheel.transform.Rotate(frontWheelSpeed * Time.deltaTime, 0, 0);
        
        if (rightFrontWheel != null)
            rightFrontWheel.transform.Rotate(frontWheelSpeed * Time.deltaTime, 0, 0);
    }
    
    private bool IsBallInCourt(Vector3 position)
    {
        if (courtCollider == null) return false;
        
        courtBounds = courtCollider.bounds;
        return courtBounds.Contains(new Vector3(position.x, courtBounds.center.y, position.z));
    }
    
    private Vector3 GetClosestPointOnCourtPerimeter(Vector3 position)
    {
        if (courtCollider == null) return position;
        
        courtBounds = courtCollider.bounds;
        
        Vector3 closestPoint = courtBounds.ClosestPoint(position);
        
        if (closestPoint == position)
        {
            Vector3 toCenter = courtBounds.center - position;
            toCenter.y = 0;
            closestPoint = position + toCenter.normalized * (Vector3.Distance(position, courtBounds.center) + courtAvoidanceMargin);
        }
        else
        {
            Vector3 direction = position - courtBounds.center;
            direction.y = 0;
            direction.Normalize();
            
            closestPoint += direction * courtAvoidanceMargin;
        }
        
        closestPoint.y = transform.position.y;
        return closestPoint;
    }
    
    private void OnDrawGizmos()
    {
        if (targetBall != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetBall.position);
            Gizmos.DrawWireSphere(targetBall.position, 0.3f);
        }
        
        if (courtEntryPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(courtEntryPoint, 0.5f);
            Gizmos.DrawLine(transform.position, courtEntryPoint);
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPosition, 0.5f);
        
        if (courtCollider != null)
        {
            Gizmos.color = Color.red;
            Bounds gizmoBounds = courtCollider.bounds;
            Gizmos.DrawWireCube(gizmoBounds.center, gizmoBounds.size);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        TennisBall ball = other.GetComponent<TennisBall>();
        if (ball != null && !collectedBalls.Contains(other.transform))
        {
            int spotIndex = collectedBalls.Count;
            if (spotIndex < ballStorageSpots.Length)
            {
                Transform targetSpot = ballStorageSpots[spotIndex];
                ball.Collect(targetSpot);
                collectedBalls.Add(other.transform);
                
                if (other.transform == targetBall)
                {
                    targetBall = null;
                    isGoingToBall = false;
                    
                    if (IsBallInCourt(transform.position))
                    {
                        isReturningToEntryPoint = true;
                    }
                    else
                    {
                        FindClosestBall();
                        if (targetBall == null)
                        {
                            isReturningToSpawn = true;
                        }
                    }
                }
            }
        }
    }
    
    public void RemoveBallFromStorage(int spotIndex)
    {
        if (spotIndex < 0 || spotIndex >= ballStorageSpots.Length || spotIndex >= collectedBalls.Count)
            return;
            
        Transform removedBall = collectedBalls[spotIndex];
        collectedBalls.RemoveAt(spotIndex);
        
        ReorganizeBalls();
    }
    
    private void ReorganizeBalls()
    {
        for (int i = 0; i < collectedBalls.Count; i++)
        {
            if (i < ballStorageSpots.Length)
            {
                Transform ball = collectedBalls[i];
                TennisBall tennisBall = ball.GetComponent<TennisBall>();
                if (tennisBall != null)
                {
                    tennisBall.MoveToSpot(ballStorageSpots[i]);
                }
            }
        }
    }

    private bool HasNoMoreBallsToCollect()
    {
        if (ballManager == null || ballManager.tennisBallsList == null)
            return true;
        
        foreach (Transform ball in ballManager.tennisBallsList)
        {
            if (ball != null && !collectedBalls.Contains(ball))
                return false;
        }
        
        return true;
    }

    public int GetCollectedBallsCount()
    {
        return collectedBalls.Count;
    }
    
    public Transform GetBallFromSpot(int spotIndex)
    {
        if (spotIndex < 0 || spotIndex >= collectedBalls.Count)
            return null;
        
        return collectedBalls[spotIndex];
    }
}
