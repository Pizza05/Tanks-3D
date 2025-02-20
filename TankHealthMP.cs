using UnityEngine;
using UnityEngine.UI;
using Fusion; // Importing Fusion for networking

public class TankHealthMP : NetworkBehaviour
{
    // Initial health value
    public float startingHealth = 100f;

    // UI components for displaying health
    public Slider slider;
    public Image fillImage;

    // Colors representing full health and zero health
    public Color fullHealthColor = Color.green;
    public Color zeroHealthColor = Color.red;

    // Prefab for explosion effect when the tank is destroyed
    public GameObject explosionPrefab;

    // Components for handling explosion sound and particles
    private AudioSource _explosionAudio;
    private ParticleSystem _explosionParticles;

    // Networked variable to track if the tank is dead, with UI updates when it changes
    [Networked, OnChangedRender(nameof(OnDeath))]
    public bool _isDead { get; set; }

    // Networked string to store the player's name
    [Networked] public NetworkString<_16> playerName { get; set; }

    // Networked variable for the tank's current health, updates UI when changed
    [Networked, OnChangedRender(nameof(SetHealthUI))]
    public float _currentHealth { get; set; }

    // Reference to the Network Manager
    public GameObject networkManager;

    private void Update()
    {
        // Ensure the network manager reference is always valid
        if (networkManager == null)
        {
            networkManager = GameObject.Find("NetworkManager");

            // Register this tank with the Network Manager
            networkManager.GetComponent<NetworkManager>().addTank(this.Object);
        }
    }

    public override void Spawned()
    {
        // Instantiate explosion effects and audio
        _explosionParticles = Instantiate(explosionPrefab).GetComponent<ParticleSystem>();
        _explosionAudio = _explosionParticles.GetComponent<AudioSource>();

        // Initially deactivate the explosion effect
        _explosionParticles.gameObject.SetActive(false);

        // Respawn the tank with full health
        Respawn();
    }

    public void Respawn()
    {
        // Reset health and revive the tank
        _currentHealth = startingHealth;
        _isDead = false;

        // Update UI to reflect the health reset
        SetHealthUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void TakeDamageRpc(float amount)
    {
        // Reduce the tank's current health
        _currentHealth -= amount;

        // Update UI
        SetHealthUI();

        // Check if the tank has died
        if (_currentHealth <= 0f && !_isDead)
        {
            // Play explosion effect at the tank's position
            _explosionParticles.transform.position = this.transform.position;
            _explosionParticles.gameObject.SetActive(true);
            _explosionParticles.Play();

            // Play explosion sound
            _explosionAudio.Play();

            // Mark the tank as dead
            _isDead = true;
        }
    }

    public void SetHealthUI()
    {
        // Update the health bar value
        slider.value = _currentHealth;

        // Change the color of the health bar based on the health percentage
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, _currentHealth / startingHealth);
    }

    private void OnDeath()
    {
        // If the tank is dead, notify the network manager to mark it as inactive
        if (_isDead)
        {
            networkManager.GetComponent<NetworkManager>().setTankInactive(Object);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void HealRpc(int hp)
    {
        // Restore health, ensuring it does not exceed the starting health
        if (_currentHealth + hp > startingHealth)
            _currentHealth = startingHealth;
        else
            _currentHealth += hp;

        // Update UI to reflect the new health value
        SetHealthUI();
    }
}
