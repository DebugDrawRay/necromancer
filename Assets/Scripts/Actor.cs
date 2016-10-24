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

    protected Actor lastAggressor;

    void Awake()
    {
        actions = new InputActions();
        bus = GetComponent<InputBus>();

        currentStats = (ActorStats)baseStats.Clone();

        actions.stats = currentStats;

        InitializeOnAwake();
    }

    protected virtual void InitializeOnAwake() { }

    public virtual void RecieveDamage(Actor source)
    {
        currentStats.health -= source.currentStats.strength;
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