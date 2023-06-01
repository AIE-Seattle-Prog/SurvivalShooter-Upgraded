using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int startingHealth = 100;                            // The amount of health the player starts the game with.
    public int currentHealth;                                   // The current health the player has.
    public UnityEvent<int> OnHealthChanged { get; private set; } = new();
    [SerializeField]
    private AudioClipMetadata deathClip;                        // The audio clip to play when the player dies.
    public float flashSpeed = 5f;                               // The speed the damageImage will fade at.
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);     // The colour the damageImage is set to, to flash.
    
    [SerializeField]
    private Animator anim;                                              // Reference to the Animator component.
    [SerializeField]
    private AudioSource playerAudio;                                    // Reference to the AudioSource component.
    [SerializeField]
    private AudioClipMetadata playerHurtSound;
    [SerializeField]
    private PlayerMovement playerMovement;                              // Reference to the player's movement.
    [SerializeField]
    private PlayerShooting playerShooting;                              // Reference to the PlayerShooting script.
    
    private bool isDead;                                                // Whether the player is dead.
    private bool damaged;                                               // True when the player gets damaged.

    void Start()
    {
        // Set the initial health of the player.
        currentHealth = startingHealth;
    }
    
    void Update ()
    {
        // Reset the damaged flag.
        damaged = false;
    }


    public void TakeDamage (int amount)
    {
        if(amount == 0) return;

        // Set the damaged flag so the screen will flash.
        damaged = true;

        // Reduce the current health by the damage amount.
        currentHealth -= amount;

        // Play the hurt sound effect.
        playerAudio.PlayOneShot(playerHurtSound);

        // If the player has lost all it's health and the death flag hasn't been set yet...
        if (currentHealth <= 0 && !isDead)
        {
            // ... it should die.
            Death ();
        }

        // Notify OnHealthChanged
        OnHealthChanged.Invoke(currentHealth);
    }


    void Death ()
    {
        // prevent repeat calls
        if(isDead) { return; }

        // Set the death flag so this function won't be called again.
        isDead = true;

        // Turn off any remaining shooting effects.
        playerShooting.DisableEffects ();

        // Tell the animator that the player is dead.
        anim.SetTrigger ("Die");

        // Stop audiosource the hurt sound from playing and play the death sound.
        playerAudio.Stop();
        playerAudio.PlayOneShot(deathClip);

        // Turn off the movement and shooting scripts.
        playerMovement.enabled = false;
        playerShooting.enabled = false;
    }
}