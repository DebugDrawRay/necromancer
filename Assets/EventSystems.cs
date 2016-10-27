public static class Events
{
    public delegate void BaseEvent<T>(UnityEngine.Transform sender, T e);
    private static event BaseEvent<AttackMessage> Attack;

    public static void TriggerAttack(UnityEngine.Transform sender, AttackMessage e)
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
    public UnityEngine.Transform source;
    public AttackMessage(UnityEngine.Transform source)
    {
        this.source = source;
    }
}
