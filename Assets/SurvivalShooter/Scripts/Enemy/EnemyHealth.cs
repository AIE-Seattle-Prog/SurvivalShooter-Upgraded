using TNRD.Autohook;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [field: SerializeField, AutoHook(ReadOnlyWhenFound = true)]
    public EnemyController Controller { get; private set; }

    public int startingHealth = 100;            // The amount of health the enemy starts the game with.
    public int currentHealth;                   // The current health the enemy has.
    public float sinkSpeed = 2.5f;              // The speed at which the enemy sinks through the floor when dead.
    public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.
    public AudioClipMetadata deathClip;                 // The sound to play when the enemy dies.
    public AudioClipMetadata hurtClip;

    public float knockbackImpulse = 5.0f;

    Animator anim;                              // Reference to the animator.
    AudioSource enemyAudio;                     // Reference to the audio source.
    [SerializeField] ParticleSystem hitParticles;   // Reference to the particle system that plays when the enemy is damaged.
    [SerializeField] ParticleSystem deathParticles; // Reference to the particle system that plays when the enemy is killed.
    CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
    bool isDead;                                // Whether the enemy is dead.
    bool isSinking;                             // Whether the enemy has started sinking through the floor.

    [field: SerializeField]
    public UnityEvent<EnemyHealth> OnDeath { get; private set; }

    void Awake ()
    {
        // Setting up the references.
        anim = GetComponent <Animator> ();
        enemyAudio = GetComponent <AudioSource> ();
        capsuleCollider = GetComponent <CapsuleCollider> ();

        // Setting the current health when the enemy first spawns.
        currentHealth = startingHealth;
    }


    void Update ()
    {
        // If the enemy should be sinking...
        if(isSinking)
        {
            // ... move the enemy down by the sinkSpeed per second.
            transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
        }
    }


    public void TakeDamage (int amount, Vector3 hitPoint, GameObject attacker)
    {
        // If the enemy is dead...
        if(isDead)
            // ... no need to take damage so exit the function.
            return;

        // Play the hurt sound effect.
        enemyAudio.Play(hurtClip);

        // Reduce the current health by the amount of damage sustained.
        currentHealth -= amount;
        
        // Set the position of the particle system to where the hit was sustained.
        hitParticles.transform.position = hitPoint;

        // And play the particles.
        hitParticles.Play();
        
        // Apply knockback on hit, if applicable to this enemy.
        // TODO: cache this component
        if(TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            // Apply knockback
            Vector3 knockVector = (transform.position - attacker.transform.position);
            knockVector.y = 0.0f;
            knockVector.Normalize();
        
            Vector3 hitVector = (transform.position - hitPoint);
            hitVector.y = 0.0f;
            hitVector.Normalize();

            // scale knockback by how close it was to the center of the target
            float knockMag = Mathf.Max(0, Vector3.Dot(knockVector, hitVector));
            knockVector *= knockbackImpulse * knockMag;
            agent.nextPosition += knockVector;
        }

        // If the current health is less than or equal to zero...
        if(currentHealth <= 0)
        {
            // ... the enemy is dead.
            Death ();
        }
    }


    void Death ()
    {
        // The enemy is dead.
        isDead = true;

        // Turn the collider into a trigger so shots can pass through it.
        capsuleCollider.isTrigger = true;

        // Tell the animator that the enemy is dead.
        anim.SetTrigger ("Dead");

        // Change the audio clip of the audio source to the death clip and play it (this will stop the hurt clip playing).
        enemyAudio.Play (deathClip);

        // Play death particles
        deathParticles.Play();

        // Notify death event
        OnDeath.Invoke(this);
    }


    public void StartSinking ()
    {
        // Find and disable the Nav Mesh Agent.
        GetComponent <UnityEngine.AI.NavMeshAgent> ().enabled = false;

        // Find the rigidbody component and make it kinematic (since we use Translate to sink the enemy).
        GetComponent <Rigidbody> ().isKinematic = true;

        // The enemy should no sink.
        isSinking = true;

        // Increase the score by the enemy's score value.
        ScoreManager.score += scoreValue;

        // After 2 seconds destory the enemy.
        Destroy (gameObject, 2f);
    }
}