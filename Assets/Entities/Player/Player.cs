using System.Collections;
using Chromecore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    [SerializeField, Min(0), Unit(Units.Second)] private float deathTime;

    [Title("References")]
    [SerializeField, Required] private PlayerMovement playerMovement;
    [SerializeField, Required] private ParticleSystem deathParticles;
    [SerializeField, Required] private GameObject sprite;
    [SerializeField, Required] private Transform mainSpawn;

    private void Reset()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Awake()
    {
        PlayerPrefs.DeleteAll();
        Spawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Spike"))
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        playerMovement.Die();
        deathParticles.Play();
        sprite.SetActive(false);
        yield return new WaitForSeconds(deathTime);
        Spawn();
    }

    private void Spawn()
    {
        transform.position = mainSpawn.position;
        sprite.SetActive(true);
        playerMovement.Spawn();
    }
}