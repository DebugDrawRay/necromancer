using UnityEngine;

public static class SkeletonEvent 
{
    public delegate void SendActorData(ActorData d);
    private static event SendActorData Attack;

    public static void TriggerAttack(ActorData d)
    {
        if(Attack != null)
        {
            Attack(d);
        }
    }
    public static void SubscribeAttack(SendActorData del)
    {
        Attack += del;
    }
}

public class ActorData
{
    public Actor source;
    public Actor target;

    public ActorData(Actor source, Actor target)
    {
        this.source = source;
        this.target = target;
    }
}