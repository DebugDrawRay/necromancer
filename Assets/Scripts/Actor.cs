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

        InitializeOnAwake();
    }

    protected virtual void InitializeOnAwake() { }

    public void DealDamage(float damage)
    {
        currentStats.health -= damage;
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
        return this.MemberwiseClone();
    }
}