using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DriftCarController : MonoBehaviour
{
    [Header("Arcade handling")]
    public float acceleration = 9500f;
    public float reverseAcceleration = 5000f;
    public float maxSpeed = 42f;
    public float steering = 105f;
    public float grip = 7.5f;
    public float driftGrip = 2.0f;
    public float handbrakeGrip = 0.75f;
    public float downforce = 45f;

    [HideInInspector] public float steerInput, throttleInput, brakeInput;
    [HideInInspector] public bool handbrake;
    public float SpeedKmh => body ? body.velocity.magnitude * 3.6f : 0f;
    public float DriftAmount { get; private set; }

    Rigidbody body;
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.mass = 1250f;
        body.drag = 0.08f;
        body.angularDrag = 2.2f;
        body.centerOfMass = new Vector3(0, -0.45f, 0.05f);
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        steerInput = Input.GetAxisRaw("Horizontal");
        throttleInput = Mathf.Max(0, Input.GetAxisRaw("Vertical"));
        brakeInput = Mathf.Max(0, -Input.GetAxisRaw("Vertical"));
        handbrake = Input.GetKey(KeyCode.Space);
#endif
    }

    void FixedUpdate()
    {
        Vector3 localVel = transform.InverseTransformDirection(body.velocity);
        float speedFactor = Mathf.Clamp01(body.velocity.magnitude / 5f);
        if (throttleInput > 0.01f && body.velocity.magnitude < maxSpeed)
            body.AddForce(transform.forward * throttleInput * acceleration * Time.fixedDeltaTime, ForceMode.Force);
        if (brakeInput > 0.01f)
            body.AddForce(-transform.forward * brakeInput * reverseAcceleration * Time.fixedDeltaTime, ForceMode.Force);

        float gripNow = handbrake ? handbrakeGrip : (Mathf.Abs(steerInput) > 0.1f && throttleInput > 0.2f ? driftGrip : grip);
        float lateralCancel = -localVel.x * gripNow * body.mass * Time.fixedDeltaTime;
        body.AddForce(transform.right * lateralCancel, ForceMode.Force);

        float steerScale = Mathf.Clamp01(body.velocity.magnitude / 2.5f);
        float yaw = steerInput * steering * steerScale * (0.35f + speedFactor * 0.65f);
        if (handbrake) yaw *= 1.35f;
        body.MoveRotation(body.rotation * Quaternion.Euler(0, yaw * Time.fixedDeltaTime, 0));
        body.AddForce(-transform.up * downforce * speedFactor, ForceMode.Force);

        float slip = Mathf.Abs(Mathf.Atan2(localVel.x, Mathf.Abs(localVel.z) + 0.1f) * Mathf.Rad2Deg);
        DriftAmount = Mathf.Clamp01((slip - 8f) / 35f) * Mathf.Clamp01(body.velocity.magnitude / 7f);
    }
}
