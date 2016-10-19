using UnityEngine;
using System.Collections.Generic;

public class Skeleton : Actor 
{
    public RaiseSkeleton necro;

    [Header("Positioning")]
    public float followDistance;
    public bool withinNecroRange
    {
        get
        {
            return Vector3.Distance(transform.position, Necromancer.instance.transform.position) <= followDistance;
        }
    }
    bool withinGroupRange
    {
        get
        {
            for (int i = 0; i < necro.activeSkeletons.Count; i++)
            {
                Skeleton skel = necro.activeSkeletons[i];
                if (Vector3.Distance(transform.position, skel.transform.position) <= followDistance && skel.withinNecroRange)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Header("Combat")]
    public string validTargets;
    public LayerMask targetLayers;

    public float seekDistance;
    public float attackRange;
    public float attackInterval;
    private float currentInterval;
    public bool withinAttackRange
    {
        get
        {
            return Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRange;
        }
    }

    private Transform currentTarget;
    private Actor enemyTarget
    {
        get
        {
            if(currentTarget.GetComponent<Actor>())
            {
                return currentTarget.GetComponent<Actor>();
            }
            return null;
        }
    }
    private Transform previousTarget;

    public enum States
    {
        FollowLeader,
        AttackTarget,
        Idle,
        MoveToLocation,
        Follow,
        Attack
    }
    public States currentState;

    //new shit
    public float checkTargetRadius;
    public float atPositionRadius;

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
        currentInterval = attackInterval;
    }

    void Update()
    {
        RunStates();
        bus.Action(actions);
    }

    void Follow(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = direction.z;
        actions.primaryDirection = direction.normalized;
    }

    void Attack(Actor target)
    {

        if (currentInterval > 0)
        {
            currentInterval -= Time.deltaTime;
        }
        else
        {
            currentInterval = attackInterval;
        }
    }

    void ChangeLocation(Vector3 position)
    {
        List<Actor> enemies = new List<Actor>();
        Necromancer necro = null;

        Collider[] hits = Physics.OverlapSphere(position, checkTargetRadius, targetLayers);
        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].tag == validTargets)
            {
                enemies.Add(hits[i].GetComponent<Actor>());
            }

            if(hits[i].GetComponent<Necromancer>())
            {
                necro = hits[i].GetComponent<Necromancer>();
            }
        }

        if(enemies.Count > 0)
        {
            Actor closest = null;
            float closestRange = checkTargetRadius;
            for(int i = 0; i < enemies.Count; i++)
            {
                Actor select = enemies[i];
                float dist = Vector3.Distance(position, select.transform.position);
                if(dist <= closestRange)
                {
                    closest = select;
                    closestRange = dist;
                }
            }
            currentTarget = closest.transform;
            if (previousTarget != currentTarget)
            {
                previousTarget = currentTarget;
                currentState = States.Follow;
            }
        }
        else
        {

        }
    }

    void RunStates()
    {
        switch(currentState)
        {
            case States.Idle:
                break;
            case States.Attack:
                break;
            case States.MoveToLocation:
                break;
        }
    }
}
