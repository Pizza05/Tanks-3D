using UnityEngine;
using UnityEngine.UI;
using Fusion; // Importing Fusion for multiplayer networking

public class TankShootingMP : NetworkBehaviour
{
    // The projectile (shell) prefab that will be instantiated
    public GameObject shellRb;

    // The position from which the shell will be fired
    public Transform fireTransform;

    // UI slider for aiming power
    public Slider aimSlider;

    // Audio source for playing shooting sounds
    public AudioSource shootingAudio;

    // Audio clips for charging and firing sounds
    public AudioClip chargingClip;
    public AudioClip fireClip;

    // Minimum force applied to the shell when fired
    public float minLaunchForce = 15f;

    // Maximum force applied to the shell when fired
    public float maxLaunchForce = 30f;

    // Time required to fully charge the shot
    public float maxChargeTime = 0.75f;

    // Current launch force of the shell
    private float _currentLaunchForce;

    // Speed at which the charge increases
    private float _chargeSpeed;

    // Indicates whether the shell has been fired
    private bool _fired;

    // Indicates whether the fire button is currently being held down
    private bool _isDown;

    private void OnEnable()
    {
        // Reset the launch force and UI slider when the script is enabled
        _currentLaunchForce = minLaunchForce;
        aimSlider.value = minLaunchForce;
    }

    private void Start()
    {
        // Calculate the charging speed based on the max charge time
        _chargeSpeed = (maxLaunchForce - minLaunchForce) / maxChargeTime;
    }

    public override void Render()
    {
        // Empty method for rendering updates if needed (Fusion multiplayer)
    }

    public override void FixedUpdateNetwork()
    {
        // Reset the aim slider to minimum launch force at the start of each update
        aimSlider.value = minLaunchForce;

        // If the launch force reaches the maximum limit and hasn't been fired yet, fire automatically
        if (_currentLaunchForce >= maxLaunchForce && !_fired)
        {
            _isDown = false;
            _currentLaunchForce = maxLaunchForce;
            Fire();
        }
        // If the fire button is pressed (charging the shot)
        else if (!_isDown && Input.GetButton("Fire1"))
        {
            _isDown = true;
            _fired = false;
            _currentLaunchForce = minLaunchForce;

            // Play the charging sound
            shootingAudio.clip = chargingClip;
            shootingAudio.Play();
        }
        // While the fire button is being held, increase the launch force
        else if (_isDown && Input.GetButton("Fire1") && !_fired)
        {
            _currentLaunchForce += _chargeSpeed * Runner.DeltaTime;
            aimSlider.value = _currentLaunchForce;
        }
        // When the fire button is released, fire the shell
        else if (_isDown && !_fired)
        {
            _isDown = false;
            Fire();
        }
    }

    public void Fire()
    {
        _fired = true;

        // Spawn the shell using Fusion's networking system
        NetworkObject shellInstance = Runner.Spawn(
            shellRb, // The projectile prefab
            fireTransform.position, // Spawn position
            fireTransform.rotation, // Spawn rotation
            Object.InputAuthority, // Authority over the object
            (runner, obj) => { obj.GetComponent<ShellExplosionMP>().Init(); } // Initialize shell properties
        );

        // Apply velocity to the shell
        shellInstance.GetComponent<Rigidbody>().velocity = _currentLaunchForce * fireTransform.forward;

        // Play the firing sound effect
        shootingAudio.clip = fireClip;
        shootingAudio.Play();

        // Reset the launch force after firing
        _currentLaunchForce = minLaunchForce;
    }
}

