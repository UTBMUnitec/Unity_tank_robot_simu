using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisRobotController : MonoBehaviour
{
    [Header("Robot Components")]
    [SerializeField] private GameObject leftRearWheel;
    [SerializeField] private GameObject rightRearWheel;
    [SerializeField] private GameObject leftFrontWheel;
    [SerializeField] private GameObject rightFrontWheel;
    
    [Header("Movement Parameters")]
    [SerializeField] private float maxLinearSpeed = 5.0f;
    [SerializeField] private float maxAngularSpeed = 180.0f;
    [SerializeField] private float wheelRadius = 0.15f;
    
    private float linearVelocity = 0f;
    private float angularVelocity = 0f;
    
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on tennis robot!");
        }
        
        if (leftRearWheel == null || rightRearWheel == null || 
            leftFrontWheel == null || rightFrontWheel == null)
        {
            Debug.LogWarning("One or more wheel references are missing!");
        }
    }

    void Update()
    {
        RotateWheels();
    }
    
    void FixedUpdate()
    {
        ApplyVelocity();
    }
    
    /// <summary>
    /// Sets the robot's velocity commands
    /// </summary>
    /// <param name="linear">Linear velocity in m/s</param>
    /// <param name="angular">Angular velocity in degrees/s</param>
    public void SetVelocity(float linear, float angular)
    {
        linearVelocity = Mathf.Clamp(linear, -maxLinearSpeed, maxLinearSpeed);
        angularVelocity = Mathf.Clamp(angular, -maxAngularSpeed, maxAngularSpeed);
    }
    
    private void ApplyVelocity()
    {
        if (rb == null) return;
        
        float deltaTime = Time.fixedDeltaTime;
        
        float angularVelocityRad = angularVelocity * Mathf.Deg2Rad;
        
        float rotationAngle = angularVelocityRad * deltaTime;
        
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
}
