using System.Collections;
using System.Collections.Generic;
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
    [SerializeField, Required] private ParticleSystem endParticles;
    [SerializeField, Required] private ParticleSystem pickupDropParticles;
    [SerializeField, Required] private GameObject sprite;
    [SerializeField, Required] private TMP_Text pickup2Text;
    [SerializeField, Required] private TMP_Text deathsText;

    private Vector3 currentSpawn;
    private bool isDead;
    private int pickup2Count;
    private int deaths;

    private List<Pickup> pickup2sSinceLastCheckpoint = new();

    private void Reset()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        pickup2Text.text = $"{pickup2Count}/{pickup2Total}";
        deathsText.text = "0";
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
        else if (other.CompareTag("CheckpointTrigger") || other.CompareTag("Ending"))
        {
            SetCheckpoint(other.transform, other.CompareTag("Ending"));
        }
        else if (other.CompareTag("Pickup2"))
        {
            pickup2sSinceLastCheckpoint.Add(other.GetComponent<Pickup>());
            pickup2Count++;
            UpdatePickupUI();
        }
    }

    private void SetCheckpoint(Transform checkpoint, bool ending)
    {
        if (currentSpawn == checkpoint.position) return;
        pickup2sSinceLastCheckpoint.Clear();
        currentSpawn = checkpoint.position;
        SoundManager.Instance.CreateSound()
            .WithRandomPitch()
            .Play(GeneralSound.checkpoint);
        if (ending)
        {
            endParticles.Play();
        }
        else
        {
            checkpointParticles.Play();
        }
    }

    private void Die()
    {
        if (isDead) return;
        StartCoroutine(HandleDie());
    }

    private IEnumerator HandleDie()
    {
        foreach (Pickup pickup in pickup2sSinceLastCheckpoint)
        {
            pickup.Reset();
            pickup2Count--;
            UpdatePickupUI();
            pickupDropParticles.Play();
        }
        pickup2sSinceLastCheckpoint.Clear();
        SoundManager.Instance.CreateSound()
            .WithRandomPitch()
            .Play(GeneralSound.death);
        isDead = true;
        deaths++;
        deathsText.text = deaths.ToString();
        playerMovement.Die();
        deathParticles.Play();
        sprite.SetActive(false);
        yield return new WaitForSeconds(deathTime);
        Spawn();
    }

    private void UpdatePickupUI()
    {
        pickup2Text.text = $"{pickup2Count}/{pickup2Total}";
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