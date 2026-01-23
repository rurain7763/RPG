using System;
using UnityEngine;

namespace Experimental
{
    public class GameplayCueSystem : AppSystem
    {
        private VFXSystem vfxSystem;
        private AudioSystem audioSystem;
        private GameplayCueSystemSettings settings;

        public override void OnAttach(AppManager appManager)
        {
            vfxSystem = appManager.GetSystem<VFXSystem>();
            audioSystem = appManager.GetSystem<AudioSystem>();

            var resSys = appManager.GetSystem<ResourcesSystem>();
            if (!resSys.TryGetResource("GameplayCueSystemSettings", out settings))
            {
                throw new Exception("GameplayCueSystemSettings not found in ResourcesSystem.");
            }
        }

        public void ExecuteCue(GameplayTag cueTag, Vector3 position)
        {
            if (!settings.TryGetCueData(cueTag, out var cueData))
            {
                return;
            }

            if (cueData.HasVFXPath)
            {
                var vfx = vfxSystem.SpawnVFX(cueData.VFXPath);
                vfx.transform.position = position;
            }

            if (cueData.HasAudioPath)
            {
                audioSystem.PlaySFX(cueData.AudioPath);
            }
        }

        public override Type[] GetDependencySystemTypes()
        {
            return new Type[]
            {
                typeof(VFXSystem),
                typeof(AudioSystem),
                typeof(ResourcesSystem),
                typeof(GameplayTagSystem),
            };
        }
    }
}