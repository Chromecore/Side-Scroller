using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class SoundManager : PersistentSingleton<SoundManager>
{
    private IObjectPool<SoundEmitter> soundEmitterPool;
    readonly List<SoundEmitter> activeSoundEmitters = new();
    public readonly Queue<SoundEmitter> FrequentSoundEmitters = new();

    [SerializeField] private SoundEmitter soundEmitterPrefab;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private int maxSoundInstances = 30;

    [Title("References")]
    [SerializeField, Required] private GeneralSoundData soundDataHolder;
    public GeneralSoundData SoundDataHolder { get { return soundDataHolder; } }
    [SerializeField, Required, AssetsOnly] private AudioMixer audioMixer;

    protected override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    private static float GetVolume(float normalizedVolume, float maxValue)
    {
        if (normalizedVolume == 0) return -80;
        float minSoundVolume = 45;
        return (normalizedVolume * (maxValue + minSoundVolume)) - minSoundVolume;
    }

    public static void PlayClickSound(GeneralSound generalSound)
    {
        Instance.CreateSound().WithRandomPitch().Play(generalSound);
    }

    public SoundBuilder CreateSound() => new SoundBuilder(this);

    public bool CanPlaySound(SoundData data)
    {
        if (!data.frequentSound) return true;

        if (FrequentSoundEmitters.Count >= maxSoundInstances && FrequentSoundEmitters.TryDequeue(out var soundEmitter))
        {
            try
            {
                soundEmitter.Stop();
                return true;
            }
            catch
            {
                Debug.Log("SoundEmitter is already released");
            }

            return false;
        }

        return true;
    }

    public SoundEmitter Get()
    {
        return soundEmitterPool.Get();
    }

    public void ReturnToPool(SoundEmitter soundEmitter)
    {
        soundEmitterPool.Release(soundEmitter);
    }

    public void StopAll()
    {
        foreach (SoundEmitter soundEmitter in activeSoundEmitters)
        {
            soundEmitter.Stop();
        }

        FrequentSoundEmitters.Clear();
    }

    private void InitializePool()
    {
        soundEmitterPool = new ObjectPool<SoundEmitter>(
            CreateSoundEmitter,
            OnTakeFromPool,
            OnReaturnedToPool,
            OnDestroyPoolObject,
            collectionCheck,
            defaultCapacity,
            maxPoolSize
        );
    }

    private SoundEmitter CreateSoundEmitter()
    {
        SoundEmitter soundEmitter = Instantiate(soundEmitterPrefab);
        soundEmitter.gameObject.SetActive(false);
        return soundEmitter;
    }

    private void OnTakeFromPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(true);
        activeSoundEmitters.Add(soundEmitter);
    }

    private void OnReaturnedToPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(false);
        activeSoundEmitters.Remove(soundEmitter);
    }

    private void OnDestroyPoolObject(SoundEmitter soundEmitter)
    {
        Destroy(soundEmitter.gameObject);
    }
}