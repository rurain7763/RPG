public class EnemyKilledEvent
{
    public readonly Entity enemy;

    public EnemyKilledEvent(Entity enemy)
    {
        this.enemy = enemy;
    }
}

