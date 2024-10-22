using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    public SoundData Data { get; private set; }
    private AudioSource audioSource;
    private Coroutine playingCoroutine;

    private bool isStopped;

    private void Awake()
    {
        audioSource = GetOrAdd<AudioSource>();
    }

    public T GetOrAdd<T>() where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (!component) component = gameObject.AddComponent<T>();

        return component;
    }

    public void Initialize(SoundData data)
    {
        isStopped = false;
        Data = data;
        audioSource.clip = data.clip;
        audioSource.outputAudioMixerGroup = data.mixerGroup;
        audioSource.loop = data.loop;
        audioSource.playOnAwake = data.playOnAwake;

        audioSource.priority = data.priority;
        audioSource.volume = data.volumne;
        audioSource.pitch = data.pitch;
    }

    public void Play()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }

        audioSource.Play();
        playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    private IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        Stop();
    }

    public void Stop()
    {
        if (isStopped) return;
        isStopped = true;
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        audioSource.Stop();
        SoundManager.Instance.ReturnToPool(this);
    }

    public void WithRandomPitch(float min, float max)
    {
        audioSource.pitch += Random.Range(min, max);
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}