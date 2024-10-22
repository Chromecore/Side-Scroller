using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDataHolder", menuName = "SoundDataHolder")]
public class GeneralSoundData : SerializedScriptableObject
{
    public Dictionary<GeneralSound, SoundData> soundDatas = new();
}

public enum GeneralSound
{
    none = 0,
    jump = 3, grappleHit = 4,
    death = 1,
    spawn = 2,
};