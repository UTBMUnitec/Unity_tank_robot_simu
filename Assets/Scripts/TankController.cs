using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour
{
    [Header("Paramètres de mouvement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float maxVelocity = 5f;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Missing Rigidbody");
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void HandleInput()
    {
        moveInput = Input.GetAxis("Vertical");   // Z/S
        rotateInput = Input.GetAxis("Horizontal"); // Q/D 
    }

    private float moveInput;
    private float rotateInput;

    private void ApplyMovement()
    {
        if (rb == null) return;

        if (Mathf.Abs(rotateInput) > 0.1f)
        {
            Vector3 rotation = new Vector3(0, rotateInput * rotationSpeed * Time.fixedDeltaTime, 0);
            Quaternion deltaRotation = Quaternion.Euler(rotation);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * moveInput * moveSpeed;
            
            if (rb.velocity.magnitude < maxVelocity)
            {
                rb.AddForce(moveDirection, ForceMode.Acceleration);
            }
        }
    }
}