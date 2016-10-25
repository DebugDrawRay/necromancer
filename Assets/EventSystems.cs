public static class Events
{
    public delegate void BaseEvent<T>(object sender, T e);
    public static event BaseEvent<AttackMessage> Attacked;

    public static void TriggerAttacked(object sender, AttackMessage e)
    {
        if(Attacked != null)
        {
            Attacked(sender, e);
        }
    }
}
public class AttackMessage
{
    public Actor aggressor;
    public AttackMessage(Actor aggressor)
    {
        this.aggressor = aggressor;
    }
}
