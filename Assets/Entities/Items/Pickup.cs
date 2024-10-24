using Sirenix.OdinInspector;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField, Min(0)] private float bounceSpeed;
    [SerializeField, Min(0)] private float bounceHeight;
    [SerializeField, Required] private Transform visualsParent;
    [SerializeField, AssetsOnly] private ParticleSystem deathParticles;
    [SerializeField] private SoundData deathAudio;

    private Vector2 position;
    private float offset;

    private void Awake()
    {
        offset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        position.y = Mathf.Sin((Time.time + offset) * bounceSpeed + bounceSpeed / 4f) * bounceHeight;
        visualsParent.localPosition = position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (deathParticles != null) Instantiate(deathParticles, transform.position, Quaternion.identity);
        SoundManager.Instance.CreateSound()
            .WithRandomPitch()
            .Play(deathAudio);
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        gameObject.SetActive(true);
    }
}