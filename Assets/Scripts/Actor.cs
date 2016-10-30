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

    [Header("Animation")]
    public Animator animator;

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
    }

    protected virtual void InitializeOnAwake() { }

    public virtual void RecieveDamage(Transform source, float damage)
    {
        currentStats.health -= damage;
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

    public void UpdateAnimation(InputActions actionsToAnimate)
    {
        if(animator != null)
        {
            if (actionsToAnimate.primaryDirection != Vector3.zero)
            {
                animator.SetFloat("x", actionsToAnimate.primaryDirection.x);
                animator.SetFloat("y", actionsToAnimate.primaryDirection.y);
            }
        }
    }

    public class ActorState
    {
        public delegate void State();
        public State Enter;
        public State Update;
        public State Exit;
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