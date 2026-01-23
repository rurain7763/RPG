using System;
using UnityEngine;

namespace Experimental
{
    [CreateAssetMenu(fileName = "NewGameplayTag", menuName = "Common/Experimental/GameplayTag")]
    public class GameplayTag : ScriptableObject
    {
        [TextArea] public string Comment;
        [NonSerialized] public GameplayTag ParentTag;
    }
}