using UnityEngine;

[RequireComponent(typeof(TennisRobotController))]
public class TennisRobotInput : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private float linearSpeedMultiplier = 5.0f;
    [SerializeField] private float angularSpeedMultiplier = 180.0f;
    [SerializeField] private string horizontalAxisName = "Horizontal";
    [SerializeField] private string verticalAxisName = "Vertical";
    [SerializeField] private bool invertVertical = false;
    [SerializeField] private bool invertHorizontal = false;

    private TennisRobotController robotController;

    void Start()
    {
        robotController = GetComponent<TennisRobotController>();
        if (robotController == null)
        {
            Debug.LogError("TennisRobotController component not found!");
            enabled = false;
        }
    }

    void Update()
    {
        float vertical = Input.GetAxis(verticalAxisName);
        float horizontal = Input.GetAxis(horizontalAxisName);
        
        if (invertVertical) vertical = -vertical;
        if (invertHorizontal) horizontal = -horizontal;
        
        float linearVelocity = vertical * linearSpeedMultiplier;
        float angularVelocity = -horizontal * angularSpeedMultiplier;
        
        robotController.SetVelocity(linearVelocity, angularVelocity);
    }
}
