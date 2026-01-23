using System;
using System.Collections.Generic;

namespace Experimental
{
    public class GameplayTagSystem : AppSystem
    {
        private Dictionary<string, GameplayTag> gameplayTagCache = new();

        public override void OnAttach(AppManager appManager)
        {
            var resourcesSystem = appManager.GetSystem<ResourcesSystem>();

            resourcesSystem.EachAllScriptableObject<GameplayTag>((gameplayTag) => gameplayTagCache[gameplayTag.name] = gameplayTag);

            foreach (var tag in gameplayTagCache.Values)
            {
                SetParent(tag);
            }
        }

        private void SetParent(GameplayTag tag)
        {
            var pathSegments = tag.name.Split('.');

            if (pathSegments.Length == 1)
            {
                tag.ParentTag = null;
            }
            else
            {
                var parentTagName = string.Join('.', pathSegments, 0, pathSegments.Length - 1);
                if (gameplayTagCache.TryGetValue(parentTagName, out var parentTag))
                {
                    tag.ParentTag = parentTag;
                }
                else
                {
                    Logger.Warn($"Parent tag '{parentTagName}' for tag '{tag.name}' not found.");
                    tag.ParentTag = null;
                }
            }
        }

        public GameplayTag GetGameplayTag(string tagName)
        {
            gameplayTagCache.TryGetValue(tagName, out var tag);
            return tag;
        }

        public override Type[] GetDependencySystemTypes()
        {
            return new Type[]
            {
                typeof(ResourcesSystem),
            };
        }
    }
}
