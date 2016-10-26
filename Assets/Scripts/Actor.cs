using UnityEngine;
using System;

[RequireComponent(typeof(InputBus))]
public class Actor : MonoBehaviour 
{
    [Header("Status")]
    [SerializeField]
    public ActorStats baseStats;
    protected ActorStats currentStats;

    //Input
    protected InputActions actions;
    protected InputBus bus;

    void Awake()
    {
        actions = new InputActions();
        bus = GetComponent<InputBus>();

        currentStats = (ActorStats)baseStats.Clone();

        actions.stats = currentStats;

        SubscribeToEvents();
        InitializeOnAwake();
    }

    void SubscribeToEvents()
    {
        Events.SubscribeAttack(RecieveDamage);
    }

    protected virtual void InitializeOnAwake() { }

    public virtual void RecieveDamage(object source, AttackMessage e)
    {
        currentStats.health -= e.damage;
    }

    public void UpdateBaseStats()
    {
        baseStats = (ActorStats)currentStats.Clone();
    }

    public void UpdateBaseStats(ActorStats newStats)
    {
        baseStats = newStats;
        currentStats = (ActorStats)baseStats.Clone();
    }
}

[System.Serializable]
public class ActorStats : ICloneable
{
    public float health;
    public float mana;
    public float strength;
    public float speed;

    //Temp, may change
    public int maxSkeletons;

    public object Clone()
    {
        return MemberwiseClone();
    }
}