using UnityEngine;

public class Level_2 : RPGLevel
{
    private HUDUI hud;

    public override void Enter()
    {
        base.Enter();

        Environment.SetCamera(player.CenterAnchor);
        Environment.SetBackground();

        hud = RPG.UISys.OpenStatic<HUDUI>();
        hud.Setup(player);

        PlayDefaultBGM();
    }

    public override void Execute(float deltaTime)
    {
        base.Execute(deltaTime);

        HandleDevInput();
        HandleUIInputs();
    }

    public override void Exit()
    {
        base.Exit();

        RPG.UISys.CloseAllStatics();
        RPG.UISys.CloseAllPopups();
    }
}