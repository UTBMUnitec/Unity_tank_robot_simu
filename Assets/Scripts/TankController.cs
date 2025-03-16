using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform leftTrack;
    [SerializeField] private Transform rightTrack;

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float maxVelocity = 5f;
    
    [Header("Turret Parameters")]
    [SerializeField] private float turretRotationSpeed = 50f;
    [SerializeField] private KeyCode turretLeftKey = KeyCode.Q; 
    [SerializeField] private KeyCode turretRightKey = KeyCode.E;
    
    [Header("Track Parameters")]
    [SerializeField] private float trackScrollSpeed = 0.5f;
    [SerializeField] private string trackTexturePropertyName = "_MainTex";

    private Rigidbody rb;
    private Material leftTrackMaterial;
    private Material rightTrackMaterial;
    private float leftTrackOffset = 0f;
    private float rightTrackOffset = 0f;
    
    private float moveInput;
    private float rotateInput;
    private float turretRotateInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Missing Rigidbody");
        }
        
        // Get track materials
        if (leftTrack != null)
        {
            Renderer renderer = leftTrack.GetComponent<Renderer>();
            if (renderer != null)
                leftTrackMaterial = renderer.material;
        }
        
        if (rightTrack != null)
        {
            Renderer renderer = rightTrack.GetComponent<Renderer>();
            if (renderer != null)
                rightTrackMaterial = renderer.material;
        }
    }

    void Update()
    {
        HandleInput();
        RotateTurret();
        AnimateTracks();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void HandleInput()
    {
        moveInput = Input.GetAxis("Vertical");      // Z/S
        rotateInput = Input.GetAxis("Horizontal");  // Q/D
        
        // Turret rotation with Q and E keys
        if (Input.GetKey(turretLeftKey))
            turretRotateInput = -1f;
        else if (Input.GetKey(turretRightKey))
            turretRotateInput = 1f;
        else
            turretRotateInput = 0f;
    }

    private void RotateTurret()
    {
        if (turret != null && Mathf.Abs(turretRotateInput) > 0.1f)
        {
            // rotate turret independently from chassis
            turret.Rotate(0f, turretRotateInput * turretRotationSpeed * Time.deltaTime, 0f);
        }
    }

    private void AnimateTracks()
    {
        if (leftTrackMaterial == null || rightTrackMaterial == null)
            return;

        // calc track speeds for differential movement
        float leftTrackSpeed = moveInput;
        float rightTrackSpeed = moveInput;
        
        // turning = different track speeds
        if (rotateInput > 0) // right turn
        {
            leftTrackSpeed += rotateInput;
            rightTrackSpeed -= rotateInput;
        }
        else if (rotateInput < 0) // left turn
        {
            leftTrackSpeed -= rotateInput;
            rightTrackSpeed += rotateInput;
        }
        
        // update texture offsets
        leftTrackOffset += leftTrackSpeed * trackScrollSpeed * Time.deltaTime;
        rightTrackOffset += rightTrackSpeed * trackScrollSpeed * Time.deltaTime;
        
        // keep values between 0-1
        leftTrackOffset %= 1.0f;
        rightTrackOffset %= 1.0f;
        
        // apply offsets to textures
        Vector2 leftOffset = leftTrackMaterial.GetTextureOffset(trackTexturePropertyName);
        leftOffset.y = leftTrackOffset;
        leftTrackMaterial.SetTextureOffset(trackTexturePropertyName, leftOffset);
        
        Vector2 rightOffset = rightTrackMaterial.GetTextureOffset(trackTexturePropertyName);
        rightOffset.y = rightTrackOffset;
        rightTrackMaterial.SetTextureOffset(trackTexturePropertyName, rightOffset);
    }

    private void ApplyMovement()
    {
        if (rb == null) return;

        // simulate tank differential movement
        float leftTrackPower = moveInput;
        float rightTrackPower = moveInput;
        
        // adjust power for turning
        if (rotateInput > 0) // right
        {
            leftTrackPower = moveInput + rotateInput;
            rightTrackPower = moveInput - rotateInput;
        }
        else if (rotateInput < 0) // left
        {
            leftTrackPower = moveInput + rotateInput;
            rightTrackPower = moveInput - rotateInput;
        }
        
        // calc forward/backward movement and rotation
        float forwardPower = (leftTrackPower + rightTrackPower) * 0.5f;
        float turnPower = (leftTrackPower - rightTrackPower) * 0.5f;
        
        // apply forward/backward movement
        if (Mathf.Abs(forwardPower) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * forwardPower * moveSpeed;
            
            if (rb.velocity.magnitude < maxVelocity)
            {
                rb.AddForce(moveDirection, ForceMode.Acceleration);
            }
        }
        
        // apply rotation
        if (Mathf.Abs(turnPower) > 0.1f)
        {
            Vector3 rotation = new Vector3(0, turnPower * rotationSpeed * Time.fixedDeltaTime, 0);
            Quaternion deltaRotation = Quaternion.Euler(rotation);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }
}