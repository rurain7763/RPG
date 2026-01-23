using UnityEngine;

[CreateAssetMenu(fileName = "ResourcesSystemSetting", menuName = "Settings/ResourcesSystemSetting")]
public class ResourcesSystemSetting : ScriptableObject
{
    public string audioBasePath;
    public string textureBasePath;
    public string prefabBasePath;
    public string scriptableObjectBasePath;
}
