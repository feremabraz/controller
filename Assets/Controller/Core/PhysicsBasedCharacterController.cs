using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = System.Diagnostics.Debug;

/// <summary>
/// A floating-capsule oriented physics based character controller.
/// </summary>
public class PhysicsBasedCharacterController : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _gravitationalForce;
    private readonly Vector3 _rayDir = Vector3.down;
    private Vector3 _previousVelocity = Vector3.zero;
    private Vector2 _moveContext;
    private ParticleSystem.EmissionModule _emission;

    [Header("Other:")]
    [SerializeField] private bool adjustInputsToCameraAngle;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private ParticleSystem dustParticleSystem;

    private bool _shouldMaintainHeight = true;
    
    [Header("Height Spring:")]
    [SerializeField] private float rideHeight = 1.75f; // rideHeight: desired distance to ground (Note, this is distance from the original raycast position (currently centre of transform)). 
    [SerializeField] private float rayToGroundLength = 3f; // rayToGroundLength: max distance of raycast to ground (Note, this should be greater than the rideHeight).
    [SerializeField] public float rideSpringStrength = 50f; // rideSpringStrength: strength of spring. (?)
    [SerializeField] private float rideSpringDamper = 5f; // rideSpringDampener: dampener of spring. (?)
    [SerializeField] private Oscillator squashAndStretchOscillator;


    private enum LookDirectionOptions { Velocity, Acceleration, MoveInput };
    private Quaternion _uprightTargetRot = Quaternion.identity; // Adjust y value to match the desired direction to face.
    private Quaternion _lastTargetRot;
    private Vector3 _platformInitRot;
    private bool _didLastRayHit;

    [Header("Upright Spring:")]
    [SerializeField] private LookDirectionOptions characterLookDirection = LookDirectionOptions.Velocity;
    [SerializeField] private float uprightSpringStrength = 40f;
    [SerializeField] private float uprightSpringDamper = 5f;


    private Vector3 _moveInput;
    private readonly float _speedFactor = 1f;
    private readonly float _maxAccelForceFactor = 1f;
    private Vector3 _mGoalVel = Vector3.zero;

    [Header("Movement:")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 200f;
    [SerializeField] private float maxAccelForce = 150f;
    [SerializeField] private float leanFactor = 0.25f;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;
    [SerializeField] private AnimationCurve maxAccelerationForceFactorFromDot;
    [SerializeField] private Vector3 moveForceScale = new Vector3(1f, 0f, 1f);


    private Vector3 _jumpInput;
    private float _timeSinceJumpPressed;
    private float _timeSinceUngrounded;
    private float _timeSinceJump;
    private bool _jumpReady = true;
    private bool _isJumping;

    [Header("Jump:")]
    [SerializeField] private float jumpForceFactor = 10f;
    [SerializeField] private float riseGravityFactor = 5f;
    [SerializeField] private float fallGravityFactor = 10f; // typically > 1f (i.e. 5f).
    [SerializeField] private float lowJumpFactor = 2.5f;
    [SerializeField] private float jumpBuffer = 0.15f; // Note, jumpBuffer shouldn't really exceed the time of the jump.
    [SerializeField] private float coyoteTime = 0.25f;

    /// <summary>
    /// Prepare frequently used variables.
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravitationalForce = Physics.gravity * _rb.mass;

        if (dustParticleSystem)
        {
            _emission = dustParticleSystem.emission; // Stores the module in a local variable
            _emission.enabled = false; // Applies the new value directly to the Particle System
        }
    }

    /// <summary>
    /// Use the result of a Raycast to determine if the capsules distance from the ground is sufficiently close to the desired ride height such that the character can be considered 'grounded'.
    /// </summary>
    /// <param name="rayHitGround">Whether or not the Raycast hit anything.</param>
    /// <param name="rayHit">Information about the ray.</param>
    /// <returns>Whether or not the player is considered grounded.</returns>
    private bool CheckIfGrounded(bool rayHitGround, RaycastHit rayHit)
    {
        bool grounded;
        if (rayHitGround)
        {
            grounded = rayHit.distance <= rideHeight * 1.3f; // 1.3f allows for greater leniency (as the value will oscillate about the rideHeight).
        }
        else
        {
            grounded = false;
        }
        return grounded;
    }

    /// <summary>
    /// Gets the look desired direction for the character to look.
    /// The method for determining the look direction is depends on the lookDirectionOption.
    /// </summary>
    /// <param name="lookDirectionOption">The factor which determines the look direction: velocity, acceleration or moveInput.</param>
    /// <returns>The desired look direction.</returns>
    private Vector3 GetLookDirection(LookDirectionOptions lookDirectionOption)
    {
        var lookDirection = Vector3.zero;
        if (lookDirectionOption == LookDirectionOptions.Velocity || lookDirectionOption == LookDirectionOptions.Acceleration)
        {
            var velocity = _rb.velocity;
            velocity.y = 0f;
            if (lookDirectionOption == LookDirectionOptions.Velocity)
            {
                lookDirection = velocity;
            }
            else
            {
                var deltaVelocity = velocity - _previousVelocity;
                _previousVelocity = velocity;
                var deltaAcceleration = deltaVelocity / Time.fixedDeltaTime;
                lookDirection = deltaAcceleration;
            }
        }
        else if (lookDirectionOption == LookDirectionOptions.MoveInput)
        {
            lookDirection = _moveInput;
        }
        return lookDirection;
    }

    private bool _prevGrounded;

    /// <summary>
    /// Determines and plays the appropriate character sounds, particle effects, then calls the appropriate methods to move and float the character.
    /// </summary>
    private void FixedUpdate()
    {
        _moveInput = new Vector3(_moveContext.x, 0, _moveContext.y);

        if (adjustInputsToCameraAngle)
        {
            _moveInput = AdjustInputToFaceCamera(_moveInput);
        }

        var (rayHitGround, rayHit) = RaycastToGround();
        SetPlatform(rayHit);

        var grounded = CheckIfGrounded(rayHitGround, rayHit);
        if (grounded)
        {
            if (_prevGrounded == false)
            {
                if (!FindObjectOfType<AudioManager>().IsPlaying("Land"))
                {
                    FindObjectOfType<AudioManager>().Play("Land");
                }

            }

            if (_moveInput.magnitude != 0)
            {
                if (!FindObjectOfType<AudioManager>().IsPlaying("Walking"))
                {
                    FindObjectOfType<AudioManager>().Play("Walking");
                }
            }
            else
            {
                FindObjectOfType<AudioManager>().Stop("Walking");
            }

            if (dustParticleSystem)
            {
                if (_emission.enabled == false)
                {
                    _emission.enabled = true; // Applies the new value directly to the Particle System                  
                }
            }

            _timeSinceUngrounded = 0f;

            if (_timeSinceJump > 0.2f)
            {
                _isJumping = false;
            }
        }
        else
        {
            FindObjectOfType<AudioManager>().Stop("Walking");

            if (dustParticleSystem)
            {
                if (_emission.enabled)
                {
                    _emission.enabled = false; // Applies the new value directly to the Particle System
                }
            }

            _timeSinceUngrounded += Time.fixedDeltaTime;
        }

        CharacterMove(_moveInput);
        CharacterJump(_jumpInput, grounded, rayHit);

        if (rayHitGround && _shouldMaintainHeight)
        {
            MaintainHeight(rayHit);
        }

        var lookDirection = GetLookDirection(characterLookDirection);
        MaintainUpright(lookDirection, rayHit);

        _prevGrounded = grounded;
    }

    /// <summary>
    /// Perform raycast towards the ground.
    /// </summary>
    /// <returns>Whether the ray hit the ground, and information about the ray.</returns>
    private (bool, RaycastHit) RaycastToGround()
    {
        var rayToGround = new Ray(transform.position, _rayDir);
        var rayHitGround = Physics.Raycast(rayToGround, out var rayHit, rayToGroundLength, terrainLayer.value);
        // Debug.DrawRay(transform.position, _rayDir * _rayToGroundLength, Color.blue);
        return (rayHitGround, rayHit);
    }

    /// <summary>
    /// Determines the relative velocity of the character to the ground beneath,
    /// Calculates and applies the oscillator force to bring the character towards the desired ride height.
    /// Additionally applies the oscillator force to the squash and stretch oscillator, and any object beneath.
    /// </summary>
    /// <param name="rayHit">Information about the RaycastToGround.</param>
    private void MaintainHeight(RaycastHit rayHit)
    {
        var vel = _rb.velocity;
        var otherVel = Vector3.zero;
        var hitBody = rayHit.rigidbody;
        if (hitBody != null)
        {
            otherVel = hitBody.velocity;
        }
        var rayDirVel = Vector3.Dot(_rayDir, vel);
        var otherDirVel = Vector3.Dot(_rayDir, otherVel);

        var relVel = rayDirVel - otherDirVel;
        var currHeight = rayHit.distance - rideHeight;
        var springForce = (currHeight * rideSpringStrength) - (relVel * rideSpringDamper);
        var maintainHeightForce = - _gravitationalForce + springForce * Vector3.down;
        var oscillationForce = springForce * Vector3.down;
        _rb.AddForce(maintainHeightForce);
        squashAndStretchOscillator.ApplyForce(oscillationForce);
        // Debug.DrawLine(transform.position, transform.position + (_rayDir * springForce), Color.yellow);
        // Apply force to objects beneath
        if (hitBody != null)
        {
            hitBody.AddForceAtPosition(-maintainHeightForce, rayHit.point);
        }
    }

    /// <summary>
    /// Determines the desired y rotation for the character, with account for platform rotation.
    /// </summary>
    /// <param name="yLookAt">The input look rotation.</param>
    /// <param name="rayHit">The rayHit towards the platform.</param>
    private void CalculateTargetRotation(Vector3 yLookAt, RaycastHit rayHit = new RaycastHit())
    {
        if (_didLastRayHit)
        {
            _lastTargetRot = _uprightTargetRot;
            try
            {
                _platformInitRot = transform.parent.rotation.eulerAngles;
            }
            catch
            {
                _platformInitRot = Vector3.zero;
            }
        }
        _didLastRayHit = rayHit.rigidbody == null;

        if (yLookAt != Vector3.zero)
        {
            _uprightTargetRot = Quaternion.LookRotation(yLookAt, Vector3.up);
            _lastTargetRot = _uprightTargetRot;
            try
            {
                _platformInitRot = transform.parent.rotation.eulerAngles;
            }
            catch
            {
                _platformInitRot = Vector3.zero;
            }
        }
        else
        {
            try
            {
                var platformRot = transform.parent.rotation.eulerAngles;
                var deltaPlatformRot = platformRot - _platformInitRot;
                var yAngle = _lastTargetRot.eulerAngles.y + deltaPlatformRot.y;
                _uprightTargetRot = Quaternion.Euler(new Vector3(0f, yAngle, 0f));
            }
            catch (Exception)
            {
                // Ignored.
            }
        }
    }

    /// <summary>
    /// Adds torque to the character to keep the character upright, acting as a torsional oscillator (i.e. vertically flipped pendulum).
    /// </summary>
    /// <param name="yLookAt">The input look rotation.</param>
    /// <param name="rayHit">The rayHit towards the platform.</param>
    private void MaintainUpright(Vector3 yLookAt, RaycastHit rayHit = new RaycastHit())
    {
        CalculateTargetRotation(yLookAt, rayHit);

        var currentRot = transform.rotation;
        var toGoal = MathsUtils.ShortestRotation(_uprightTargetRot, currentRot);

        toGoal.ToAngleAxis(out var rotDegrees, out var rotAxis);
        rotAxis.Normalize();

        var rotRadians = rotDegrees * Mathf.Deg2Rad;

        _rb.AddTorque((rotAxis * (rotRadians * uprightSpringStrength)) - (_rb.angularVelocity * uprightSpringDamper));
    }

    /// <summary>
    /// Reads the player movement input.
    /// </summary>
    /// <param name="context">The move input's context.</param>
    public void MoveInputAction(InputAction.CallbackContext context)
    {
        _moveContext = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Reads the player jump input.
    /// </summary>
    /// <param name="context">The jump input's context.</param>
    public void JumpInputAction(InputAction.CallbackContext context)
    {
        var jumpContext = context.ReadValue<float>();
        _jumpInput = new Vector3(0, jumpContext, 0);

        if (context.started) // button down
        {
            _timeSinceJumpPressed = 0f;
        }
    }

    /// <summary>
    /// Adjusts the input, so that the movement matches input regardless of camera rotation.
    /// </summary>
    /// <param name="moveInput">The player movement input.</param>
    /// <returns>The camera corrected movement input.</returns>
    private Vector3 AdjustInputToFaceCamera(Vector3 moveInput)
    {
        Debug.Assert(Camera.main != null, "Camera.main != null");
        var facing = Camera.main.transform.eulerAngles.y;
        return (Quaternion.Euler(0, facing, 0) * moveInput);
    }

    /// <summary>
    /// Set the transform parent to be the result of RaycastToGround.
    /// If the raycast didn't hit, then unset the transform parent.
    /// </summary>
    /// <param name="rayHit">The rayHit towards the platform.</param>
    private void SetPlatform(RaycastHit rayHit)
    {
        try
        {
            var rigidPlatform = rayHit.transform.GetComponent<RigidPlatform>();
            var rigidParent = rigidPlatform.rigidParent;
            transform.SetParent(rigidParent.transform);
        }
        catch
        {
            transform.SetParent(null);
        }
    }

    /// <summary>
    /// Apply forces to move the character up to a maximum acceleration, with consideration to acceleration graphs.
    /// </summary>
    /// <param name="moveInput">The player movement input.</param>
    private void CharacterMove(Vector3 moveInput)
    {
        var mUnitGoal = moveInput;
        var unitVel = _mGoalVel.normalized;
        var velDot = Vector3.Dot(mUnitGoal, unitVel);
        var accel = acceleration * accelerationFactorFromDot.Evaluate(velDot);
        var goalVel = mUnitGoal * (maxSpeed * _speedFactor);
        _mGoalVel = Vector3.MoveTowards(_mGoalVel,
                                        goalVel,
                                        accel * Time.fixedDeltaTime);
        var neededAccel = (_mGoalVel - _rb.velocity) / Time.fixedDeltaTime;
        var maxAccel = maxAccelForce * maxAccelerationForceFactorFromDot.Evaluate(velDot) * _maxAccelForceFactor;
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
        Transform transform1;
        _rb.AddForceAtPosition(Vector3.Scale(neededAccel * _rb.mass, moveForceScale), (transform1 = transform).position + new Vector3(0f, transform1.localScale.y * leanFactor, 0f)); // Using AddForceAtPosition in order to both move the player and cause the play to lean in the direction of input.
    }

    /// <summary>
    /// Apply force to cause the character to perform a single jump, including coyote time and a jump input buffer.
    /// </summary>
    /// <param name="jumpInput">The player jump input.</param>
    /// <param name="grounded">Whether or not the player is considered grounded.</param>
    /// <param name="rayHit">The rayHit towards the platform.</param>
    private void CharacterJump(Vector3 jumpInput, bool grounded, RaycastHit rayHit)
    {
        _timeSinceJumpPressed += Time.fixedDeltaTime;
        _timeSinceJump += Time.fixedDeltaTime;
        if (_rb.velocity.y < 0)
        {
            _shouldMaintainHeight = true;
            _jumpReady = true;
            if (!grounded)
            {
                // Increase downforce for a sudden plummet.
                _rb.AddForce(_gravitationalForce * (fallGravityFactor - 1f)); // Hmm... this feels a bit weird. I want a reactive jump, but I don't want it to dive all the time...
            }
        }
        else if (_rb.velocity.y > 0)
        {
            if (!grounded)
            {
                if (_isJumping)
                {
                    _rb.AddForce(_gravitationalForce * (riseGravityFactor - 1f));
                }
                if (jumpInput == Vector3.zero)
                {
                    // Impede the jump height to achieve a low jump.
                    _rb.AddForce(_gravitationalForce * (lowJumpFactor - 1f));
                }
            }
        }

        if (_timeSinceJumpPressed < jumpBuffer)
        {
            if (_timeSinceUngrounded < coyoteTime)
            {
                if (_jumpReady)
                {
                    _jumpReady = false;
                    _shouldMaintainHeight = false;
                    _isJumping = true;
                    var velocity = _rb.velocity;
                    velocity = new Vector3(velocity.x, 0f, velocity.z); // Cheat fix... (see comment below when adding force to rigidbody).
                    _rb.velocity = velocity;
                    if (rayHit.distance != 0) // i.e. if the ray has hit
                    {
                        var position = _rb.position;
                        position = new Vector3(position.x, position.y - (rayHit.distance - rideHeight), position.z);
                        _rb.position = position;
                    }
                    _rb.AddForce(Vector3.up * jumpForceFactor, ForceMode.Impulse); // This does not work very consistently... Jump height is affected by initial y velocity and y position relative to RideHeight... Want to adopt a fancier approach (more like PlayerMovement). A cheat fix to ensure consistency has been issued above...
                    _timeSinceJumpPressed = jumpBuffer; // So as to not activate further jumps, in the case that the player lands before the jump timer surpasses the buffer.
                    _timeSinceJump = 0f;

                    FindObjectOfType<AudioManager>().Play("Jump");
                }
            }
        }
    }
}
