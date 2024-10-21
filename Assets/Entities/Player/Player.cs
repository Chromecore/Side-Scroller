using System.Collections;
using Chromecore;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    [SerializeField, Min(0), Unit(Units.Second)] private float deathTime;

    [Title("References")]
    [SerializeField, Required] private PlayerMovement playerMovement;
    [SerializeField, Required] private ParticleSystem deathParticles;
    [SerializeField, Required] private ParticleSystem checkpointParticles;
    [SerializeField, Required] private GameObject sprite;
    [SerializeField, Required] private Transform mainSpawn;

    private Transform currentSpawn;
    private bool isDead;

    private void Reset()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Awake()
    {
        currentSpawn = mainSpawn;
        PlayerPrefs.DeleteAll();
        Spawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Spike"))
        {
            Die();
        }
        else if (other.CompareTag("CheckpointTrigger"))
        {
            SetCheckpoint(other.transform);
        }
    }

    private void SetCheckpoint(Transform checkpoint)
    {
        if (currentSpawn == checkpoint) return;
        currentSpawn = checkpoint;
        checkpointParticles.Play();
    }

    private void Die()
    {
        if (isDead) return;
        StartCoroutine(HandleDie());
    }

    private IEnumerator HandleDie()
    {
        isDead = true;
        playerMovement.Die();
        deathParticles.Play();
        sprite.SetActive(false);
        yield return new WaitForSeconds(deathTime);
        Spawn();
    }

    private void Spawn()
    {
        isDead = false;
        transform.position = currentSpawn.position;
        sprite.SetActive(true);
        playerMovement.Spawn();
    }
}