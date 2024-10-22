using System.Collections;
using Chromecore;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    [SerializeField, Min(0), Unit(Units.Second)] private float deathTime;
    [SerializeField, Min(0)] private float pickup2Total;

    [Title("References")]
    [SerializeField, Required] private PlayerMovement playerMovement;
    [SerializeField, Required] private ParticleSystem deathParticles;
    [SerializeField, Required] private ParticleSystem checkpointParticles;
    [SerializeField, Required] private GameObject sprite;
    [SerializeField, Required] private TMP_Text pickup2Text;

    private Vector3 currentSpawn;
    private bool isDead;
    private int pickup2Count;

    private void Reset()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Awake()
    {
        pickup2Text.text = $"{pickup2Count}/{pickup2Total}";
        currentSpawn = transform.position;
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
        else if (other.CompareTag("Pickup2"))
        {
            pickup2Count++;
            pickup2Text.text = $"{pickup2Count}/{pickup2Total}";
        }
    }

    private void SetCheckpoint(Transform checkpoint)
    {
        if (currentSpawn == checkpoint.position) return;
        currentSpawn = checkpoint.position;
        checkpointParticles.Play();
    }

    private void Die()
    {
        if (isDead) return;
        StartCoroutine(HandleDie());
    }

    private IEnumerator HandleDie()
    {
        SoundManager.Instance.CreateSound()
            .WithRandomPitch()
            .Play(GeneralSound.death);
        isDead = true;
        playerMovement.Die();
        deathParticles.Play();
        sprite.SetActive(false);
        yield return new WaitForSeconds(deathTime);
        Spawn();
    }

    private void Spawn()
    {
        SoundManager.Instance.CreateSound()
            .WithRandomPitch()
            .Play(GeneralSound.spawn);
        isDead = false;
        transform.position = currentSpawn;
        sprite.SetActive(true);
        playerMovement.Spawn();
    }
}