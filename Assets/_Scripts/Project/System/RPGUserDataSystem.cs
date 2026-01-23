using System;
using UnityEngine;

public class RPGUserDataSystem : UserDataSystem
{
    private RPGAppDataSystem appDataSystem;

    public string UserDataPath { get; private set; }
    public UserPlayDataTable PlayData { get; private set; }

    public event Action OnBeforeSaveUserDatas;

    public override void OnAttach(AppManager appManager)
    {
        base.OnAttach(appManager);

        appDataSystem = appManager.GetSystem<RPGAppDataSystem>();

        UserDataPath = Application.persistentDataPath + "/UserData/";

        PlayData = AddTable<UserPlayDataTable>(appDataSystem, UserDataPath);

        UpdateTables<UserPlayDataTable>();
    }

    public void ClearAllUserData()
    {
        ClearTables<UserPlayDataTable>();
    }

    public void SaveAllUserData()
    {
        OnBeforeSaveUserDatas?.Invoke();
        UploadTables<UserPlayDataTable>();
    }

    private void OnApplicationQuit()
    {
        SaveAllUserData();
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[]
        {
            typeof(RPGAppDataSystem),
        };
    }
}