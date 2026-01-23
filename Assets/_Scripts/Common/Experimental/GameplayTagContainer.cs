using System;
using System.Collections.Generic;
using UnityEngine;

namespace Experimental
{
    [Serializable]
    public class GameplayTagContainer : HashSet<GameplayTag>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<GameplayTag> serializedTags = new();

        public void Add(GameplayTagContainer other)
        {
            foreach (var tag in other)
            {
                Add(tag);
            }
        }

        public void Remove(GameplayTagContainer other)
        {
            foreach (var tag in other)
            {
                Remove(tag);
            }
        }

        public bool Has(GameplayTag tag)
        {
            GameplayTag currentTag = tag;

            while (currentTag != null)
            {
                if (Contains(currentTag))
                {
                    return true;
                }

                currentTag = currentTag.ParentTag;
            }

            return false;
        }

        public bool All(GameplayTagContainer other)
        {
            foreach (var tag in other)
            {
                if (!Has(tag))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Any(GameplayTagContainer other)
        {
            foreach (var tag in other)
            {
                if (Has(tag))
                {
                    return true;
                }
            }
            return false;
        }

        public void OnBeforeSerialize()
        {
            serializedTags.Clear();
            serializedTags.AddRange(this);
        }

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (var tag in serializedTags)
            {
                if (tag != null)
                {
                    Add(tag);
                }
            }
        }

        public static explicit operator GameplayTagContainer(GameplayTag tag)
        {
            var container = new GameplayTagContainer();
            container.Add(tag);
            return container;
        }
    }
}