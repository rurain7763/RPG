using System.Collections;
using TMPro;
using UnityEngine;

public class Waypoint : MonoBehaviour, IInteractable
{
    [SerializeField, Reference("Text_Label")] private TMP_Text label;
    [SerializeField] private LevelID levelID;

    public void Interact(Player player)
    {
        RPG.LoadLevel(levelID, new CheckpointSpawnPolicy());
    }

    private void OnValidate()
    {
        if (label != null)
        {
            label.text = Local.GetLevelName(levelID);
        }
    }
}