using Sirenix.OdinInspector;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField, Min(0)] private float bounceSpeed;
    [SerializeField, Min(0)] private float bounceHeight;
    [SerializeField, Required] private Transform visualsParent;

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
        Destroy(gameObject);
    }
}