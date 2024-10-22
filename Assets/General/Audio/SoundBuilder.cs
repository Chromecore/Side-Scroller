using UnityEngine;

public class SoundBuilder
{
    private readonly SoundManager soundManager;
    private Vector3 position = Vector3.zero;
    private bool randomPitch;
    private float randomPitchMin = -0.08f;
    private float randomPitchMax = 0.08f;

    public SoundBuilder(SoundManager soundManager)
    {
        this.soundManager = soundManager;
    }

    public SoundBuilder WithPosition(Vector3 position)
    {
        this.position = position;
        return this;
    }

    public SoundBuilder WithRandomPitch(float min = -0.08f, float max = 0.08f)
    {
        this.randomPitch = true;
        randomPitchMin = min;
        randomPitchMax = max;
        return this;
    }

    public SoundEmitter Play(GeneralSound sound)
    {
        if (sound == GeneralSound.none) return null;
        soundManager.SoundDataHolder.soundDatas.TryGetValue(sound, out SoundData soundData);
        if (soundData != null) return Play(soundData);
        return null;
    }

    public SoundEmitter Play(SoundData soundData)
    {
        if (soundData == null) return null;

        if (!soundManager.CanPlaySound(soundData)) return null;

        SoundEmitter soundEmitter = soundManager.Get();
        soundEmitter.Initialize(soundData);
        soundEmitter.transform.position = position;
        soundEmitter.transform.parent = soundManager.transform;

        if (randomPitch)
        {
            soundEmitter.WithRandomPitch(randomPitchMin, randomPitchMax);
        }

        if (soundData.frequentSound)
        {
            soundManager.FrequentSoundEmitters.Enqueue(soundEmitter);
        }
        soundEmitter.Play();
        return soundEmitter;
    }
}