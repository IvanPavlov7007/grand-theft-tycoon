using UnityEngine;

/// <summary>
/// Simulates a simple wind-up spring motor on a 2D wheel.
/// Stores energy when rotated in one direction and releases it as torque.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class WheelSpringMotor2D : MonoBehaviour
{
    [Header("Spring settings")]
    [Tooltip("How much spring energy is gained per radian of backward rotation.")]
    public float springStiffness = 5f;

    [Tooltip("Maximum stored spring energy.")]
    public float maxEnergy = 50f;

    [Tooltip("How fast the spring discharges when driving the wheel.")]
    public float dischargeRate = 10f;

    [Tooltip("Maximum torque the spring can apply.")]
    public float maxTorque = 50f;

    [Tooltip("If true, only accumulate energy when wheel rotates 'backwards' relative to the car body.")]
    public bool onlyWindWhenBackward = true;

    [Header("Ground detection")]
    [Tooltip("Layer mask for what counts as ground.")]
    public LayerMask groundLayerMask;

    [Tooltip("Distance to raycast below the wheel center to detect ground.")]
    public float groundCheckDistance = 0.25f;

    [Tooltip("If true, do not release spring energy while wheel is not grounded.")]
    public bool onlyDriveWhenGrounded = true;

    [Header("Stability")]
    [Tooltip("Max allowed angular velocity of the car body while driving. Extra torque is reduced above this.")]
    public float maxCarAngularVelocity = 200f;

    [Tooltip("Scale applied to torque when car is spinning too fast (0..1).")]
    public float spinDampingFactor = 0.25f;

    [Header("Debug")]
    public float currentEnergy;
    public bool isGrounded;

    private Rigidbody2D _rb;
    private Rigidbody2D _carBody;
    private WheelJoint2D _joint;

    private float _previousRelativeAngle;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _joint = GetComponent<WheelJoint2D>();
        if (_joint != null)
        {
            _carBody = _joint.connectedBody;
        }
    }

    private void Start()
    {
        _previousRelativeAngle = GetRelativeAngle();
    }

    private void FixedUpdate()
    {
        if (_rb == null || _carBody == null) return;

        // --- Ground check ---
        isGrounded = CheckGrounded();

        float relativeAngle = GetRelativeAngle();
        float deltaAngle = Mathf.DeltaAngle(_previousRelativeAngle, relativeAngle) * Mathf.Deg2Rad;
        Debug.Log($"Delta Angle: {deltaAngle}, relativeAngle {relativeAngle}");
        _previousRelativeAngle = relativeAngle;

        // WIND PHASE
        if (!Mathf.Approximately(deltaAngle, 0f))
        {
            bool isBackward = deltaAngle > 0f;

            if (!onlyWindWhenBackward || isBackward)
            {
                float addedEnergy = deltaAngle * springStiffness; // negative deltaAngle -> positive energy
                if (addedEnergy > 0f)
                {
                    currentEnergy = Mathf.Min(currentEnergy + addedEnergy, maxEnergy);
                }
            }
        }

        // RELEASE PHASE
        if (currentEnergy > 0f)
        {
            if (!onlyDriveWhenGrounded || isGrounded)
            {
                float desiredTorque = Mathf.Min(currentEnergy, maxTorque);

                // If the car is spinning like crazy, damp the torque to avoid chaotic flips:
                float torqueScale = 1f;
                float absCarAngVel = Mathf.Abs(_carBody.angularVelocity);
                if (absCarAngVel > maxCarAngularVelocity)
                {
                    torqueScale = spinDampingFactor;
                }

                desiredTorque *= torqueScale;

                if (desiredTorque > 0f)
                {
                    // Sign may need flipping depending on wheel orientation:
                    _rb.AddTorque(-desiredTorque, ForceMode2D.Force);

                    float drain = dischargeRate * Time.fixedDeltaTime * torqueScale;
                    currentEnergy = Mathf.Max(0f, currentEnergy - drain);
                }
            }
            else
            {
                // Optional: very slight passive leakage so it doesn't store infinite energy in air
                float slightLeak = dischargeRate * 0.1f * Time.fixedDeltaTime;
                currentEnergy = Mathf.Max(0f, currentEnergy - slightLeak);
            }
        }
    }

    /// <summary>
    /// Returns wheel angle relative to the car body in degrees.
    /// </summary>
    private float GetRelativeAngle()
    {
        if (_carBody == null) return _rb.rotation;
        return _rb.rotation - _carBody.rotation;
    }

    /// <summary>
    /// Simple raycast below wheel to see if something on groundLayerMask is under us.
    /// </summary>
    private bool CheckGrounded()
    {
        Vector2 origin = _rb.worldCenterOfMass;
        Vector2 direction = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, groundCheckDistance, groundLayerMask);
        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 origin = _rb != null ? (Vector3)_rb.worldCenterOfMass : transform.position;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}