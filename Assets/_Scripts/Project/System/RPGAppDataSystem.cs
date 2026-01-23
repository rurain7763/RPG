using System;
using UnityEngine;

public class RPGAppDataSystem : AppDataSystem
{
    public RPGAppData AppData { get; private set; }

    private ResourcesSystem resourcesSystem;

    public override void OnAttach(AppManager appManager)
    {
        base.OnAttach(appManager);

        resourcesSystem = appManager.GetSystem<ResourcesSystem>();

        AppData = AddTable<RPGAppData>(resourcesSystem);

        UpdateTables<RPGAppData>();
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[]
        {
            typeof(ResourcesSystem),
        };
    }
}