public static class Events
{
    public delegate void BaseEvent<T>(object sender, T e);
    private static event BaseEvent<AttackMessage> Attack;

    public static void TriggerAttack(object sender, AttackMessage e)
    {
        if(Attack != null)
        {
            Attack(sender, e);
        }
    }

    public static void SubscribeAttack(BaseEvent<AttackMessage> e)
    {
        Attack += e;
    }
}
public class AttackMessage
{
    public Actor target;
    public float damage;
    public AttackMessage(Actor target, float damage)
    {
        this.target = target;
        this.damage = damage;
    }
}
