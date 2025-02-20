using UnityEngine;
using Fusion; // Importing Fusion for multiplayer networking

public class ShellExplosionMP : NetworkBehaviour
{
    // Layer mask to detect tanks in the explosion radius
    public LayerMask tankMask;

    // Particle system for the explosion effect
    public ParticleSystem explosionParticles;

    // Audio source for explosion sound
    public AudioSource explosionAudio;

    // Maximum damage the explosion can cause
    public float maxDamage = 100f;

    // Force applied to nearby objects during explosion
    public float explosionForce = 1000f;

    // Array to store colliders affected by the explosion
    public Collider[] colliders = new Collider[3];

    // TickTimer to control the lifetime of the shell
    [Networked] private TickTimer maxLifeTime { get; set; }

    // Explosion radius within which damage is applied
    public float explosionRadius = 5f;

    // Initialization method to set the shell's lifetime
    public void Init()
    {
        maxLifeTime = TickTimer.CreateFromSeconds(Runner, 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Find all objects within the explosion radius that belong to the tankMask layer
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, tankMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            // Get the Rigidbody of the target
            Rigidbody targetRb = colliders[i].GetComponent<Rigidbody>();

            // Get the NetworkObject associated with the target
            NetworkObject targetNo = targetRb.GetComponent<NetworkObject>();

            // If no Rigidbody is found, skip this target
            if (!targetRb)
                continue;

            // Apply explosion force to the target
            targetRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            // Get the health component of the target tank
            TankHealthMP targetHealth = targetRb.gameObject.GetComponent<TankHealthMP>();

            // If the target has no health component, skip it
            if (!targetHealth)
                continue;

            // Calculate the damage based on the target's position
            float damage = CalculateDamage(targetRb.position);

            // Apply damage to the target using an RPC function
            targetHealth.TakeDamageRpc(damage);

            // Detach explosion particles from the shell and play the effect
            explosionParticles.transform.parent = null;
            explosionParticles.Play();
            explosionAudio.Play();

            // Destroy the explosion effect after 3 seconds
            Destroy(explosionParticles.gameObject, 3f);
            Destroy(this.gameObject); // Destroy the shell after explosion
        }

        // If no valid target was hit, still trigger the explosion effect
        explosionParticles.transform.parent = null;
        explosionParticles.Play();
        explosionAudio.Play();

        // Destroy the explosion effect and the shell object after 3 seconds
        Destroy(explosionParticles.gameObject, 3f);
        Destroy(this.gameObject);
    }

    private float CalculateDamage(Vector3 targetPosition)
    {
        // Calculate damage based on the target's distance from the explosion center

        // Compute the vector from explosion center to target
        Vector3 explosionToTarget = targetPosition - transform.position;

        // Get the distance from the explosion center to the target
        float explosionDistance = explosionToTarget.magnitude;

        // Calculate the relative distance (0 = farthest, 1 = closest)
        float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;

        // Damage is proportional to the target's proximity to the explosion center
        float damage = relativeDistance * maxDamage;

        // Ensure damage is not negative
        damage = Mathf.Max(0f, damage);

        return damage;
    }
}
