using System;
using UnityEngine;

namespace Experimental
{
    [Serializable]
    public class GameplayCueData
    {
        public string VFXPath;
        public string AudioPath;
        // Add other effect paths as needed

        public bool HasVFXPath => !string.IsNullOrEmpty(VFXPath);
        public bool HasAudioPath => !string.IsNullOrEmpty(AudioPath);
    }

    [CreateAssetMenu(fileName = "GameplayCueSystemSettings", menuName = "Common/Experimental/Gameplay Cue System Settings")]
    public class GameplayCueSystemSettings : ScriptableObject
    {
        public SerializedDictionary<GameplayTag, GameplayCueData> cueDatas;

        public bool TryGetCueData(GameplayTag tag, out GameplayCueData data)
        {
            return cueDatas.TryGetValue(tag, out data);
        }
    }
}