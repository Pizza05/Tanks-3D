using UnityEngine;
using Fusion; // Importing Fusion for networking

public class TankMovementMP : NetworkBehaviour
{
    // Tank movement speed
    public float speed = 12f;

    // Rotation speed when turning
    public float turnSpeed = 180f;

    // Audio source for tank movement sounds
    public AudioSource movementAudio;

    // Sound clips for idle and moving states
    public AudioClip engineIdling;
    public AudioClip engineDriving;

    // Random pitch variation range for engine sound
    public float pitchRange = 0.2f;

    // Rigidbody for physics-based movement
    private Rigidbody _rb;

    // Player input values
    private float _movementInput;
    private float _turnInput;

    // Stores the original pitch of the engine sound
    private float _originalPitch;

    private void Awake()
    {
        // Get reference to the Rigidbody component
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // Enable physics interactions
        _rb.isKinematic = false;

        // Reset movement inputs
        _movementInput = 0f;
        _turnInput = 0f;
    }

    private void OnDisable()
    {
        // Disable physics interactions when the object is not active
        _rb.isKinematic = true;
    }

    private void Start()
    {
        // Store the original pitch of the engine sound
        _originalPitch = movementAudio.pitch;
    }

    public override void FixedUpdateNetwork()
    {
        // Get player input for movement and turning
        _movementInput = Input.GetAxis("Vertical1");
        _turnInput = Input.GetAxis("Horizontal1");

        // Play appropriate engine sound
        EngineAudio();

        // Move and rotate the tank based on player input
        Move();
        Turn();
    }

    private void EngineAudio()
    {
        // Check if the tank is moving or stationary
        if (Mathf.Abs(_movementInput) < 0.1f && Mathf.Abs(_turnInput) < 0.1f)
        {
            // If the tank is idle, switch to idle engine sound
            if (movementAudio.clip == engineDriving)
            {
                movementAudio.clip = engineIdling;
                movementAudio.pitch = Random.Range(_originalPitch - pitchRange, _originalPitch + pitchRange);
                movementAudio.Play();
            }
        }
        else
        {
            // If the tank is moving, switch to driving engine sound
            if (movementAudio.clip == engineIdling)
            {
                movementAudio.clip = engineDriving;
                movementAudio.pitch = Random.Range(_originalPitch - pitchRange, _originalPitch + pitchRange);
                movementAudio.Play();
            }
        }
    }

    private void Move()
    {
        // Calculate movement direction based on player input
        Vector3 movement = transform.forward * _movementInput * speed * Runner.DeltaTime;

        // Apply movement to the tank
        _rb.MovePosition(_rb.position + movement);
    }

    private void Turn()
    {
        // Calculate rotation angle based on player input
        float turn = _turnInput * turnSpeed * Runner.DeltaTime;

        // Convert to a Quaternion rotation
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Apply rotation to the tank
        _rb.MoveRotation(_rb.rotation * turnRotation);
    }
}